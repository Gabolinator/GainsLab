using System.Data;
using GainsLab.Application.DTOs;
using GainsLab.Application.DTOs.Description;
using GainsLab.Application.DTOs.MovementCategory;
using GainsLab.Contracts.Dtos.SyncDto;
using GainsLab.Contracts.Interface;
using GainsLab.Contracts.SyncService.Mapper;
using GainsLab.Domain;
using GainsLab.Domain.Interfaces;
using GainsLab.Infrastructure.DB.Context;
using GainsLab.Infrastructure.SyncService.Mapper;
using Microsoft.EntityFrameworkCore;

namespace GainsLab.Infrastructure.SyncService;

public class MovementCategorySyncService : ISyncService<MovementCategorySyncDto>
{
    private readonly GainLabPgDBContext _db;
    private readonly ILogger _log;
    private const string SyncActor = "sync";
    private static readonly Guid PlaceholderDescriptorGuid = Guid.Empty;
    private readonly Dictionary<Guid, DescriptorRecord> _descriptorCache = new();
    private readonly Dictionary<Guid, int> _categoryIdCache = new();
    
    public MovementCategorySyncService(GainLabPgDBContext db, ILogger log)
    {
        _db = db;
        _log = log;
    }

    /// <summary>
    /// Gets the entity type handled by this service.
    /// </summary>
    public EntityType EntityType => EntityType.MovementCategory;
    /// <summary>
    /// Gets the DTO type used for serialization.
    /// </summary>
    public Type DtoType => typeof(MovementCategorySyncDto);
    
    Task<PushResult> ISyncService.PushBoxedAsync(IEnumerable<ISyncDto> dtos, CancellationToken ct)
        => PushAsync(dtos.Cast<MovementCategorySyncDto>(), ct);

    async Task<object> ISyncService.PullBoxedAsync(SyncCursor cur, int take, CancellationToken ct)
        => await PullAsync(cur, take, ct);


    public async Task<SyncPage<MovementCategorySyncDto>> PullAsync(SyncCursor cur, int take, CancellationToken ct)
    {
        var serverTime = DateTimeOffset.UtcNow;
        take = Math.Clamp(take, 1, 500);

        var movementCategoryDtos = await _db.MovementCategories.AsNoTracking()
            .Include(m => m.Descriptor)
            .Include(m => m.ParentCategory)
            .Where(m => m.UpdatedAtUtc > cur.Ts
                        || (m.UpdatedAtUtc == cur.Ts && m.UpdatedSeq > cur.Seq))
            .OrderBy(m => m.UpdatedAtUtc)
            .ThenBy(m => m.UpdatedSeq)
            .Take(take)
            .ToListAsync(ct);

        var baseCategoriesMap = await LoadBaseCategoriesAsync(ct);
        var relationsDtos = await LoadCategoryRelationsAsync(movementCategoryDtos.Select(m => m.Id).ToArray(), ct);

        (Guid? parent, IReadOnlyList<eMovementCategories>? bases) GetParentsAndBase(
            MovementCategoryRecord m)
        {
            if (!relationsDtos.TryGetValue(m.Id, out var relationParents) || relationParents.Count == 0)
            {
                return (m.ParentCategory?.GUID, null);
            }

            var baseMatches = relationParents
                .Where(baseCategoriesMap.ContainsKey)
                .Select(guid => baseCategoriesMap[guid])
                .Distinct()
                .ToList();

            return (m.ParentCategory?.GUID, baseMatches.Count == 0 ? null : baseMatches);
        }

        var items = movementCategoryDtos
            .Select(m => MovementCategorySyncMapper.ToSyncDTO(
                m,
                GetParentsAndBase(m)))
            .ToList();

        SyncCursor? next = items.Count < take
            ? null
            : new SyncCursor(items[^1].UpdatedAtUtc, items[^1].UpdatedSeq);

        return new SyncPage<MovementCategorySyncDto>(serverTime, next, items);
    }

    private async Task<Dictionary<Guid, eMovementCategories>> LoadBaseCategoriesAsync(CancellationToken ct)
    {
        var records = await LoadBaseCategoryRecordsAsync(ct);
        return records.ToDictionary(r => r.Guid, r => r.Category);
    }

    private async Task<Dictionary<eMovementCategories, int>> LoadBaseCategoryIdLookupAsync(CancellationToken ct)
    {
        var records = await LoadBaseCategoryRecordsAsync(ct);
        return records
            .GroupBy(r => r.Category)
            .ToDictionary(g => g.Key, g => g.First().Id);
    }

    private async Task<List<(int Id, Guid Guid, eMovementCategories Category)>> LoadBaseCategoryRecordsAsync(CancellationToken ct)
    {
        var validNames = Enum.GetNames(typeof(eMovementCategories))
            .Where(name => !string.Equals(name, nameof(eMovementCategories.undefined), StringComparison.OrdinalIgnoreCase))
            .ToArray();

        if (validNames.Length == 0)
        {
            return new List<(int, Guid, eMovementCategories)>();
        }

        var baseDtos = await _db.MovementCategories.AsNoTracking()
            .Where(m => validNames.Contains(m.Name))
            .Select(m => new { m.Id, m.GUID, m.Name })
            .ToListAsync(ct);

        var records = new List<(int, Guid, eMovementCategories)>();
        foreach (var dto in baseDtos)
        {
            if (Enum.TryParse(dto.Name, true, out eMovementCategories parsed) &&
                parsed != eMovementCategories.undefined)
            {
                records.Add((dto.Id, dto.GUID, parsed));
            }
        }

        return records;
    }

    private async Task SyncCategoryRelationsAsync(
        MovementCategoryRecord category,
        Guid? parentGuid,
        IReadOnlyList<eMovementCategories>? baseCategories,
        IReadOnlyDictionary<eMovementCategories, int> baseCategoryLookup,
        CancellationToken ct)
    {
        if (category.Id == 0)
        {
            _log.LogWarning(nameof(MovementCategorySyncService),
                $"Cannot sync relations for {category.GUID} because the row has no ID yet.");
            return;
        }

        _categoryIdCache[category.GUID] = category.Id;

        baseCategories ??= Array.Empty<eMovementCategories>();
        var desiredParentIds = new HashSet<int>();

        if (parentGuid.HasValue && parentGuid.Value != Guid.Empty)
        {
            var parentId = await ResolveCategoryIdAsync(parentGuid.Value, ct);
            if (parentId.HasValue)
            {
                desiredParentIds.Add(parentId.Value);
                category.ParentCategoryDbId = parentId.Value;
            }
            else
            {
                category.ParentCategoryDbId = null;
                _log.LogWarning(nameof(MovementCategorySyncService),
                    $"Parent {parentGuid} missing for category {category.GUID}. Parent link skipped.");
            }
        }
        else
        {
            category.ParentCategoryDbId = null;
            category.ParentCategory = null;
        }

        foreach (var baseCategory in baseCategories)
        {
            if (!baseCategoryLookup.TryGetValue(baseCategory, out var baseParentId))
            {
                _log.LogWarning(nameof(MovementCategorySyncService),
                    $"Base category {baseCategory} missing. Relationship for {category.GUID} skipped.");
                continue;
            }

            desiredParentIds.Add(baseParentId);
        }

        var existingRelations = await _db.MovementCategoryRelations
            .Where(r => r.ChildCategoryId == category.Id)
            .ToListAsync(ct);

        if (desiredParentIds.Count == 0)
        {
            if (existingRelations.Count > 0)
            {
                _db.MovementCategoryRelations.RemoveRange(existingRelations);
            }

            return;
        }

        var existingParentIds = existingRelations.Select(r => r.ParentCategoryId).ToHashSet();
        var toRemove = existingRelations
            .Where(r => !desiredParentIds.Contains(r.ParentCategoryId))
            .ToList();
        if (toRemove.Count > 0)
        {
            _db.MovementCategoryRelations.RemoveRange(toRemove);
        }

        var missingParentIds = desiredParentIds.Except(existingParentIds).ToList();
        foreach (var parentId in missingParentIds)
        {
            _db.MovementCategoryRelations.Add(new MovementCategoryRelationRecord
            {
                ChildCategoryId = category.Id,
                ParentCategoryId = parentId
            });
        }
    }

    private async Task<int?> ResolveCategoryIdAsync(Guid guid, CancellationToken ct)
    {
        if (guid == Guid.Empty)
            return null;

        if (_categoryIdCache.TryGetValue(guid, out var cached) && cached > 0)
        {
            return cached;
        }

        var tracked = _db.MovementCategories.Local.FirstOrDefault(c => c.GUID == guid);
        if (tracked != null && tracked.Id > 0)
        {
            _categoryIdCache[guid] = tracked.Id;
            return tracked.Id;
        }

        var id = await _db.MovementCategories.AsNoTracking()
            .Where(c => c.GUID == guid)
            .Select(c => c.Id)
            .SingleOrDefaultAsync(ct);

        if (id == 0)
        {
            return null;
        }

        _categoryIdCache[guid] = id;
        return id;
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
                _log.Log(nameof(MovementCategorySyncService),
                    $"Descriptor {descriptorGuid} not found. Creating stub so FK constraint is satisfied.");
            }
        }

        _descriptorCache[cacheKey] = descriptor;
        return descriptor;
    }

    private DescriptorRecord ResolvePlaceholderDescriptor()
    {
        var descriptor = TryGetTrackedDescriptor(PlaceholderDescriptorGuid);
        return descriptor ?? CreateDescriptorStub(PlaceholderDescriptorGuid, "unspecified");
    }

    private DescriptorRecord? TryGetTrackedDescriptor(Guid guid)
    {
        return _db.Descriptors.Local.FirstOrDefault(d => d.GUID == guid)
               ?? _db.Descriptors.FirstOrDefault(d => d.GUID == guid);
    }

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

    private async Task<long> NextUpdateSeqAsync(CancellationToken ct)
    {
        await using var cmd = _db.Database.GetDbConnection().CreateCommand();
        cmd.CommandText = "SELECT nextval('update_seq')";
        if (cmd.Connection!.State != ConnectionState.Open)
            await cmd.Connection.OpenAsync(ct);
        var val = await cmd.ExecuteScalarAsync(ct);
        return Convert.ToInt64(val);
    }

    private async Task<Dictionary<int, IReadOnlyList<Guid>>> LoadCategoryRelationsAsync(int[] movementCategoryIds, CancellationToken ct)
    {
        if (movementCategoryIds.Length == 0)
        {
            return new();
        }

        var relations = await _db.MovementCategoryRelations.AsNoTracking()
            .Where(r => movementCategoryIds.Contains(r.ChildCategoryId))
            .Include(r => r.ParentCategory)
            .Select(r => new
            {
                r.ChildCategoryId,
                ParentGuid = r.ParentCategory != null ? r.ParentCategory.GUID : Guid.Empty
            })
            .ToListAsync(ct);

        return relations
            .Where(r => r.ParentGuid != Guid.Empty)
            .GroupBy(r => r.ChildCategoryId)
            .ToDictionary(
                g => g.Key,
                g => (IReadOnlyList<Guid>)g
                    .Select(x => x.ParentGuid)
                    .Distinct()
                    .ToList());
    }

    public async Task<PushResult> PushAsync(IEnumerable<MovementCategorySyncDto> items, CancellationToken ct)
    {
        _descriptorCache.Clear();
        _categoryIdCache.Clear();

        var now = DateTimeOffset.UtcNow;
        var preValidationResults = new List<PushItemResult>();
        var preValidationFailures = 0;
        var payloads = new List<(MovementCategoryRecord Entity, Guid? ParentGuid, IReadOnlyList<eMovementCategories> BaseCategories)>();

        foreach (var dto in items)
        {
            if (dto.GUID == Guid.Empty)
            {
                const string reason = "Movement category GUID is required.";
                _log.LogError(nameof(MovementCategorySyncService), reason);
                preValidationResults.Add(new PushItemResult(Guid.Empty, PushItemStatus.Failed, reason));
                preValidationFailures++;
                continue;
            }

            var descriptor = GetDescriptorRecord(dto.DescriptorGUID);
            var entity = MovementCategorySyncMapper.FromSyncDTO(dto, descriptor, SyncActor);

            var normalizedBases = dto.IsDeleted
                ? Array.Empty<eMovementCategories>()
                : (dto.BaseCategories ?? Array.Empty<eMovementCategories>())
                    .Where(b => b != eMovementCategories.undefined)
                    .Distinct()
                    .ToArray();

            Guid? parentGuid = null;
            if (!dto.IsDeleted && dto.ParentCategoryGUID.HasValue && dto.ParentCategoryGUID.Value != Guid.Empty)
            {
                parentGuid = dto.ParentCategoryGUID.Value;
            }

            payloads.Add((entity, parentGuid, normalizedBases));
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
                var pendingRelations = new List<(MovementCategoryRecord Entity, Guid? ParentGuid, IReadOnlyList<eMovementCategories> BaseCategories)>();

                await using var tx = await _db.Database.BeginTransactionAsync(ct);
                try
                {
                    foreach (var payload in payloads)
                    {
                        var entity = payload.Entity;
                        try
                        {
                            var existing = await _db.MovementCategories
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

                                await _db.MovementCategories.AddAsync(entity, ct);
                                pendingRelations.Add((entity, payload.ParentGuid, payload.BaseCategories));
                                txResults.Add(new PushItemResult(entity.Iguid, PushItemStatus.Upserted));
                                accepted++;
                                continue;
                            }

                            if (existing.Authority == DataAuthority.Upstream)
                            {
                                txResults.Add(new PushItemResult(entity.Iguid, PushItemStatus.Failed,
                                    "Upstream-owned category cannot be modified downstream."));
                                continue;
                            }

                            var isIncomingNewer =
                                entity.UpdatedAtUtc > existing.UpdatedAtUtc ||
                                (entity.UpdatedAtUtc == existing.UpdatedAtUtc &&
                                 entity.UpdatedSeq > existing.UpdatedSeq);

                            if (!isIncomingNewer)
                            {
                                txResults.Add(new PushItemResult(entity.Iguid, PushItemStatus.SkippedDuplicate,
                                    "Incoming not newer."));
                                continue;
                            }

                            if (entity.IsDeleted)
                            {
                                existing.IsDeleted = true;
                                existing.ParentCategory = null;
                                existing.ParentCategoryDbId = null;
                            }
                            else
                            {
                                existing.Name = entity.Name;
                                existing.Descriptor = entity.Descriptor;
                                existing.IsDeleted = false;
                            }

                            existing.UpdatedAtUtc = now;
                            existing.UpdatedSeq = await NextUpdateSeqAsync(ct);
                            existing.UpdatedBy = SyncActor;

                            _db.MovementCategories.Update(existing);
                            pendingRelations.Add((existing,
                                entity.IsDeleted ? null : payload.ParentGuid,
                                entity.IsDeleted ? Array.Empty<eMovementCategories>() : payload.BaseCategories));

                            txResults.Add(new PushItemResult(entity.Iguid,
                                entity.IsDeleted ? PushItemStatus.Deleted : PushItemStatus.Upserted));
                            accepted++;
                        }
                        catch (DbUpdateException ex)
                        {
                            failed++;
                            var baseEx = ex.GetBaseException();
                            _log.LogError(nameof(MovementCategorySyncService),
                                $"Push item {entity.Iguid} failed: {baseEx.Message}");
                            txResults.Add(new PushItemResult(entity.Iguid, PushItemStatus.Failed, baseEx.Message));
                        }
                        catch (Exception ex)
                        {
                            failed++;
                            _log.LogError(nameof(MovementCategorySyncService),
                                $"Push item {entity.Iguid} failed: {ex.Message}");
                            txResults.Add(new PushItemResult(entity.Iguid, PushItemStatus.Failed, ex.Message));
                        }
                    }

                    await _db.SaveChangesAsync(ct);

                    var baseCategoryLookup = await LoadBaseCategoryIdLookupAsync(ct);
                    foreach (var pending in pendingRelations)
                    {
                        await SyncCategoryRelationsAsync(pending.Entity, pending.ParentGuid, pending.BaseCategories,
                            baseCategoryLookup, ct);
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
            _log.LogError(nameof(MovementCategorySyncService), $"Push batch failed/rolled back: {ex}");
            var transactionalFailures = payloads
                .Select(p => new PushItemResult(p.Entity.Iguid, PushItemStatus.Failed, "Batch rolled back."))
                .ToList();

            var combinedResults = preValidationResults.Concat(transactionalFailures).ToList();
            var totalFailed = preValidationFailures + payloads.Count;
            return new PushResult(now, 0, totalFailed, combinedResults);
        }

        return finalResult ?? new PushResult(now, 0, preValidationFailures, preValidationResults);
    }
}
