using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using GainsLab.Contracts.Interface;
using GainsLab.Contracts.SyncDto;
using GainsLab.Contracts.SyncService.Mapper;
using GainsLab.Core.Models.Core;
using GainsLab.Core.Models.Core.Interfaces.DB;
using GainsLab.Infrastructure.DB.Context;
using GainsLab.Infrastructure.DB.DTOs;
using Microsoft.EntityFrameworkCore;

namespace GainsLab.Contracts.SyncService;

/// <summary>
/// Provides pull/push synchronization for muscles, keeping descriptors and antagonist links in sync.
/// </summary>
public class MuscleSyncService : ISyncService<MuscleSyncDTO>
{
    private readonly GainLabPgDBContext _db;
    private readonly Core.Models.Core.Utilities.Logging.ILogger _log;
    private const string SyncActor = "sync";
    private static readonly Guid PlaceholderDescriptorGuid = Guid.Empty;
    private readonly Dictionary<Guid, DescriptorDTO> _descriptorCache = new();

    public MuscleSyncService(GainLabPgDBContext db, Core.Models.Core.Utilities.Logging.ILogger log)
    {
        _db = db;
        _log = log;
    }

    /// <summary>
    /// Gets the entity type handled by this service.
    /// </summary>
    public EntityType EntityType => EntityType.Muscle;
    /// <summary>
    /// Gets the DTO type used for serialization.
    /// </summary>
    public Type DtoType => typeof(MuscleSyncDTO);

    Task<PushResult> ISyncService.PushBoxedAsync(IEnumerable<ISyncDto> dtos, CancellationToken ct)
        => PushAsync(dtos.Cast<MuscleSyncDTO>(), ct);

    async Task<object> ISyncService.PullBoxedAsync(SyncCursor cur, int take, CancellationToken ct)
        => await PullAsync(cur, take, ct);

    /// <summary>
    /// Retrieves a deterministic page of muscle DTOs newer than the supplied cursor.
    /// </summary>
    public async Task<SyncPage<MuscleSyncDTO>> PullAsync(SyncCursor cur, int take, CancellationToken ct)
    {
        var serverTime = DateTimeOffset.UtcNow;
        take = Math.Clamp(take, 1, 500);

        var muscles = await _db.Muscles.AsNoTracking()
            .Include(m => m.Descriptor)
            .Where(m => m.UpdatedAtUtc > cur.Ts
                        || (m.UpdatedAtUtc == cur.Ts && m.UpdatedSeq > cur.Seq))
            .OrderBy(m => m.UpdatedAtUtc)
            .ThenBy(m => m.UpdatedSeq)
            .Take(take)
            .ToListAsync(ct);

        var antagonistMap = await LoadAntagonistsAsync(muscles.Select(m => m.Id).ToArray(), ct);

        var items = muscles
            .Select(m => MuscleSyncMapper.ToSyncDTO(m,
                antagonistMap.TryGetValue(m.Id, out var ids) ? ids : null))
            .ToList();

        SyncCursor? next = items.Count < take
            ? null
            : new SyncCursor(items[^1].UpdatedAtUtc, items[^1].UpdatedSeq);

        return new SyncPage<MuscleSyncDTO>(serverTime, next, items);
    }

    /// <summary>
    /// Applies incoming muscle mutations, including descriptor resolution and antagonist maintenance.
    /// </summary>
    public async Task<PushResult> PushAsync(IEnumerable<MuscleSyncDTO> items, CancellationToken ct)
    {
        _descriptorCache.Clear();

        var now = DateTimeOffset.UtcNow;
        var preValidationResults = new List<PushItemResult>();
        var preValidationFailures = 0;
        var payloads = new List<(MuscleDTO Entity, IReadOnlyList<Guid> Antagonists)>();

        foreach (var dto in items)
        {
            if (dto.GUID == Guid.Empty)
            {
                const string reason = "Muscle GUID is required.";
                _log.LogError(nameof(MuscleSyncService), reason);
                preValidationResults.Add(new PushItemResult(Guid.Empty, PushItemStatus.Failed, reason));
                preValidationFailures++;
                continue;
            }

            var descriptor = GetDescriptorDto(dto.DescriptorGUID);
            var entity = MuscleSyncMapper.FromSyncDTO(dto, descriptor, SyncActor);
            var antagonists = (dto.IsDeleted ? Array.Empty<Guid>() : dto.AntagonistGuids ?? Array.Empty<Guid>())
                .Where(g => g != Guid.Empty)
                .Distinct()
                .ToArray();
            payloads.Add((entity, antagonists));
        }

        var executionStrategy = _db.Database.CreateExecutionStrategy();
        PushResult? finalResult = null;

        try
        {
            await executionStrategy.ExecuteAsync(async () =>
            {
                var txResults = new List<PushItemResult>();
                var accepted = 0;
                var failed = 0;
                var pendingAntagonists = new List<(MuscleDTO Entity, IReadOnlyList<Guid> Antagonists)>();

                await using var tx = await _db.Database.BeginTransactionAsync(ct);
                try
                {
                    foreach (var (entity, antagonists) in payloads)
                    {
                        try
                        {
                            var existing = await _db.Muscles
                                .Include(m => m.Descriptor)
                                .SingleOrDefaultAsync(m => m.GUID == entity.GUID, ct);

                            if (existing is null)
                            {
                                if (entity.IsDeleted)
                                {
                                    txResults.Add(new PushItemResult(entity.Iguid, PushItemStatus.NotFound,
                                        "Delete ignored for missing row."));
                                    continue;
                                }

                                entity.UpdatedAtUtc = now;
                                entity.UpdatedSeq = await NextUpdateSeqAsync(ct);
                                entity.UpdatedBy = SyncActor;

                                await _db.Muscles.AddAsync(entity, ct);
                                pendingAntagonists.Add((entity, antagonists));
                                txResults.Add(new PushItemResult(entity.Iguid, PushItemStatus.Upserted));
                                accepted++;
                                continue;
                            }

                            if (existing.Authority == DataAuthority.Upstream)
                            {
                                txResults.Add(new PushItemResult(entity.Iguid, PushItemStatus.Failed,
                                    "Upstream-owned muscle cannot be modified downstream."));
                                continue;
                            }

                            var isIncomingNewer =
                                entity.UpdatedAtUtc > existing.UpdatedAtUtc ||
                                (entity.UpdatedAtUtc == existing.UpdatedAtUtc &&
                                 entity.UpdatedSeq > existing.UpdatedSeq);

                            if (!isIncomingNewer)
                            {
                                txResults.Add(new PushItemResult(entity.Iguid,
                                    PushItemStatus.SkippedDuplicate, "Incoming not newer."));
                                continue;
                            }

                            if (entity.IsDeleted)
                            {
                                existing.IsDeleted = true;
                            }
                            else
                            {
                                existing.Name = entity.Name;
                                existing.BodySection = entity.BodySection;
                                existing.Descriptor = entity.Descriptor;
                                existing.IsDeleted = false;
                            }

                            existing.UpdatedAtUtc = now;
                            existing.UpdatedSeq = await NextUpdateSeqAsync(ct);
                            existing.UpdatedBy = SyncActor;

                            _db.Muscles.Update(existing);
                            pendingAntagonists.Add((existing, antagonists));

                            txResults.Add(new PushItemResult(entity.Iguid,
                                entity.IsDeleted ? PushItemStatus.Deleted : PushItemStatus.Upserted));
                            accepted++;
                        }
                        catch (DbUpdateException ex)
                        {
                            failed++;
                            _log.LogError(nameof(MuscleSyncService),
                                $"Push item {entity.Iguid} failed: {ex.GetBaseException().Message}");
                            txResults.Add(new PushItemResult(entity.Iguid, PushItemStatus.Failed,
                                ex.GetBaseException().Message));
                        }
                        catch (Exception ex)
                        {
                            failed++;
                            _log.LogError(nameof(MuscleSyncService),
                                $"Push item {entity.Iguid} failed: {ex.Message}");
                            txResults.Add(new PushItemResult(entity.Iguid, PushItemStatus.Failed, ex.Message));
                        }
                    }

                    await _db.SaveChangesAsync(ct);

                    foreach (var (muscle, antagonists) in pendingAntagonists)
                    {
                        await SyncAntagonistsAsync(muscle, antagonists, ct);
                    }

                    await _db.SaveChangesAsync(ct);
                    await tx.CommitAsync(ct);

                    finalResult = new PushResult(
                        now,
                        accepted,
                        preValidationFailures + failed,
                        preValidationResults.Concat(txResults).ToList());
                }
                catch
                {
                    await tx.RollbackAsync(ct);
                    throw;
                }
            });
        }
        catch (Exception ex)
        {
            _log.LogError(nameof(MuscleSyncService), $"Push batch failed/rolled back: {ex}");
            var transactionalFailures = payloads
                .Select(p => new PushItemResult(p.Entity.Iguid, PushItemStatus.Failed, "Batch rolled back."))
                .ToList();

            var combined = preValidationResults.Concat(transactionalFailures).ToList();
            var totalFailed = preValidationFailures + payloads.Count;
            return new PushResult(now, 0, totalFailed, combined);
        }

        return finalResult ?? new PushResult(now, 0, preValidationFailures, preValidationResults);
    }

    /// <summary>
    /// Loads existing antagonist GUIDs for the supplied muscle ids in a single query.
    /// </summary>
    private async Task<Dictionary<int, IReadOnlyList<Guid>>> LoadAntagonistsAsync(int[] muscleIds, CancellationToken ct)
    {
        if (muscleIds.Length == 0)
            return new Dictionary<int, IReadOnlyList<Guid>>();

        var antagonists = await _db.MuscleAntagonists.AsNoTracking()
            .Where(link => muscleIds.Contains(link.MuscleId))
            .Include(link => link.Antagonist)
            .Select(link => new { link.MuscleId, link.Antagonist.GUID })
            .ToListAsync(ct);

        return antagonists
            .GroupBy(x => x.MuscleId)
            .ToDictionary(
                g => g.Key,
                g => (IReadOnlyList<Guid>)g
                    .Where(x => x.GUID != Guid.Empty)
                    .Select(x => x.GUID)
                    .Distinct()
                    .ToList());
    }

    /// <summary>
    /// Syncs the muscle_antagonists join rows to match the provided GUID list.
    /// </summary>
    private async Task SyncAntagonistsAsync(MuscleDTO muscle, IReadOnlyList<Guid>? desiredGuids, CancellationToken ct)
    {
        if (muscle.Id == 0)
        {
            _log.LogWarning(nameof(MuscleSyncService),
                $"Cannot sync antagonists for {muscle.GUID} because the row has no ID yet.");
            return;
        }

        desiredGuids ??= Array.Empty<Guid>();
        var desiredSet = desiredGuids.Where(g => g != Guid.Empty).ToHashSet();

        var existingLinks = await _db.MuscleAntagonists
            .Where(link => link.MuscleId == muscle.Id)
            .Include(link => link.Antagonist)
            .ToListAsync(ct);

        var toRemove = existingLinks
            .Where(link => link.Antagonist == null || !desiredSet.Contains(link.Antagonist.GUID))
            .ToList();

        if (toRemove.Count > 0)
        {
            _db.MuscleAntagonists.RemoveRange(toRemove);
        }

        var existingGuids = existingLinks
            .Where(link => link.Antagonist != null)
            .Select(link => link.Antagonist!.GUID)
            .ToHashSet();

        var missingGuids = desiredSet.Except(existingGuids).ToList();
        if (missingGuids.Count == 0)
            return;

        var related = await _db.Muscles
            .Where(m => missingGuids.Contains(m.GUID))
            .Select(m => new { m.GUID, m.Id })
            .ToListAsync(ct);
        var lookup = related.ToDictionary(x => x.GUID, x => x.Id);

        foreach (var guid in missingGuids)
        {
            if (!lookup.TryGetValue(guid, out var antagonistId))
            {
                _log.LogWarning(nameof(MuscleSyncService),
                    $"Antagonist {guid} missing. Relationship for muscle {muscle.GUID} skipped.");
                continue;
            }

            _db.MuscleAntagonists.Add(new MuscleAntagonistDTO
            {
                MuscleId = muscle.Id,
                AntagonistId = antagonistId
            });
        }
    }

    /// <summary>
    /// Resolves a descriptor for the provided GUID, creating placeholders when necessary.
    /// </summary>
    private DescriptorDTO GetDescriptorDto(Guid? descriptorGuid)
    {
        var cacheKey = descriptorGuid ?? PlaceholderDescriptorGuid;
        if (_descriptorCache.TryGetValue(cacheKey, out var cached))
        {
            return cached;
        }

        DescriptorDTO descriptor;
        var missingDescriptor = !descriptorGuid.HasValue || descriptorGuid.Value == Guid.Empty;

        if (missingDescriptor)
        {
            descriptor = ResolvePlaceholderDescriptor();
        }
        else
        {
            var guid = descriptorGuid.Value;
            descriptor = TryGetTrackedDescriptor(guid)
                         ?? CreateDescriptorStub(guid, "none");

            if (descriptor.Id == 0)
            {
                _log.Log(nameof(MuscleSyncService),
                    $"Descriptor {descriptorGuid} not found. Creating stub so FK constraint is satisfied.");
            }
        }

        _descriptorCache[cacheKey] = descriptor;
        return descriptor;
    }

    /// <summary>
    /// Retrieves or creates the shared placeholder descriptor used when upstream omits descriptor data.
    /// </summary>
    private DescriptorDTO ResolvePlaceholderDescriptor()
    {
        var descriptor = TryGetTrackedDescriptor(PlaceholderDescriptorGuid);
        return descriptor ?? CreateDescriptorStub(PlaceholderDescriptorGuid, "unspecified");
    }

    /// <summary>
    /// Searches the current context for a descriptor that matches the provided GUID.
    /// </summary>
    private DescriptorDTO? TryGetTrackedDescriptor(Guid guid)
    {
        return _db.Descriptors.Local.FirstOrDefault(d => d.GUID == guid)
               ?? _db.Descriptors.FirstOrDefault(d => d.GUID == guid);
    }

    /// <summary>
    /// Creates an in-memory descriptor stub so foreign-key constraints can be satisfied.
    /// </summary>
    private DescriptorDTO CreateDescriptorStub(Guid guid, string content)
    {
        var now = DateTimeOffset.UtcNow;
        return new DescriptorDTO
        {
            GUID = guid,
            Content = content,
            CreatedAtUtc = now,
            UpdatedAtUtc = now,
            CreatedBy = SyncActor,
            UpdatedBy = SyncActor,
            IsDeleted = false,
            Authority = DataAuthority.Bidirectional
        };
    }

    /// <summary>
    /// Obtains the next global update sequence value from the database.
    /// </summary>
    private async Task<long> NextUpdateSeqAsync(CancellationToken ct)
    {
        await using var cmd = _db.Database.GetDbConnection().CreateCommand();
        cmd.CommandText = "SELECT nextval('update_seq')";
        if (cmd.Connection!.State != ConnectionState.Open)
            await cmd.Connection.OpenAsync(ct);
        var val = await cmd.ExecuteScalarAsync(ct);
        return Convert.ToInt64(val);
    }
}
