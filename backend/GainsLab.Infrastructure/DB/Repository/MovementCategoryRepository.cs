using System.Collections.Generic;
using System.Linq;
using GainsLab.Application.DomainMappers;
using GainsLab.Application.DTOs.Extensions;
using GainsLab.Application.DTOs.MovementCategory;
using GainsLab.Application.Interfaces.DataManagement.Repository;
using GainsLab.Application.Results.APIResults;
using GainsLab.Contracts;
using GainsLab.Contracts.Dtos.GetDto;
using GainsLab.Contracts.Dtos.PostDto;
using GainsLab.Contracts.Dtos.PutDto;
using GainsLab.Contracts.Dtos.UpdateDto;
using GainsLab.Contracts.Dtos.UpdateDto.Outcome;
using GainsLab.Domain;
using GainsLab.Domain.Interfaces;
using GainsLab.Infrastructure.DB.Context;
using GainsLab.Infrastructure.Utilities;
using Microsoft.EntityFrameworkCore;

namespace GainsLab.Infrastructure.DB.Repository;

public class MovementCategoryRepository : IMovementCategoryRepository
{
    
    private readonly GainLabPgDBContext _db;
    private readonly IClock _clock;
    private readonly ILogger _log;
    
    
    private const string SyncActor = "repo";

    public MovementCategoryRepository(GainLabPgDBContext db, IClock clock, ILogger log)
    {
        _db = db;
        _clock = clock;
        _log = log;
    }
    
    public async Task<APIResult<MovementCategoryGetDTO>> PullByIdAsync(Guid id, CancellationToken ct)
    {
       if(id == Guid.Empty) return APIResult<MovementCategoryGetDTO>.BadRequest("Id cannot be empty");
       
       
       try
       {
           var allCategories = await _db.MovementCategories
               .AsNoTracking().ToListAsync(ct);
           
           var entry = await _db.MovementCategories
               .AsNoTracking()
               .Where(c=>!c.IsDeleted)
               .Include(c=>c.BaseCategoryLinks)
               .Include(c=>c.ChildCategoryLinks)
               .ThenInclude(c=>c.ParentCategory)
               .FirstOrDefaultAsync(c=>c.GUID == id, ct);

           return entry !=null ? 
               APIResult<MovementCategoryGetDTO>.Found(entry.ToGetDTO()!):
               APIResult<MovementCategoryGetDTO>.NotFound(id.ToString());
           
       }
       catch (Exception e)
       {
           return APIResult<MovementCategoryGetDTO>.Exception(e.Message);
       }
     
       
    }

    public async Task<APIResult<MovementCategoryGetDTO>> PostAsync(MovementCategoryPostDTO payload, CancellationToken ct)
    {
        try
        {
            var allCategories = await _db.MovementCategories
                .AsNoTracking()
                .ToListAsync(ct);
            
            MovementCategoryRecord? parentCategory = null;
            if (payload.ParentCategoryId.HasValue && payload.ParentCategoryId.Value != Guid.Empty)
            {
                _log.Log(nameof(MovementCategoryRepository), $"Trying to find parent for {payload.Id}");
                parentCategory = allCategories.FirstOrDefault(c => c.GUID == payload.ParentCategoryId.Value && !c.IsDeleted);

                if (parentCategory == null)
                {
                    _log.LogError(nameof(MovementCategoryRepository), $"Trying to find parent for {payload.Id} Failed - {payload.ParentCategoryId} not found");

                    return APIResult<MovementCategoryGetDTO>.BadRequest(
                        $"Parent category {payload.ParentCategoryId} not found");
                }
                
                _db.Attach(parentCategory);
            }

        
            
            ICollection<MovementCategoryRecord>? baseCategoryRecords = null;
            
            if (payload.BaseCategories?.Count > 0)
            {
                var normalized = payload.BaseCategories
                    .Select(b => Normalize(b.ToString()))
                    .ToHashSet();
                
                baseCategoryRecords = (await GetCategoriesByNames(normalized, allCategories))?.ToList();
                
                _log.Log(nameof(MovementCategoryRepository), $"Found bases for {payload.Id}  -  { (baseCategoryRecords != null &&  baseCategoryRecords.Count > 0 ? string.Join(", ",  baseCategoryRecords.Select(c=> c.Id.ToString())):"none")} ");

                if (baseCategoryRecords != null)
                {
                    foreach (var baseCategory in baseCategoryRecords)
                    {
                        _db.Attach(baseCategory);
                    }
                }
                
            }
            else
            {
                _log.Log(nameof(MovementCategoryRepository), $"No bases for {payload.Id} ");
                baseCategoryRecords = new List<MovementCategoryRecord>();
            }

            var entity = payload.ToEntity(_clock, parentCategory, baseCategoryRecords);           // GUID created here
            if (entity == null)
            {
                _log.LogError(nameof(MovementCategoryRepository), $"Could not conver payload  {payload.Id} to entity");

                return APIResult<MovementCategoryGetDTO>.BadRequest("Could not create record from dto");
            }
       
            var record = await CreateAsync(entity, ct);
            
            //if success => value != null
            return record.Success  ? APIResult<MovementCategoryGetDTO>.Created(record.Value.ToGetDTO()!) : APIResult<MovementCategoryGetDTO>.NotCreated("Failed to create record");
            
        }
        catch (Exception e)
        {
            return APIResult<MovementCategoryGetDTO>.Exception(e.Message);
        }
    }


    public async Task<APIResult<MovementCategoryPutDTO>> PutAsync(Guid id, MovementCategoryPutDTO payload,
        CancellationToken ct)
    {
        if (id == Guid.Empty) return APIResult<MovementCategoryPutDTO>.BadRequest("Id cannot be empty");
        try
        {

            
            
            var existing = await _db.MovementCategories
                .Where(c=>!c.IsDeleted)
                .Include(c=>c.ParentCategory)
                .Include(c=>c.BaseCategoryLinks)
                .Include(c=>c.ChildCategoryLinks)
                .FirstOrDefaultAsync(d => d.GUID == id , ct);

            var allCategories = await _db.MovementCategories
                .AsNoTracking()
                .ToListAsync(ct);
            
            if (existing is null)
            {
                MovementCategoryRecord? parentCategory = null;
                if (payload.ParentCategoryId.HasValue && payload.ParentCategoryId.Value != Guid.Empty)
                {
                    parentCategory = await _db.MovementCategories
                        .FirstOrDefaultAsync(c => c.GUID == payload.ParentCategoryId.Value && !c.IsDeleted, ct);

                    if (parentCategory == null)
                    {
                        return APIResult<MovementCategoryPutDTO>.BadRequest(
                            $"Parent category {payload.ParentCategoryId} not found");
                    }
                }

                
                
                ICollection<MovementCategoryRecord>? baseCategoryRecords = null;
                if (payload.BaseCategories?.Count > 0)
                {
                    var normalized = payload.BaseCategories
                        .Select(b => Normalize(b.ToString()))
                        .ToHashSet();

                    

                    baseCategoryRecords = (await GetCategoriesByNames(normalized, allCategories))?.ToList();
                }

                // create via shared method
                var entity =
                    payload.ToEntity(_clock,
                        id,
                        null,
                        parentCategory,
                        baseCategoryRecords)!; // guid may be null -> create a new one inside mapping OR enforce not null
                var created = await CreateAsync(entity, ct);


                return !created.Success
                    ? APIResult<MovementCategoryPutDTO>.NotCreated(created.GetErrorMessage() ?? "Create failed")
                    : APIResult<MovementCategoryPutDTO>.Created(created.Value!.ToPutDTO(_clock,
                        UpsertOutcome.Created)!);
            }

            payload.Id = id;
            
            IEnumerable<MovementCategoryRecord>? baseCat = await GetCategoriesByNames(payload.BaseCategories.Select(b=>Normalize(b.ToString())).ToHashSet(), allCategories);
            IEnumerable<MovementCategoryRecord>? childCat = await GetChildCategoriesByGuis(existing.GUID, allCategories);
            var baseCatGuid = baseCat?.Select(b => b.GUID).Distinct().ToList();
            var childCatGuid = childCat?.Select(c=>c.GUID).Distinct().ToList();
            //nothing changed
            if (!existing.AnythingChanged(payload, baseCatGuid, childCatGuid))
                return APIResult<MovementCategoryPutDTO>.NothingChanged($"For entity : {payload.Id}");

            //put always updates all field

            var allRelations = await _db.MovementCategoryRelations
                .AsNoTracking()
                .ToListAsync(cancellationToken: ct);
            
            // update branch
            existing.Descriptor = payload.Descriptor.ToEntity(_clock);
            var baseRelations = GetOrCreateRelations(existing, baseCat, allRelations);
            var mergedBaseRelations = MergeBaseRelations(existing, baseRelations);
            existing.BaseCategoryLinks.Clear();
            foreach (var relation in mergedBaseRelations)
            {
                existing.BaseCategoryLinks.Add(relation);
            }
            existing.ChildCategoryLinks =  GetOrCreateRelations(existing, childCat, allRelations);
            existing.UpdatedAtUtc = _clock.UtcNow;
            existing.UpdatedBy = payload.UpdatedBy;
            existing.Authority = payload.Authority;

            await _db.SaveChangesAsync(ct).ConfigureAwait(false);
            return APIResult<MovementCategoryPutDTO>.Updated(existing.ToPutDTO(_clock, UpsertOutcome.Updated)!);
        }
        catch (Exception e)
        {
            return APIResult<MovementCategoryPutDTO>.Exception(e.Message);
        }

    }

    private ICollection<MovementCategoryRelationRecord> GetOrCreateRelations(
        MovementCategoryRecord existing,
        IEnumerable<MovementCategoryRecord>? relatedCategories,
        List<MovementCategoryRelationRecord>? allRelations = null)
    {
        if (relatedCategories == null)
        {
            return new List<MovementCategoryRelationRecord>();
        }

        var relations = new List<MovementCategoryRelationRecord>();
        foreach (var candidate in relatedCategories)
        {
            if (candidate == null) continue;

            // Determine whether the current record should be the parent or the child in the relation.
            var existingIsParent = candidate.ParentAndBaseCategoryGUIDs.Contains(existing.GUID);
            var parent = existingIsParent ? existing : candidate;
            var child = existingIsParent ? candidate : existing;

            var relation = TryReuseRelation(parent, child, allRelations) ?? new MovementCategoryRelationRecord();
            relation.ParentCategory = parent;
            relation.ParentCategoryId = parent.Id;
            relation.ChildCategory = child;
            relation.ChildCategoryId = child.Id;

            relations.Add(relation);
        }

        return relations;
    }

    private static MovementCategoryRelationRecord? TryReuseRelation(
        MovementCategoryRecord parent,
        MovementCategoryRecord child,
        List<MovementCategoryRelationRecord>? allRelations)
    {
        if (allRelations == null || allRelations.Count == 0)
        {
            return null;
        }

        return allRelations.FirstOrDefault(r =>
            (r.ParentCategoryId > 0 && r.ParentCategoryId == parent.Id && r.ChildCategoryId == child.Id) ||
            (r.ParentCategory?.GUID == parent.GUID && r.ChildCategory?.GUID == child.GUID));
    }

    private async Task<IEnumerable<MovementCategoryRecord>?> GetChildCategoriesByGuis(Guid existingGuid, List<MovementCategoryRecord>? allCategories = null)
    {
        if(allCategories == null) 
            allCategories = await _db.MovementCategories.AsNoTracking().ToListAsync();
        
        if(!allCategories.Any()) return new List<MovementCategoryRecord>();
        
        return  allCategories.Where(c => c.ParentAndBaseCategoryGUIDs.Contains(existingGuid)).ToList();
    }

    private async Task<IEnumerable<MovementCategoryRecord>?> GetCategoriesByNames(HashSet<string> normalizedNames, List<MovementCategoryRecord>? allCategories = null)
    {
        if(!normalizedNames.Any()) return new List<MovementCategoryRecord>();
        
       if(allCategories == null) 
           allCategories = await _db.MovementCategories.AsNoTracking().ToListAsync();
       
       if(!allCategories.Any()) return new List<MovementCategoryRecord>();
       
       return allCategories.Where(c => normalizedNames.Contains(Normalize(c.Name))).ToList();
       
    }

    private string Normalize(string argName)
    {
       return StringFormater.Normalize(argName);
    }

    private ICollection<MovementCategoryRelationRecord> MergeBaseRelations(
        MovementCategoryRecord category,
        ICollection<MovementCategoryRelationRecord>? requestedRelations)
    {
        if (requestedRelations == null || requestedRelations.Count == 0)
        {
            return new List<MovementCategoryRelationRecord>();
        }

        var existingByParentChild = category.BaseCategoryLinks?
            .Where(link => link.ParentCategoryId > 0 && link.ChildCategoryId > 0)
            .ToDictionary(
                link => (link.ParentCategoryId, link.ChildCategoryId),
                link => link)
            ?? new Dictionary<(int ParentId, int ChildId), MovementCategoryRelationRecord>();

        var merged = new List<MovementCategoryRelationRecord>();
        foreach (var relation in requestedRelations)
        {
            var key = (relation.ParentCategoryId, relation.ChildCategoryId);
            if (existingByParentChild.TryGetValue(key, out var trackedRelation))
            {
                merged.Add(trackedRelation);
                existingByParentChild.Remove(key);
            }
            else
            {
                merged.Add(relation);
            }
        }

        return merged;
    }


    public async Task<APIResult<MovementCategoryUpdateOutcome>> PatchAsync(Guid id, MovementCategoryUpdateDTO payload, CancellationToken ct)
    {
        if (id == Guid.Empty) return APIResult<MovementCategoryUpdateOutcome>.BadRequest("Id cannot be empty");
        try
        {
            var category = await _db.MovementCategories
                .Include(c => c.Descriptor)
                .Include(c => c.ParentCategory)
                .Include(c => c.BaseCategoryLinks)
                    .ThenInclude(link => link.ParentCategory)
                .FirstOrDefaultAsync(c => c.GUID == id && !c.IsDeleted, ct);

            if (category == null)
            {
                return APIResult<MovementCategoryUpdateOutcome>.NotUpdated("Not found for update");
            }

            MovementCategoryRecord? requestedParent = null;
            if (payload.ParentCategory !=null && payload.ParentCategory.Id != Guid.Empty)
            {
                requestedParent = await _db.MovementCategories
                    .FirstOrDefaultAsync(c => c.GUID == payload.ParentCategory.Id && !c.IsDeleted, ct);

                if (requestedParent == null)
                {
                    return APIResult<MovementCategoryUpdateOutcome>.BadRequest($"Parent category {payload.ParentCategory} not found");
                }
            }

            var requestedBaseGuids = payload.BaseCategories == null || payload.BaseCategories.Count == 0
                ? new HashSet<Guid>()
                : payload.BaseCategories
                    .Select(b => b.Id)
                    .Where(id => id != Guid.Empty)
                    .ToHashSet();

            _log.Log(nameof(MovementCategoryRepository),
                $"Base categories requested: {requestedBaseGuids.Count}, existing: {category.BaseCategoryLinks?.Count ?? 0}");

            var existingLinks = category.BaseCategoryLinks ?? new List<MovementCategoryRelationRecord>();
            var existingGuids = existingLinks
                .Select(link => link.ParentCategory?.GUID)
                .Where(g => g.HasValue && g.Value != Guid.Empty)
                .Select(g => g!.Value)
                .ToList();

            var guidsToRemove = existingGuids.Where(g => !requestedBaseGuids.Contains(g)).ToList();
            var guidsToAdd = requestedBaseGuids.Where(g => !existingGuids.Contains(g)).ToList();

            var basesChanged = guidsToRemove.Any() || guidsToAdd.Any();

            if (basesChanged)
            {
                var toRemove = existingLinks
                    .Where(link => link.ParentCategory?.GUID is Guid guid && guidsToRemove.Contains(guid))
                    .ToList();
                
                if (category.BaseCategoryLinks != null)
                    foreach (var relation in toRemove)
                    {
                        category.BaseCategoryLinks.Remove(relation);
                    }

                foreach (var guidToAdd in guidsToAdd)
                {
                    var parent = _db.MovementCategories.Local.FirstOrDefault(c => c.GUID == guidToAdd);
                    parent ??= await _db.MovementCategories
                        .FirstOrDefaultAsync(c => c.GUID == guidToAdd && !c.IsDeleted, ct);

                    if (parent == null)
                    {
                        _log.LogWarning(nameof(MovementCategoryRepository),
                            $"Base category {guidToAdd} not found when patching {category.GUID}");
                        continue;
                    }

                    var relation = new MovementCategoryRelationRecord
                    {
                        ParentCategory = parent,
                        ParentCategoryId = parent.Id,
                        ChildCategory = category,
                        ChildCategoryId = category.Id
                    };
                    category.BaseCategoryLinks ??= new List<MovementCategoryRelationRecord>();
                    category.BaseCategoryLinks.Add(relation);
                }

                _log.Log(nameof(MovementCategoryRepository),
                    $"Base diff: removed {guidsToRemove.Count}, added {guidsToAdd.Count}");
            }

            var categoryChanged = category.TryUpdate(
                payload,
                _clock,
                payload.ParentCategory != null ? requestedParent : null,
                null,
                _log);
            categoryChanged = categoryChanged || basesChanged;
            
            _log.Log(nameof(MovementCategoryRepository), $"Category Changed : {categoryChanged}");
            
            var descriptorOutcomeState = payload.Descriptor == null ? UpdateOutcome.NotRequested : UpdateOutcome.NotUpdated;
            var descriptorChanged = false;
            DescriptorUpdateOutcome? descriptorOutcome = null;

            if (payload.Descriptor != null)
            {
                if (category.Descriptor == null)
                {
                    _log.LogWarning(nameof(MovementCategoryRepository), $"No descriptor provided");

                    return APIResult<MovementCategoryUpdateOutcome>.Problem("Descriptor missing for movement category");
                }

                descriptorChanged = category.Descriptor.TryUpdate(payload.Descriptor, _clock);
                descriptorOutcomeState = descriptorChanged ? UpdateOutcome.Updated : UpdateOutcome.NotUpdated;
                descriptorOutcome = new DescriptorUpdateOutcome(descriptorOutcomeState, category.Descriptor.ToGetDTO());
            }

            if (!categoryChanged && !descriptorChanged)
            {
                return APIResult<MovementCategoryUpdateOutcome>.NothingChanged("Nothing changed");
            }

            await _db.SaveChangesAsync(ct).ConfigureAwait(false);

            var updatedState = category.ToGetDTO();
            var outcome = new MovementCategoryUpdateOutcome(
                categoryChanged ? UpdateOutcome.Updated : UpdateOutcome.NotUpdated,
                descriptorOutcomeState,
                descriptorOutcome,
                updatedState);

            return APIResult<MovementCategoryUpdateOutcome>.Updated(outcome);
        }
        catch (Exception e)
        {
            _log.LogError(nameof(MovementCategoryRepository), $"Exception: {e.GetBaseException().Message}");

            return APIResult<MovementCategoryUpdateOutcome>.Exception(e.Message);
        }
    }

    public async Task<APIResult<MovementCategoryGetDTO>> DeleteAsync(Guid id, CancellationToken ct)
    {
        if (id == Guid.Empty) return APIResult<MovementCategoryGetDTO>.BadRequest("Invalid id for delete");
        try
        {
            var existing = await _db.MovementCategories
                .Include(c => c.Descriptor)
                .Include(c => c.ParentCategory)
                .Include(c => c.BaseCategoryLinks)
                    .ThenInclude(link => link.ParentCategory)
                .FirstOrDefaultAsync(c => c.GUID == id, ct);

            if (existing is null)
            {
                return APIResult<MovementCategoryGetDTO>.NotFound($"Movement category {id} not found for deletion");
            }

            var dto = existing.ToGetDTO();

            _db.MovementCategories.Remove(existing);
            await _db.SaveChangesAsync(ct).ConfigureAwait(false);

            return APIResult<MovementCategoryGetDTO>.Deleted(dto!);
        }
        catch (Exception e)
        {
            return APIResult<MovementCategoryGetDTO>.Exception(e.Message);
        }
    }
    
    private async Task<APIResult<MovementCategoryRecord>> CreateAsync(MovementCategoryRecord entity, CancellationToken ct)
    {
        try
        {
            _log.Log(nameof(MovementCategoryRepository), $"Trying to add MovementCategory Entity {entity.Id} - {entity.Name} - {entity.Iguid} " );

            
            var entry = await _db.MovementCategories.AddAsync(entity, ct).ConfigureAwait(false);
            if (entry is { State: EntityState.Added, Entity: not null })
            {
                await _db.SaveChangesAsync(ct).ConfigureAwait(false);
                return APIResult<MovementCategoryRecord>.Created(entry.Entity);
            }

            return APIResult<MovementCategoryRecord>.Problem("Not inserted");
        }
        catch (Exception e)
        {
            _log.LogError(nameof(MovementCategoryRepository), $"An exception happened while trying to add MovementCategory Entity {entity.Id} - {entity.Name} - {entity.Iguid} :  {e.GetBaseException().Message}" );

            return APIResult<MovementCategoryRecord>.Exception(e.Message);
        }
    }

}
