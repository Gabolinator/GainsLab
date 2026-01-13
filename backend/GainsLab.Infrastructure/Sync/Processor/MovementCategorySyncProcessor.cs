using GainsLab.Application.DTOs;
using GainsLab.Application.DTOs.Description;
using GainsLab.Application.DTOs.MovementCategory;
using GainsLab.Application.Interfaces.DataManagement;
using GainsLab.Application.Results;
using GainsLab.Contracts.Dtos.SyncDto;
using GainsLab.Contracts.Interface;
using GainsLab.Domain;
using GainsLab.Domain.Interfaces;
using GainsLab.Infrastructure.DB.Context;
using Microsoft.EntityFrameworkCore;

namespace GainsLab.Infrastructure.Sync.Processor;

public class MovementCategorySyncProcessor : ISyncEntityProcessor
{
    
    private const string SyncActor = "sync";

    private readonly IDbContextFactory<GainLabSQLDBContext> _dbContextFactory;
    private readonly ILogger _logger;
    private readonly IDescriptorResolver _descriptorResolver;


    public EntityType EntityType => EntityType.MovementCategory;
    
    public MovementCategorySyncProcessor(IDbContextFactory<GainLabSQLDBContext> dbContextFactory,IDescriptorResolver descriptorResolver, ILogger logger)
    {
        _dbContextFactory = dbContextFactory;
        _logger = logger;
        _descriptorResolver = descriptorResolver;
    }
    
    public async Task<Result> ApplyAsync(IReadOnlyList<ISyncDto> items, ILocalRepository localRepository, CancellationToken ct)
    {
        _logger?.Log(nameof(MovementCategorySyncProcessor), $"Applying Async for {items.Count} items");
        
        if (items.Count == 0) return Result.SuccessResult();

        var typed = items.OfType<MovementCategorySyncDTO>().ToList();
        if (typed.Count == 0) return Result.SuccessResult();
        
          try
        {
            _logger?.Log(nameof(MovementCategorySyncProcessor), $"Applying Async for {items.Count} {nameof(MovementCategorySyncDTO)}");

            await using var dbContext = await _dbContextFactory.CreateDbContextAsync(ct).ConfigureAwait(false);
            var descriptorCache = new Dictionary<Guid, DescriptorRecord>();
            var categoryLookup = new Dictionary<Guid, MovementCategoryRecord>();
            var baseCategory = new Dictionary<eMovementCategories, Guid>();
            var parentCategory = new Dictionary<Guid, Guid>();
            var desiredBaseCategories = new Dictionary<Guid, IReadOnlyList<eMovementCategories>>();

            foreach (var dto in typed)
            {
                ct.ThrowIfCancellationRequested();

                _logger?.Log(nameof(MovementCategorySyncProcessor),
                    $"Applying Async for {nameof(MovementCategorySyncDTO)} : {dto.Name} | {dto.GUID} | {(dto.DescriptorGUID == null ? "null descriptor guid" : dto.DescriptorGUID)}");
                
                var descriptor = await _descriptorResolver.ResolveDescriptorAsync(dbContext, dto.DescriptorGUID, descriptorCache, ct)
                    .ConfigureAwait(false);

                var entity = await dbContext.MovementCategories
                    .Include(e => e.Descriptor)
                    .FirstOrDefaultAsync(e => e.GUID == dto.GUID, ct)
                    .ConfigureAwait(false);

                if (entity is null)
                {
                    entity = new MovementCategoryRecord
                    {
                        Name = dto.Name,
                        GUID = dto.GUID,
                        CreatedAtUtc = dto.UpdatedAtUtc,
                        CreatedBy = SyncActor,
                        Authority = dto.Authority
                    };

                    await dbContext.MovementCategories.AddAsync(entity, ct).ConfigureAwait(false);
                }

                entity.Name = dto.Name;
                entity.Descriptor = descriptor;
                entity.Authority = dto.Authority;

                entity.UpdatedAtUtc = dto.UpdatedAtUtc;
                entity.UpdatedSeq = dto.UpdatedSeq;
                entity.UpdatedBy = SyncActor;
                entity.Version = dto.UpdatedSeq;
                entity.IsDeleted = dto.IsDeleted;
                entity.DeletedAt = dto.IsDeleted ? dto.UpdatedAtUtc : null;
                entity.DeletedBy = dto.IsDeleted ? SyncActor : null;

                _logger?.Log(nameof(MovementCategorySyncProcessor), $"Prepared category {entity.Name} ({entity.GUID})");

                categoryLookup[dto.GUID] = entity;
                var baseCat = NormalizeBaseCategoryGuid(dto);
                if(baseCat.Item1 != eMovementCategories.undefined || baseCat.Item2 != Guid.Empty) baseCategory.TryAdd(baseCat.Item1, baseCat.Item2);

               
                if (dto.ParentCategoryGUID != null && dto.ParentCategoryGUID != Guid.Empty) 
                    parentCategory[dto.GUID] =dto.ParentCategoryGUID.Value;

                desiredBaseCategories[dto.GUID] = NormalizeBaseCategories(dto);

            }

            _logger?.Log(nameof(MovementCategorySyncProcessor), "Save Changes Async (categories)");
            await dbContext.SaveChangesAsync(ct).ConfigureAwait(false);
            _logger?.Log(nameof(MovementCategorySyncProcessor), "Save Changes Async Completed (categories)");

            foreach (var kvp in categoryLookup)
            {
                var categoryGuid = kvp.Key;
                var categoryEntity = kvp.Value;

                parentCategory.TryGetValue(categoryGuid, out var parentId);
                desiredBaseCategories.TryGetValue(categoryGuid, out var desiredBases);
                var baseCatGuids = desiredBases is null
                    ? Array.Empty<Guid>()
                    : GetBaseCatGuids(desiredBases, baseCategory);

                await SyncParentCategoriesAsync(dbContext, categoryEntity, parentId, baseCatGuids, ct)
                    .ConfigureAwait(false);
            }

            _logger?.Log(nameof(MovementCategorySyncProcessor), "Save Changes Async (antagonists)");
            await dbContext.SaveChangesAsync(ct).ConfigureAwait(false);
            _logger?.Log(nameof(MovementCategorySyncProcessor), "Save Changes Async Completed (antagonists)");

            return Result.SuccessResult();
        }
        catch (OperationCanceledException)
        {
            return Result.Failure("Sync cancelled");
        }
        catch (Exception ex)
        {
            _logger?.LogError(nameof(MovementCategorySyncProcessor), $"Failed to apply  sync: {ex.Message}");
            return Result.Failure($"Failed to apply category sync: {ex.GetBaseException().Message}");
        }
        
        
    
    }

    private IReadOnlyList<Guid> GetBaseCatGuids(IReadOnlyList<eMovementCategories> baseCat, Dictionary<eMovementCategories, Guid> baseCategory)
    {
       if(baseCat.Count ==0) return Array.Empty<Guid>();
       
       return baseCategory
           .Where(x => baseCat.Contains(x.Key) && x.Key != eMovementCategories.undefined)
           .Select(x => x.Value)
           .ToList();
    }

    private IReadOnlyList<eMovementCategories> NormalizeBaseCategories(MovementCategorySyncDTO dto)
    {
          if (dto.IsDeleted)
            return Array.Empty<eMovementCategories>();
          
          return dto.BaseCategories?
                     .Where(g => g != eMovementCategories.undefined)
                     .Distinct()
                     .ToList()
                 ?? new();
    }

  
    private (eMovementCategories, Guid) NormalizeBaseCategoryGuid(MovementCategorySyncDTO dto)
    {
        if (dto.IsDeleted)
            return (eMovementCategories.undefined, Guid.Empty);

        return (ParseToBaseCategory(dto), dto.GUID);
    }
    
    
    public eMovementCategories ParseToBaseCategory(MovementCategorySyncDTO dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name)) return eMovementCategories.undefined;
        
        return Enum.TryParse(dto.Name, true, out eMovementCategories baseCategory) ? baseCategory : eMovementCategories.undefined  ;
    }

    private async Task SyncParentCategoriesAsync(GainLabSQLDBContext dbContext, MovementCategoryRecord category, Guid? parentGuid ,IReadOnlyList<Guid> baseCategories, CancellationToken ct)
    {
        if (category.Id == 0)
        {
            _logger?.LogWarning(nameof(MovementCategorySyncProcessor),
                $"Skipping parents sync for {category.GUID} because it has no primary key yet.");
            return;
        }

        var parent = parentGuid != null && parentGuid.Value != Guid.Empty
            ? await dbContext.MovementCategories
                .AsNoTracking().FirstOrDefaultAsync(c=> c.GUID == parentGuid, cancellationToken: ct).ConfigureAwait(false) : null;

        category.ParentCategoryDbId = parent?.Id;
        category.ParentCategory = parent ?? null;
        
        baseCategories ??= Array.Empty<Guid>();
        var desiredSet = baseCategories.Where(g => g != Guid.Empty).ToHashSet();
        
        var existingLinks = await dbContext.MovementCategoryRelations
            .AsNoTracking()
            .Where(link => link.ChildCategoryId == category.Id)
            .Include(link => link.ParentCategory)
            .ToListAsync(ct)
            .ConfigureAwait(false);
        
        
        var toRemove = existingLinks
            .Where(link => link.ParentCategory == null || !desiredSet.Contains(link.ParentCategory.GUID))
            .ToList();
        
        
        if (toRemove.Count > 0)
        {
            dbContext.MovementCategoryRelations.RemoveRange(toRemove);
        }
        
        var existingGuids = existingLinks
            .Where(link => link.ParentCategory != null)
            .Select(link => link.ParentCategory!.GUID)
            .ToHashSet();

        var missingGuids = desiredSet.Except(existingGuids).ToList();
        if (missingGuids.Count == 0)
            return;
        
        
        var related = await dbContext.MovementCategories
            .AsNoTracking()
            .Where(m => missingGuids.Contains(m.GUID))
            .Select(m => new { m.GUID, m.Id })
            .ToListAsync(ct)
            .ConfigureAwait(false);

        var lookup = related.ToDictionary(x => x.GUID, x => x.Id);

        foreach (var guid in missingGuids)
        {
            if (!lookup.TryGetValue(guid, out var parentId))
            {
                _logger?.LogWarning(nameof(MovementCategorySyncProcessor),
                    $"Parent {guid} missing locally. Relationship for catefory {category.GUID} skipped.");
                continue;
            }

            dbContext.MovementCategoryRelations.Add(new MovementCategoryRelationRecord()
            {
                ParentCategoryId = parentId,
                ChildCategoryId = category.Id
            });
        }
        
    }
}
