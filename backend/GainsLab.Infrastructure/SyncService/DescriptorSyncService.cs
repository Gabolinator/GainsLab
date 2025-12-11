using System.Data;
using GainsLab.Application.DTOs;
using GainsLab.Application.Results;
using GainsLab.Contracts.Interface;
using GainsLab.Contracts.SyncDto;
using GainsLab.Contracts.SyncService.Mapper;
using GainsLab.Domain;
using GainsLab.Domain.Interfaces;
using GainsLab.Infrastructure.DB.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace GainsLab.Infrastructure.SyncService;

/// <summary>
/// Provides read-side synchronization operations for descriptor entities stored in the PostgreSQL database.
/// </summary>
public class DescriptorSyncService : ISyncService<DescriptorSyncDTO>
{
    private readonly GainLabPgDBContext _db;
    private readonly ILogger _log;
    private const string SyncActor = "sync";
    
    /// <summary>
    /// Initializes a new instance of the <see cref="DescriptorSyncService"/> class.
    /// </summary>
    public DescriptorSyncService(GainLabPgDBContext db, ILogger log)
    {
        _db = db;
        _log = log;
    }

    /// <inheritdoc />
    public EntityType EntityType => EntityType.Descriptor;

    /// <summary>
    /// Gets the DTO type handled by this service.
    /// </summary>
    public Type DtoType => typeof(DescriptorSyncDTO);

    /// <inheritdoc />
    Task<PushResult> ISyncService.PushBoxedAsync(IEnumerable<ISyncDto> dtos, CancellationToken ct)
    {
        return PushAsync(dtos.Cast<DescriptorSyncDTO>(), ct);
    }

    /// <inheritdoc />
    async Task<object> ISyncService.PullBoxedAsync(SyncCursor cur, int take, CancellationToken ct)
        => await PullAsync(cur, take, ct); // returns SyncPage<DescriptorSyncDto>

    /// <summary>
    /// Retrieves a page of descriptor changes newer than the supplied cursor, ordered deterministically for incremental sync.
    /// </summary>
    /// <param name="cur">The cursor that represents the last successful sync position.</param>
    /// <param name="take">Maximum number of records to include in the page.</param>
    /// <param name="ct">Cancellation token propagated from the caller.</param>
    /// <returns>A <see cref="SyncPage{TSyncDto}"/> containing descriptor DTOs and the next cursor when available.</returns>
    public async Task<SyncPage<DescriptorSyncDTO>> PullAsync(SyncCursor cur, int take, CancellationToken ct)
    {
        var serverTime = DateTimeOffset.UtcNow;
        take = Math.Clamp(take, 1, 500);

        var q = _db.Descriptors.AsNoTracking()
            .Where(d => d.UpdatedAtUtc > cur.Ts
                        || (d.UpdatedAtUtc == cur.Ts && d.UpdatedSeq > cur.Seq))
            .OrderBy(d => d.UpdatedAtUtc)
            .ThenBy(d => d.UpdatedSeq)
            .Take(take);

        var items = await q.Select(d => new DescriptorSyncDTO(
            d.GUID,
            d.Content,
            d.UpdatedAtUtc,
            d.UpdatedSeq,
            d.IsDeleted,
            d.Authority
        )).ToListAsync(ct);

        _log.Log(nameof(DescriptorSyncService), $"Pull Descriptor Async- take {take} - items count: {items.Count} items[0] {(items.Count > 0 ? items[0] : "none")} ");

        SyncCursor? next = items.Count < take
            ? null
            : new SyncCursor(items[^1].UpdatedAtUtc, items[^1].UpdatedSeq);

        return new SyncPage<DescriptorSyncDTO>(serverTime, next, items);
    }

    /// <summary>
    /// Processes descriptor mutations pushed from clients.
    /// </summary>
    /// <param name="items">Incoming descriptor payloads.</param>
    /// <param name="ct">Cancellation token propagated from the caller.</param>
    /// <returns>A push result describing the outcome for each item.</returns>
   public async Task<PushResult> PushAsync(IEnumerable<DescriptorSyncDTO> items, CancellationToken ct)
{
    var now = DateTimeOffset.UtcNow;
    var payloads = items.Select(s => DescriptorSyncMapper.FromSyncDTO(s, SyncActor)).ToList();

    var strategy = _db.Database.CreateExecutionStrategy();

    return await strategy.ExecuteAsync(async () =>
    {
        var results = new List<PushItemResult>();
        var accepted = 0;
        var failed = 0;

        await using var tx = await _db.Database.BeginTransactionAsync(ct);
        try
        {
            foreach (var incoming in payloads)
            {
                try
                {
                    _log.Log(nameof(DescriptorSyncService), $"Push descriptor {incoming.Content} and {incoming.GUID}");
                    
                    
                    // Read-before-update by business key; do NOT use Equals()
                    var existing = await _db.Descriptors
                        .AsNoTracking()
                        .SingleOrDefaultAsync(d => d.GUID == incoming.GUID, ct);

                    if (existing is null)
                    {
                        if (incoming.IsDeleted)
                        {
                            results.Add(new PushItemResult(incoming.Iguid, PushItemStatus.NotFound,
                                "Delete ignored for missing row."));
                            continue;
                        }

                        // New insert: stamp on server
                        incoming.UpdatedAtUtc = now;
                        incoming.UpdatedSeq   = await NextUpdateSeqAsync(ct);

                        await _db.Descriptors.AddAsync(incoming, ct);
                        results.Add(new PushItemResult(incoming.Iguid, PushItemStatus.Upserted));
                        accepted++;
                    }
                    else
                    {
                        if (existing.Authority == DataAuthority.Upstream)
                        {
                            results.Add(new PushItemResult(incoming.Iguid, PushItemStatus.Failed,
                                "Upstream-owned descriptor cannot be modified downstream."));
                            continue;
                        }

                        // Order check (newer wins)
                        var isIncomingNewer =
                            incoming.UpdatedAtUtc > existing.UpdatedAtUtc ||
                            (incoming.UpdatedAtUtc == existing.UpdatedAtUtc &&
                             incoming.UpdatedSeq   > existing.UpdatedSeq);

                        if (!isIncomingNewer)
                        {
                            results.Add(new PushItemResult(incoming.Iguid, PushItemStatus.SkippedDuplicate,
                                "Incoming not newer."));
                            continue;
                        }

                        // Prepare a stub with the known PK so EF treats it as the existing row
                        var stub = new DescriptorRecord
                        {
                            Id        = existing.Id,   // PK
                            GUID      = existing.GUID, // keep business key
                            // Copy fields you want to preserve if not deleted:
                            Content   = incoming.IsDeleted ? existing.Content : incoming.Content,
                            IsDeleted = incoming.IsDeleted,
                            Authority = existing.Authority,

                            // Server stamps:
                            UpdatedAtUtc = now,
                            UpdatedSeq   = await NextUpdateSeqAsync(ct),

                            // Preserve immutable/original metadata (don’t mark them modified)
                            CreatedAtUtc = existing.CreatedAtUtc,
                            CreatedBy    = existing.CreatedBy,

                            // Concurrency token must carry the original value
                            RowVersion   = existing.RowVersion
                        };

                        // Attach + mark modified only for the columns we’re changing
                        _db.Attach(stub);

                        // Mark what changes:
                        var entry = _db.Entry(stub);
                        entry.Property(p => p.Content     ).IsModified = !incoming.IsDeleted;
                        entry.Property(p => p.IsDeleted   ).IsModified = true;
                        entry.Property(p => p.UpdatedAtUtc).IsModified = true;
                        entry.Property(p => p.UpdatedSeq  ).IsModified = true;

                        // Ensure we don't overwrite creation data
                        entry.Property(p => p.CreatedAtUtc).IsModified = false;
                        entry.Property(p => p.CreatedBy   ).IsModified = false;

                        // Make RowVersion participate in concurrency check
                        entry.Property(p => p.RowVersion).OriginalValue = existing.RowVersion!;
                        entry.Property(p => p.RowVersion).IsModified    = false; // server/database will update it

                        results.Add(new PushItemResult(incoming.Iguid,
                            incoming.IsDeleted ? PushItemStatus.Deleted : PushItemStatus.Upserted));
                        accepted++;
                    }
                }
                catch (DbUpdateConcurrencyException cx)
                {
                    _log.LogError(nameof(DescriptorSyncService),
                        $"Concurrency conflict for {incoming.Iguid}: {cx.GetBaseException().Message}");
                    results.Add(new PushItemResult(incoming.Iguid, PushItemStatus.Failed, "Concurrency conflict."));
                    failed++;
                }
                catch (DbUpdateException ex)
                {
                    _log.LogError(nameof(DescriptorSyncService),
                        $"Push item {incoming.Iguid} failed: {ex.GetBaseException().Message}");
                    results.Add(new PushItemResult(incoming.Iguid, PushItemStatus.Failed,
                        ex.GetBaseException().Message));
                    failed++;
                }
                catch (Exception ex)
                {
                    _log.LogError(nameof(DescriptorSyncService), $"Push item {incoming.Iguid} failed: {ex}");
                    results.Add(new PushItemResult(incoming.Iguid, PushItemStatus.Failed, ex.Message));
                    failed++;
                }
            }

            await _db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);

            return new PushResult(now, accepted, failed, results);
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync(ct);
            _log.LogError(nameof(DescriptorSyncService), $"Push batch failed/rolled back: {ex}");

            var allFailed = payloads.Select(p =>
                new PushItemResult(p.Iguid, PushItemStatus.Failed, "Batch rolled back.")).ToList();

            return new PushResult(now, 0, payloads.Count, allFailed);
        }
    });
}
    
    /// <summary>
    /// Obtains the next sequence number used to order equipment updates.
    /// </summary>
    /// <param name="ct">Cancellation token propagated from the caller.</param>
    private async Task<long> NextUpdateSeqAsync(CancellationToken ct)
    {
        // Postgres: SELECT nextval('update_seq');
        await using var cmd = _db.Database.GetDbConnection().CreateCommand();
        cmd.CommandText = "SELECT nextval('update_seq')";
        if (cmd.Connection!.State != ConnectionState.Open)
            await cmd.Connection.OpenAsync(ct);
        var val = await cmd.ExecuteScalarAsync(ct);
        return Convert.ToInt64(val);
    }


    public async Task<Result<DescriptorSyncDTO>> PullById(Guid id,CancellationToken ct = default)
    {
        var entity = await _db.Descriptors.AsNoTracking().FirstOrDefaultAsync(d=> d.GUID == id,ct);
        
        if (entity is null) return Result<DescriptorSyncDTO>.Failure("Descriptor not found");
        if(entity.IsDeleted) return Result<DescriptorSyncDTO>.Failure("Descriptor is deleted");
        
        var dto= new DescriptorSyncDTO(
            entity.GUID,
            entity.Content,
            entity.UpdatedAtUtc,
            entity.UpdatedSeq,
            entity.IsDeleted,
            entity.Authority);
        
        return Result<DescriptorSyncDTO>.SuccessResult(dto);
        
    }
    
}
