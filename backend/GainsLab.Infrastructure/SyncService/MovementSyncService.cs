using System.Data;
using GainsLab.Application.DTOs;
using GainsLab.Contracts.Dtos.SyncDto;
using GainsLab.Contracts.Interface;
using GainsLab.Contracts.SyncService.Mapper;
using GainsLab.Domain;
using GainsLab.Domain.Interfaces;
using GainsLab.Infrastructure.DB.Context;
using Microsoft.EntityFrameworkCore;

namespace GainsLab.Infrastructure.SyncService;

public class MovementSyncService : ISyncService<MovementSyncDTO>
{
    private readonly GainLabPgDBContext _db;
    private readonly ILogger _log;
    private const string SyncActor = "sync";
    private static readonly Guid PlaceholderDescriptorGuid = Guid.Empty;
    private readonly Dictionary<Guid, DescriptorRecord> _descriptorCache = new();
    private readonly Dictionary<Guid, int> _categoryIdCache = new();
    private readonly Dictionary<Guid, int> _equipmentIdCache = new();
    private readonly Dictionary<Guid, int> _muscleIdCache = new();

    public MovementSyncService(GainLabPgDBContext db, ILogger log)
    {
        _db = db;
        _log = log;
    }

    public EntityType EntityType => EntityType.Movement;
    public Type DtoType => typeof(MovementSyncDTO);
    Task<PushResult> ISyncService.PushBoxedAsync(IEnumerable<ISyncDto> dtos, CancellationToken ct)
        => PushAsync(dtos.Cast<MovementSyncDTO>(), ct);

    async Task<object> ISyncService.PullBoxedAsync(SyncCursor cur, int take, CancellationToken ct)
        => await PullAsync(cur, take, ct);

    public async Task<SyncPage<MovementSyncDTO>> PullAsync(SyncCursor cur, int take, CancellationToken ct)
    {
        var serverTime = DateTimeOffset.UtcNow;
        take = Math.Clamp(take, 1, 500);

        var movements = await _db.Movement.AsNoTracking()
            .Include(m => m.Descriptor)
            .Where(m => m.UpdatedAtUtc > cur.Ts
                        || (m.UpdatedAtUtc == cur.Ts && m.UpdatedSeq > cur.Seq))
            .OrderBy(m => m.UpdatedAtUtc)
            .ThenBy(m => m.UpdatedSeq)
            .Take(take)
            .ToListAsync(ct);

        Dictionary<int , IReadOnlyList<Guid>> equipmentMap = await LoadEquipmentAsync(movements.Select(m => m.Id).ToArray(), ct);
        Dictionary<int, (IReadOnlyList<Guid> primaryMuscle, IReadOnlyList<Guid> secondaryMuscle)> muscleRelationMap = await LoadMuscleRelationAsync(movements.Select(m => m.Id).ToArray(), ct);

        var categoryDtos = await _db.MovementCategories.AsNoTracking().ToListAsync(ct);
        var categoryMap = categoryDtos.ToDictionary(x => x.Id, x => x.GUID);
     
        
        var items = movements
            .Select(m => 
                MovementSyncMapper.ToSyncDTO(m,
                    equipmentMap.TryGetValue(m.Id, out var equipmentIds) ? equipmentIds : null,
                    muscleRelationMap.TryGetValue(m.Id, out var musclesIds) ? musclesIds.primaryMuscle : null,
                    muscleRelationMap.TryGetValue(m.Id, out  musclesIds) ? musclesIds.secondaryMuscle : null,
                    categoryMap.TryGetValue(m.MovementCategoryId, out var category) ? category : null,
                    m.VariantOfMovementGuid 
                    )
                )
            .ToList();

        SyncCursor? next = items.Count < take
            ? null
            : new SyncCursor(items[^1].UpdatedAtUtc, items[^1].UpdatedSeq);

        return new SyncPage<MovementSyncDTO>(serverTime, next, items);
        
    }

    private async Task<Dictionary<int, (IReadOnlyList<Guid> primaryMuscle, IReadOnlyList<Guid> secondaryMuscle)>> LoadMuscleRelationAsync(int[] movementIds, CancellationToken ct)
    {
        if (movementIds.Length == 0)
            return new Dictionary<int, (IReadOnlyList<Guid> primaryMuscle, IReadOnlyList<Guid> secondaryMuscle)>();

        var muscleRelations = await _db.MovementMuscleRelations.AsNoTracking()
            .Where(link => movementIds.Contains(link.MovementId))
            .Include(link => link.Muscle)
            .Select(link => new { link.MovementId, link.Muscle.GUID, link.MuscleRole })
            .ToListAsync(ct);

        return muscleRelations
            .GroupBy(x => x.MovementId)
            .ToDictionary(
                g => g.Key,
                g =>
                {
                    var primary = g
                        .Where(x => x.GUID != Guid.Empty && x.MuscleRole == MuscleRole.Primary)
                        .Select(x => x.GUID)
                        .Distinct()
                        .ToList();

                    var secondary = g
                        .Where(x => x.GUID != Guid.Empty && x.MuscleRole == MuscleRole.Secondary)
                        .Select(x => x.GUID)
                        .Distinct()
                        .ToList();

                    return ((IReadOnlyList<Guid>)primary, (IReadOnlyList<Guid>)secondary);
                }
                );
    }

    
    private async Task<Dictionary<int, IReadOnlyList<Guid>>> LoadEquipmentAsync(int[] movementIds, CancellationToken ct)
    {
        if (movementIds.Length == 0)
            return new Dictionary<int, IReadOnlyList<Guid>>();

        var antagonists = await _db.MovementEquipmentRelations.AsNoTracking()
            .Where(link => movementIds.Contains(link.MovementId))
            .Include(link => link.Equipment)
            .Select(link => new { link.MovementId, link.Equipment.GUID })
            .ToListAsync(ct);

        return antagonists
            .GroupBy(x => x.MovementId)
            .ToDictionary(
                g => g.Key,
                g => (IReadOnlyList<Guid>)g
                    .Where(x => x.GUID != Guid.Empty)
                    .Select(x => x.GUID)
                    .Distinct()
                    .ToList());
    }

    public async Task<PushResult> PushAsync(IEnumerable<MovementSyncDTO> items, CancellationToken ct)
    {
        _descriptorCache.Clear();
        _categoryIdCache.Clear();
        _equipmentIdCache.Clear();
        _muscleIdCache.Clear();

        var now = DateTimeOffset.UtcNow;
        var preValidationResults = new List<PushItemResult>();
        var preValidationFailures = 0;
        var payloads = new List<(MovementRecord Entity, MovementSyncDTO Source, IReadOnlyList<Guid> Equipments, IReadOnlyList<Guid> PrimaryMuscles, IReadOnlyList<Guid> SecondaryMuscles)>();

        foreach (var dto in items)
        {
            if (dto.GUID == Guid.Empty)
            {
                const string reason = "Movement GUID is required.";
                _log.LogError(nameof(MovementSyncService), reason);
                preValidationResults.Add(new PushItemResult(Guid.Empty, PushItemStatus.Failed, reason));
                preValidationFailures++;
                continue;
            }

            if (!dto.IsDeleted && dto.category == Guid.Empty)
            {
                const string reason = "Movement category GUID is required.";
                _log.LogError(nameof(MovementSyncService), reason);
                preValidationResults.Add(new PushItemResult(dto.GUID, PushItemStatus.Failed, reason));
                preValidationFailures++;
                continue;
            }

            var descriptor = GetDescriptorRecord(dto.DescriptorGUID);
            var entity = MovementSyncMapper.FromSyncDTO(dto, descriptor, SyncActor);

            var equipments = (dto.IsDeleted ? Array.Empty<Guid>() : dto.Equipment ?? Array.Empty<Guid>())
                .Where(g => g != Guid.Empty)
                .Distinct()
                .ToArray();

            var primaryMuscles = (dto.IsDeleted ? Array.Empty<Guid>() : dto.PrimaryMuscles ?? Array.Empty<Guid>())
                .Where(g => g != Guid.Empty)
                .Distinct()
                .ToArray();

            var secondaryMuscles = (dto.IsDeleted ? Array.Empty<Guid>() : dto.SecondaryMuscles ?? Array.Empty<Guid>())
                .Where(g => g != Guid.Empty)
                .Distinct()
                .ToArray();

            payloads.Add((entity, dto, equipments, primaryMuscles, secondaryMuscles));
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
                var pendingRelations = new List<(MovementRecord Movement, IReadOnlyList<Guid> Equipments, IReadOnlyList<Guid> Primary, IReadOnlyList<Guid> Secondary)>();

                await using var tx = await _db.Database.BeginTransactionAsync(ct);
                try
                {
                    foreach (var payload in payloads)
                    {
                        var entity = payload.Entity;
                        var dto = payload.Source;

                        try
                        {
                            var resolvedCategoryId = dto.IsDeleted
                                ? (int?)null
                                : await ResolveMovementCategoryIdAsync(dto.category, ct);

                            if (!dto.IsDeleted && (!resolvedCategoryId.HasValue || resolvedCategoryId.Value == 0))
                            {
                                failed++;
                                txResults.Add(new PushItemResult(entity.Iguid, PushItemStatus.Failed,
                                    $"Category '{dto.category}' not found."));
                                continue;
                            }

                            if (resolvedCategoryId.HasValue)
                            {
                                entity.MovementCategoryId = resolvedCategoryId.Value;
                            }

                            entity.VariantOfMovementGuid = dto.IsDeleted
                                ? null
                                : await ResolveVariantGuidAsync(dto.variantOf, ct);

                            var existing = await _db.Movement
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

                                await _db.Movement.AddAsync(entity, ct);
                                pendingRelations.Add((entity, payload.Equipments, payload.PrimaryMuscles, payload.SecondaryMuscles));
                                txResults.Add(new PushItemResult(entity.Iguid, PushItemStatus.Upserted));
                                accepted++;
                                continue;
                            }

                            if (existing.Authority == DataAuthority.Upstream)
                            {
                                txResults.Add(new PushItemResult(entity.Iguid, PushItemStatus.Failed,
                                    "Upstream-owned movement cannot be modified downstream."));
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
                                existing.VariantOfMovementGuid = null;
                            }
                            else
                            {
                                existing.Name = entity.Name;
                                existing.Descriptor = entity.Descriptor;
                                existing.DescriptorID = entity.DescriptorID;
                                if (resolvedCategoryId.HasValue)
                                {
                                    existing.MovementCategoryId = resolvedCategoryId.Value;
                                }

                                existing.VariantOfMovementGuid = entity.VariantOfMovementGuid;
                                existing.IsDeleted = false;
                            }

                            existing.UpdatedAtUtc = now;
                            existing.UpdatedSeq = await NextUpdateSeqAsync(ct);
                            existing.UpdatedBy = SyncActor;

                            _db.Movement.Update(existing);
                            pendingRelations.Add((existing, payload.Equipments, payload.PrimaryMuscles, payload.SecondaryMuscles));

                            txResults.Add(new PushItemResult(entity.Iguid,
                                entity.IsDeleted ? PushItemStatus.Deleted : PushItemStatus.Upserted));
                            accepted++;
                        }
                        catch (DbUpdateException ex)
                        {
                            failed++;
                            _log.LogError(nameof(MovementSyncService),
                                $"Push item {entity.Iguid} failed: {ex.GetBaseException().Message}");
                            txResults.Add(new PushItemResult(entity.Iguid, PushItemStatus.Failed,
                                ex.GetBaseException().Message));
                        }
                        catch (Exception ex)
                        {
                            failed++;
                            _log.LogError(nameof(MovementSyncService),
                                $"Push item {entity.Iguid} failed: {ex.Message}");
                            txResults.Add(new PushItemResult(entity.Iguid, PushItemStatus.Failed, ex.Message));
                        }
                    }

                    await _db.SaveChangesAsync(ct);

                    foreach (var (movement, equipments, primary, secondary) in pendingRelations)
                    {
                        await SyncMovementRelationsAsync(movement, equipments, primary, secondary, ct);
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
            _log.LogError(nameof(MovementSyncService), $"Push batch failed/rolled back: {ex}");
            var transactionalFailures = payloads
                .Select(p => new PushItemResult(p.Entity.Iguid, PushItemStatus.Failed, "Batch rolled back."))
                .ToList();

            var combined = preValidationResults.Concat(transactionalFailures).ToList();
            var totalFailed = preValidationFailures + payloads.Count;
            return new PushResult(now, 0, totalFailed, combined);
        }

        return finalResult ?? new PushResult(now, 0, preValidationFailures, preValidationResults);
    }

    private async Task<int?> ResolveMovementCategoryIdAsync(Guid? guid, CancellationToken ct)
    {
        if (!guid.HasValue || guid.Value == Guid.Empty)
        {
            return null;
        }

        if (_categoryIdCache.TryGetValue(guid.Value, out var cached) && cached > 0)
        {
            return cached;
        }

        var tracked = _db.MovementCategories.Local.FirstOrDefault(c => c.GUID == guid.Value);
        if (tracked?.Id > 0)
        {
            _categoryIdCache[guid.Value] = tracked.Id;
            return tracked.Id;
        }

        var id = await _db.MovementCategories.AsNoTracking()
            .Where(c => c.GUID == guid.Value)
            .Select(c => c.Id)
            .SingleOrDefaultAsync(ct);

        if (id == 0)
        {
            return null;
        }

        _categoryIdCache[guid.Value] = id;
        return id;
    }

    private async Task<Guid?> ResolveVariantGuidAsync(Guid? variantGuid, CancellationToken ct)
    {
        if (!variantGuid.HasValue || variantGuid.Value == Guid.Empty)
        {
            return null;
        }

        if (_db.Movement.Local.Any(m => m.GUID == variantGuid.Value))
        {
            return variantGuid;
        }

        var exists = await _db.Movement.AsNoTracking()
            .AnyAsync(m => m.GUID == variantGuid.Value, ct);

        if (!exists)
        {
            _log.LogWarning(nameof(MovementSyncService),
                $"Variant movement '{variantGuid}' not found. Variant link skipped.");
            return null;
        }

        return variantGuid;
    }

    private async Task SyncMovementRelationsAsync(
        MovementRecord movement,
        IReadOnlyList<Guid> equipmentGuids,
        IReadOnlyList<Guid> primaryMuscles,
        IReadOnlyList<Guid> secondaryMuscles,
        CancellationToken ct)
    {
        if (movement.Id <= 0)
        {
            _log.LogWarning(nameof(MovementSyncService),
                $"Movement {movement.GUID} missing database id. Relation sync skipped.");
            return;
        }

        var equipmentIds = movement.IsDeleted
            ? new List<int>()
            : await ResolveEquipmentIdsAsync(equipmentGuids, ct);
        var primaryIds = movement.IsDeleted
            ? new List<int>()
            : await ResolveMuscleIdsAsync(primaryMuscles, ct);
        var secondaryIds = movement.IsDeleted
            ? new List<int>()
            : await ResolveMuscleIdsAsync(secondaryMuscles, ct);

        await SyncEquipmentRelationsAsync(movement.Id, equipmentIds, ct);
        await SyncMuscleRelationsAsync(movement.Id, primaryIds, secondaryIds, ct);
    }

    private async Task<List<int>> ResolveEquipmentIdsAsync(IEnumerable<Guid> guids, CancellationToken ct)
    {
        var result = new List<int>();
        foreach (var guid in guids ?? Array.Empty<Guid>())
        {
            if (guid == Guid.Empty) continue;

            if (_equipmentIdCache.TryGetValue(guid, out var cached) && cached > 0)
            {
                result.Add(cached);
                continue;
            }

            var tracked = _db.Equipments.Local.FirstOrDefault(e => e.GUID == guid);
            if (tracked?.Id > 0)
            {
                _equipmentIdCache[guid] = tracked.Id;
                result.Add(tracked.Id);
                continue;
            }

            var id = await _db.Equipments.AsNoTracking()
                .Where(e => e.GUID == guid)
                .Select(e => e.Id)
                .SingleOrDefaultAsync(ct);

            if (id == 0)
            {
                _log.LogWarning(nameof(MovementSyncService),
                    $"Equipment '{guid}' not found. Relationship skipped.");
                continue;
            }

            _equipmentIdCache[guid] = id;
            result.Add(id);
        }

        return result;
    }

    private async Task<List<int>> ResolveMuscleIdsAsync(IEnumerable<Guid> guids, CancellationToken ct)
    {
        var result = new List<int>();
        foreach (var guid in guids ?? Array.Empty<Guid>())
        {
            if (guid == Guid.Empty) continue;

            if (_muscleIdCache.TryGetValue(guid, out var cached) && cached > 0)
            {
                result.Add(cached);
                continue;
            }

            var tracked = _db.Muscles.Local.FirstOrDefault(m => m.GUID == guid);
            if (tracked?.Id > 0)
            {
                _muscleIdCache[guid] = tracked.Id;
                result.Add(tracked.Id);
                continue;
            }

            var id = await _db.Muscles.AsNoTracking()
                .Where(m => m.GUID == guid)
                .Select(m => m.Id)
                .SingleOrDefaultAsync(ct);

            if (id == 0)
            {
                _log.LogWarning(nameof(MovementSyncService),
                    $"Muscle '{guid}' not found. Relationship skipped.");
                continue;
            }

            _muscleIdCache[guid] = id;
            result.Add(id);
        }

        return result;
    }

    private async Task SyncEquipmentRelationsAsync(int movementId, IReadOnlyList<int> desiredEquipmentIds, CancellationToken ct)
    {
        var existing = await _db.MovementEquipmentRelations
            .Where(r => r.MovementId == movementId)
            .ToListAsync(ct);

        if (existing.Count > 0)
        {
            var remove = existing
                .Where(r => !desiredEquipmentIds.Contains(r.EquipmentId))
                .ToList();
            if (remove.Count > 0)
            {
                _db.MovementEquipmentRelations.RemoveRange(remove);
            }
        }

        var current = new HashSet<int>(existing.Select(r => r.EquipmentId));
        foreach (var equipmentId in desiredEquipmentIds.Distinct())
        {
            if (current.Contains(equipmentId)) continue;
            _db.MovementEquipmentRelations.Add(new MovementEquipmentRelationRecord
            {
                MovementId = movementId,
                EquipmentId = equipmentId
            });
        }
    }

    private async Task SyncMuscleRelationsAsync(int movementId, IReadOnlyList<int> primaryIds, IReadOnlyList<int> secondaryIds, CancellationToken ct)
    {
        var existing = await _db.MovementMuscleRelations
            .Where(r => r.MovementId == movementId)
            .ToListAsync(ct);

        if (existing.Count > 0)
        {
            _db.MovementMuscleRelations.RemoveRange(existing);
        }

        foreach (var muscleId in primaryIds.Distinct())
        {
            _db.MovementMuscleRelations.Add(new MovementMuscleRelationRecord
            {
                MovementId = movementId,
                MuscleId = muscleId,
                MuscleRole = MuscleRole.Primary
            });
        }

        foreach (var muscleId in secondaryIds.Distinct())
        {
            _db.MovementMuscleRelations.Add(new MovementMuscleRelationRecord
            {
                MovementId = movementId,
                MuscleId = muscleId,
                MuscleRole = MuscleRole.Secondary
            });
        }
    }
    
    private DescriptorRecord GetDescriptorRecord(Guid? descriptorGuid)
    {
        var cacheKey = descriptorGuid ?? PlaceholderDescriptorGuid;
        if (_descriptorCache.TryGetValue(cacheKey, out var cached))
        {
            return cached;
        }

        DescriptorRecord descriptor;
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
                _log.Log(nameof(MovementSyncService),
                    $"Descriptor {descriptorGuid} not found. Creating stub so FK constraint is satisfied.");
            }
        }

        _descriptorCache[cacheKey] = descriptor;
        return descriptor;
    }

    
    /// <summary>
    /// Retrieves or creates the shared placeholder descriptor used when upstream omits descriptor data.
    /// </summary>
    private DescriptorRecord ResolvePlaceholderDescriptor()
    {
        var descriptor = TryGetTrackedDescriptor(PlaceholderDescriptorGuid);
        return descriptor ?? CreateDescriptorStub(PlaceholderDescriptorGuid, "unspecified");
    }

    /// <summary>
    /// Searches the current context for a descriptor that matches the provided GUID.
    /// </summary>
    private DescriptorRecord? TryGetTrackedDescriptor(Guid guid)
    {
        return _db.Descriptors.Local.FirstOrDefault(d => d.GUID == guid)
               ?? _db.Descriptors.FirstOrDefault(d => d.GUID == guid);
    }

    /// <summary>
    /// Creates an in-memory descriptor stub so foreign-key constraints can be satisfied.
    /// </summary>
    private DescriptorRecord CreateDescriptorStub(Guid guid, string content)
    {
        var now = DateTimeOffset.UtcNow;
        return new DescriptorRecord
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
