using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GainsLab.Contracts.Outbox;
using GainsLab.Contracts.SyncService;
using GainsLab.Core.Models.Core;
using GainsLab.Core.Models.Core.Interfaces.DataManagement;
using GainsLab.Core.Models.Core.Interfaces.DB;
using GainsLab.Core.Models.Core.Results;
using GainsLab.Core.Models.Core.Utilities;
using GainsLab.Core.Models.Core.Utilities.Logging;
using GainsLab.Infrastructure.DB;
using GainsLab.Infrastructure.DB.Outbox;

namespace GainsLab.Models.DataManagement.Sync;

/// <summary>
/// Coordinates remote ⇄ local synchronization by invoking entity processors per type.
/// </summary>
public interface ISyncOrchestrator
{
    /// <summary>
    /// Forces a full re-sync for every registered entity, ignoring saved cursors.
    /// </summary>
    Task<Result<SeedOutcome>>  SeedAsync(CancellationToken ct = default);
    /// <summary>
    /// Pulls incremental changes for all entities using either provided or persisted cursors.
    /// </summary>
    Task<Result<DeltaOutcome>> PullDeltasAsync(
        IReadOnlyDictionary<string, string> cursors,
        CancellationToken ct = default);
    
    /// <summary>
    /// Runs an incremental pull for every registered entity using stored cursors.
    /// </summary>
    Task<Result> SyncDownAsync(CancellationToken ct = default); // pull remote → local
    /// <summary>
    /// Dispatches local outbox mutations to the server.
    /// </summary>
    Task<Result> SyncUpAsync(CancellationToken ct = default);   // push local → remote
}

/// <summary>
/// Default orchestration implementation backed by an <see cref="IRemoteProvider"/>.
/// </summary>
public sealed class SyncOrchestrator : ISyncOrchestrator
{
    private const int DefaultPageSize = 200;

    private readonly ILogger _logger;
    private readonly IRemoteProvider _remoteProvider;
    private readonly ILocalRepository _localRepository;
    private readonly ISyncCursorStore _cursorStore;
    private readonly IReadOnlyDictionary<EntityType, ISyncEntityProcessor> _processors;
    private readonly IOutboxDispatcher _outboxDispatcher;
    private readonly int _pageSize;

    public SyncOrchestrator(
        ILogger logger,
        IRemoteProvider remoteProvider,
        ILocalRepository localRepository,
        ISyncCursorStore cursorStore,
        IEnumerable<ISyncEntityProcessor> processors,
        IOutboxDispatcher outboxDispatcher,
        int pageSize = DefaultPageSize)
    {
        _logger = logger;
        _remoteProvider = remoteProvider;
        _localRepository = localRepository;
        _cursorStore = cursorStore;
        _outboxDispatcher = outboxDispatcher;
        _pageSize = Math.Clamp(pageSize, 1, 500);
        _processors = BuildProcessorMap(processors ?? Array.Empty<ISyncEntityProcessor>());
    }

    /// <inheritdoc />


 public async Task<Result<SeedOutcome>> SeedAsync(CancellationToken ct = default)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();
        var cursors = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var entitiesWritten = 0;

        var ordered = _processors.Keys
            .OrderBy(e => e.RankOf())
            .ThenBy(e => e.ToString());

        foreach (var entityType in ordered)
        {
            if (ct.IsCancellationRequested)
                return Result<SeedOutcome>.Failure("Seed cancelled");

            var (ok, count, lastCursor, error, more ) =
                await SyncEntityDownCoreAsync(entityType, startAt: SyncCursorUtil.MinValue, ct);

            if (!ok)
                return Result<SeedOutcome>.Failure(error ?? $"Seed failed for {entityType}");

            entitiesWritten += count;
            cursors[entityType.ToString()] = CursorToToken(lastCursor);
        }

        sw.Stop();
        var outcome = new SeedOutcome(
            SnapshotVersion: null,                 // plug in when your server exposes a head/etag
            Cursors: cursors,
            EntitiesWritten: entitiesWritten,
            Duration: sw.Elapsed
        );
        return Result<SeedOutcome>.SuccessResult(outcome);
    }
    
    public async Task<Result<DeltaOutcome>> PullDeltasAsync(
        IReadOnlyDictionary<string, string> providedCursors,
        CancellationToken ct = default)
    {
        var cursors = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var upserted = 0;
        var deleted  = 0;      // keep 0 unless you handle tombstones in ApplyAsync
        var hadMore  = false;

        var ordered = _processors.Keys
            .OrderBy(e => e.RankOf())
            .ThenBy(e => e.ToString());

        foreach (var entityType in ordered)
        {
            if (ct.IsCancellationRequested)
                return Result<DeltaOutcome>.Failure("Delta sync cancelled");

            // Choose starting cursor:
            ISyncCursor start;
            if (providedCursors.TryGetValue(entityType.ToString(), out var token) && !string.IsNullOrWhiteSpace(token))
            {
                start = ParseCursor(token) ?? SyncCursorUtil.MinValue;
            }
            else
            {
                start = await _cursorStore.GetCursorAsync(entityType, ct) ?? SyncCursorUtil.MinValue;
            }

            var (ok, count, lastCursor, error, more) =
                await SyncEntityDownCoreAsync(entityType, start, ct, computeHadMore: true);

            if (!ok)
                return Result<DeltaOutcome>.Failure(error ?? $"Delta sync failed for {entityType}");

            upserted += count;
            hadMore  |= more;
            cursors[entityType.ToString()] = CursorToToken(lastCursor);
        }

        var outcome = new DeltaOutcome(
            SnapshotVersion: null, // supply when your remote exposes a “head”
            Cursors: cursors,
            EntitiesUpserted: upserted,
            EntitiesDeleted: deleted,
            HadMore: hadMore
        );
        return Result<DeltaOutcome>.SuccessResult(outcome);
    }
    
    public Task<Result> SeedUntypedAsync(CancellationToken ct = default) =>
        RunForAllEntities(forceFullResync: true, ct);
    
    /// <inheritdoc />
    public Task<Result> SyncDownAsync(CancellationToken ct = default) =>
        RunForAllEntities(forceFullResync: false, ct);

    /// <inheritdoc />
    public Task<Result> SyncUpAsync(CancellationToken ct = default) =>
        _outboxDispatcher.DispatchAsync(ct);

    private IReadOnlyDictionary<EntityType, ISyncEntityProcessor> BuildProcessorMap(
        IEnumerable<ISyncEntityProcessor> processors)
    {
        var groups = processors
            .GroupBy(p => p.EntityType)
            .ToList();

        foreach (var dupes in groups.Where(g => g.Count() > 1))
        {
            _logger.LogWarning(nameof(SyncOrchestrator),
                $"Multiple processors registered for {dupes.Key}. Using the first registration.");
        }

        return groups.ToDictionary(g => g.Key, g => g.First());
    }
    
        // Returns: success, itemsProcessed, lastCursor, error, hadMore (optional)
    private async Task<(bool ok, int count, ISyncCursor last, string? error, bool hadMore)>
        SyncEntityDownCoreAsync(EntityType entityType, ISyncCursor startAt, CancellationToken ct, bool computeHadMore = false)
    {
        _logger.Log(nameof(SyncOrchestrator), $"Sync Entity Of Type : {entityType}");

        if (!_processors.TryGetValue(entityType, out var processor))
        {
            _logger.LogWarning(nameof(SyncOrchestrator), $"No processor registered for {entityType}");
            // nothing to do, preserve startAt as last
            return (true, 0, startAt, null, false);
        }

        var cursor = startAt;
        var processed = 0;
        var sawMore = false;

        while (!ct.IsCancellationRequested)
        {
            var pageResult = await _remoteProvider.PullAsync(entityType, cursor, _pageSize, ct);
            if (!pageResult.Success || pageResult.Value is null)
                return (false, processed, cursor, pageResult.GetErrorMessage(), sawMore);

            var page = pageResult.Value;

            // if provider signals “nothing,” stop
            if (page.ItemsList.Count == 0 && page.NextPage is null)
                break;

            // apply to local store
            var persistResult = await processor.ApplyAsync(page.ItemsList, _localRepository, ct);
            if (!persistResult.Success)
                return (false, processed, cursor, persistResult.GetErrorMessage(), sawMore);

            processed += page.ItemsList.Count;

            // advance + persist cursor
            cursor = page.NextPage ?? new SyncCursor(page.Time, cursor.ISeq);
            await _cursorStore.SaveCursorAsync(entityType, cursor, ct);

            if (page.NextPage is null)
                break;

            sawMore = computeHadMore || sawMore;
        }

        return (true, processed, cursor, null, sawMore);
    }

    // --- Cursor (de)serialization helpers ---
    private static string CursorToToken(ISyncCursor cursor)
        => SyncCursorUtil.ToToken(cursor); 

    private static ISyncCursor? ParseCursor(string token)
        => SyncCursorUtil.Parse(token);    

    

    /// <summary>
    /// Executes the synchronization workflow for every registered entity type.
    /// </summary>
    private async Task<Result> RunForAllEntities(bool forceFullResync, CancellationToken ct)
    {
        var ordered = _processors.Keys
            .OrderBy(e=> e.RankOf())
            .ThenBy(t => t.ToString()); // stable tie-breaker if multiple unknowns
        
        foreach (var entityType in ordered)
        {
            if (ct.IsCancellationRequested)
                return Result.Failure("Sync cancelled");
            
            var result = await SyncEntityDownAsync(entityType, forceFullResync, ct);
            if (!result.Success)
                return result;
        }

        return Result.SuccessResult();
    }

    /// <summary>
    /// Pulls remote changes for a specific entity type until the provider signals completion.
    /// </summary>
    public async Task<Result>  SyncEntityDownAsync(EntityType entityType, bool forceFullResync,  CancellationToken ct)
    {
        _logger.Log(nameof(SyncOrchestrator), $"Sync Entity Of Type : {entityType}");
        
        if (!_processors.TryGetValue(entityType, out var processor))
        {
            _logger.LogWarning(nameof(SyncOrchestrator), $"No processor registered for {entityType}");
            return Result.SuccessResult();
        }

        var cursor = forceFullResync
            ? SyncCursorUtil.MinValue
            : await _cursorStore.GetCursorAsync(entityType, ct) ?? SyncCursorUtil.MinValue;

        while (!ct.IsCancellationRequested)
        {
            
            var pageResult = await _remoteProvider.PullAsync(entityType, cursor, _pageSize, ct);
            if (!pageResult.Success || pageResult.Value is null)
                return Result.Failure(pageResult.GetErrorMessage());

            var page = pageResult.Value;
            if (page.ItemsList.Count == 0 && page.NextPage is null)
                break;

            var persistResult = await processor.ApplyAsync(page.ItemsList, _localRepository, ct);
            if (!persistResult.Success)
                return persistResult;

            cursor = page.NextPage ?? new SyncCursor(page.Time, cursor.ISeq);
            await _cursorStore.SaveCursorAsync(entityType, cursor, ct);

            if (page.NextPage is null)
                break;
        }

        return Result.SuccessResult();
    }
}
