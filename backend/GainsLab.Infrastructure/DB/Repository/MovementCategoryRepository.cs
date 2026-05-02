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

public class MovementCategoryRepository(GainLabPgDBContext db, IDescriptorRepository descriptorRepository ,IClock clock, ILogger log) : IMovementCategoryRepository
{
    public async Task<APIResult<MovementCategoryGetDTO>> PullByIdAsync(Guid id, CancellationToken ct)
    {
        if(id == Guid.Empty) return APIResult<MovementCategoryGetDTO>.BadRequest("Id cannot be empty");
       
        try
        {
            var result = await GetRecordByIdAsync(id, ct);
            
            return CrudResultUtilities.DispatchResult<MovementCategoryGetDTO, MovementCategoryRecord>(result, record => record.ToGetDTO()!);
        }
        catch (Exception e)
        {
            return APIResult<MovementCategoryGetDTO>.Exception(e.Message);
        }
     
       
    }

    private async Task<APIResult<MovementCategoryRecord>> GetRecordByIdAsync(Guid? id, CancellationToken ct)
    {
        if(!id.HasValue || id == Guid.Empty) 
            return APIResult<MovementCategoryRecord>.BadRequest("Id cannot be empty or null");
        
        try
        {
            var entry = await db.MovementCategories
                .AsNoTracking()
                .Where(c => !c.IsDeleted)
                .Include(c=> c.Descriptor)
                .Include(c => c.BaseCategoryLinks)
                .ThenInclude(r => r.ParentCategory)
                .Include(c => c.ChildCategoryLinks)
                .ThenInclude(r => r.ChildCategory)
                .FirstOrDefaultAsync(c => c.GUID == id, ct);
            
            return  entry != null ? 
                APIResult<MovementCategoryRecord>.Found( entry!):
                APIResult<MovementCategoryRecord>.NotFound(id.Value.ToString());

        }
        catch (Exception e)
        {
            return APIResult<MovementCategoryRecord>.Exception(e.Message);
        }
    }
    
    private async Task<APIResult<MovementCategoryRecord>> GetRecordByNameAsync(string name, CancellationToken ct)
    {
        var formatedContent = name.Trim();
        if(string.IsNullOrWhiteSpace(formatedContent)) 
            return APIResult<MovementCategoryRecord>.BadRequest("Name cannot be empty");
        
        try
        {
            var movementCategory = await db.MovementCategories
                .AsNoTracking()
                .Include(e=>e.Descriptor)
                .Where(e => !e.IsDeleted)
                .FirstOrDefaultAsync(e=> e.Name.ToLower().Trim() == name, ct);
            
            return movementCategory != null ? 
                APIResult<MovementCategoryRecord>.Found(movementCategory!):
                APIResult<MovementCategoryRecord>.NotFound(name);

        }
        catch (Exception e)
        {
            return APIResult<MovementCategoryRecord>.Exception(e.Message);
        }
    }

    private Task<UniqueValidationResult<MovementCategoryGetDTO>> TryValidateUnique(
        MovementCategoryRecord entity,
        CancellationToken ct)
    {
        return CrudResultUtilities.TryValidateUniqueAsync<MovementCategoryRecord, MovementCategoryGetDTO>(
            entity,
            EntityType.Descriptor,
            getId: x => x.GUID,
            getName: x => x.Name,
            getContent: x => null,
            getOther: x => null,
            getExistingRecordAsync: (x, token) => GetExistingRecordAsync(x.GUID, x.Name, token),
            ct);
    }
    
    private Task<MatchingResult<MovementCategoryRecord>> GetExistingRecordAsync(
        Guid? id,
        string? name,
        CancellationToken ct)
    {
        return CrudResultUtilities.GetExistingRecordAsync(
            id: id,
            name: name,
            content: null,
            other: null,
            getById: GetRecordByIdAsync,
            getByName: GetRecordByNameAsync,
            getByContent: null,
            getByOther: null,
            ct: ct);
    }
    
    public async Task<APIResult<MovementCategoryGetDTO>> PostAsync(MovementCategoryPostDTO payload, CancellationToken ct)
    {
        try
        {
            log.Log(nameof(MovementCategoryRepository), $"Try to post {payload.Print()}");

            var validation = await ValidatePayloadAsync(payload, ct);
            if (!validation.Success)
                return validation;

            var allCategories = await LoadCategoriesAsync(ct);

            var parentResult = ResolveParentCategory(payload, allCategories);
            if (!parentResult.IsSuccess)
                return parentResult.Error!;

            var baseResult = await ResolveBaseCategoriesAsync(payload, allCategories);
            if (!baseResult.IsSuccess)
                return baseResult.Error!;

            AttachIfNotNull(parentResult.Value);
            AttachMany(baseResult.Value);

            var entity = payload.ToEntity(clock, parentResult.Value, baseResult.Value);
            if (entity == null)
            {
                return APIResult<MovementCategoryGetDTO>
                    .BadRequest("Could not create record from dto");
            }

            var createResult = await CreateAsync(entity, ct);

            return createResult.Success && createResult.Value != null
                ? APIResult<MovementCategoryGetDTO>.Created(createResult.Value.ToGetDTO()!)
                : APIResult<MovementCategoryGetDTO>.NotCreated(
                    "Failed to create record",
                    NotCreatedReason.Other);
        }
        catch (Exception e)
        {
            return APIResult<MovementCategoryGetDTO>.Exception(e.Message);
        }
        
    }
    
    private Task<List<MovementCategoryRecord>> LoadCategoriesAsync(CancellationToken ct)
    {
        return db.MovementCategories
            .AsNoTracking()
            .Include(c => c.ParentCategory)
            .Include(c => c.BaseCategoryLinks)
            .ToListAsync(ct);
    }
    
    private ResolveResult<MovementCategoryRecord?, MovementCategoryGetDTO> ResolveParentCategory(
        MovementCategoryPostDTO payload,
        List<MovementCategoryRecord> allCategories)
    {
        if (!payload.ParentCategoryId.HasValue ||
            payload.ParentCategoryId.Value == Guid.Empty)
        {
            return ResolveResult<MovementCategoryRecord?, MovementCategoryGetDTO>
                .Success(null);
        }

        log.Log(nameof(MovementCategoryRepository), $"Trying to find parent for {payload.Id}");

        var parent = allCategories.FirstOrDefault(c =>
            c.GUID == payload.ParentCategoryId.Value &&
            !c.IsDeleted);

        if (parent == null)
        {
            log.LogError(
                nameof(MovementCategoryRepository),
                $"Trying to find parent for {payload.Id} Failed - {payload.ParentCategoryId} not found");

            return ResolveResult<MovementCategoryRecord?, MovementCategoryGetDTO>
                .Fail(APIResult<MovementCategoryGetDTO>.BadRequest(
                    $"Parent category {payload.ParentCategoryId} not found"));
        }

        log.Log(nameof(MovementCategoryRepository), $"Parent {parent.Name} found for {payload.Id}");

        return ResolveResult<MovementCategoryRecord?, MovementCategoryGetDTO>
            .Success(parent);
    }
    
    private async Task<ResolveResult<ICollection<MovementCategoryRecord>, MovementCategoryGetDTO>> ResolveBaseCategoriesAsync(
        MovementCategoryPostDTO payload,
        List<MovementCategoryRecord> allCategories)
    {
        if (payload.BaseCategories == null || payload.BaseCategories.Count == 0)
        {
            log.Log(nameof(MovementCategoryRepository), $"No bases for {payload.Id}");

            return ResolveResult<ICollection<MovementCategoryRecord>, MovementCategoryGetDTO>
                .Success(new List<MovementCategoryRecord>());
        }

        var normalized = payload.BaseCategories
            .Select(b => Normalize(b.ToString()))
            .ToHashSet();

        var bases = (await GetCategoriesByNames(normalized, allCategories))
                    ?.ToList()
                    ?? new List<MovementCategoryRecord>();

        log.Log(
            nameof(MovementCategoryRepository),
            $"Found bases for {payload.Id} - {(bases.Count > 0 ? string.Join(", ", bases.Select(c => c.Id)) : "none")}");

        return ResolveResult<ICollection<MovementCategoryRecord>, MovementCategoryGetDTO>
            .Success(bases);
    }
    
    private void AttachIfNotNull(MovementCategoryRecord? category)
    {
        if (category != null)
        {
            db.Attach(category);
        }
    }

    private void AttachMany(IEnumerable<MovementCategoryRecord>? categories)
    {
        if (categories == null)
            return;

        foreach (var category in categories)
        {
            db.Attach(category);
        }
    }
    
    private async Task<APIResult<MovementCategoryGetDTO>> ValidatePayloadAsync(
        MovementCategoryPostDTO payload,
        CancellationToken ct)
    {
        var baseEntity = payload.ToEntity(clock);

        if (baseEntity == null)
        {
            return APIResult<MovementCategoryGetDTO>.NotCreated(
                "Could not construct entity",
                NotCreatedReason.Other);
        }

        var match = await TryValidateUnique(baseEntity, ct);

        if (match.AlreadyExists)
        {
            return match.ExistingResult!;
        }

        return APIResult<MovementCategoryGetDTO>.Ok();
    }


    public async Task<APIResult<MovementCategoryPutDTO>> PutAsync(Guid id, MovementCategoryPutDTO payload,
        CancellationToken ct)
    {
        if (id == Guid.Empty) return APIResult<MovementCategoryPutDTO>.BadRequest("Id cannot be empty");
        try
        {
            payload.Id = id;

            var result = await GetTrackedRecordByIdAsync(id, ct);
            var existing = result.Value;

            if (!result.Success || existing is null)
            {
                var postPayload = payload.ToPostDto(id);
               
                var created = await PostAsync(postPayload, ct);

                return created.Success && created.Value != null
                    ? APIResult<MovementCategoryPutDTO>.Created(
                        created.Value.ToPutDto(clock, UpsertOutcome.Created)!)
                    : APIResult<MovementCategoryPutDTO>.NotCreated(
                        !string.IsNullOrWhiteSpace(created.GetErrorMessage()) ? created.GetErrorMessage() : "Failed to create record",
                        NotCreatedReason.Other);
            }

            var allCategories = await LoadCategoriesAsync(ct);

            var baseCategoryNames = payload.BaseCategories?
                                        .Select(b => Normalize(b.ToString()))
                                        .ToHashSet()
                                    ?? new HashSet<string>();

            var baseCategories =
                (await GetCategoriesByNames(baseCategoryNames, allCategories))
                ?.ToList()
                ?? new List<MovementCategoryRecord>();

            var childCategories =
                (await GetChildCategoriesByGuis(existing.GUID, allCategories))
                ?.ToList()
                ?? new List<MovementCategoryRecord>();

            var baseCategoryGuids = baseCategories
                .Select(b => b.GUID)
                .Distinct()
                .ToList();

            var childCategoryGuids = childCategories
                .Select(c => c.GUID)
                .Distinct()
                .ToList();

            if (!existing.AnythingChanged(payload, baseCategoryGuids, childCategoryGuids))
            {
                return APIResult<MovementCategoryPutDTO>
                    .NothingChanged($"For entity : {payload.Id}");
            }

            var descriptorEntity = payload.Descriptor?.ToEntity(clock);

            if (descriptorEntity != null)
            {
                var descriptorResult =
                    await descriptorRepository.GetOrCreateAsync(descriptorEntity, ct);

                if (!descriptorResult.Success || descriptorResult.Value is null)
                {
                    return APIResult<MovementCategoryPutDTO>
                        .BadRequest("Could not resolve descriptor");
                }

                existing.DescriptorID = descriptorResult.Value.Id;
                existing.Descriptor = null;
            }

            var allRelations = await db.MovementCategoryRelations
                .AsNoTracking()
                .ToListAsync(ct);

            foreach (var baseCategory in baseCategories)
                db.Attach(baseCategory);

            foreach (var childCategory in childCategories)
                db.Attach(childCategory);

            var baseRelations = GetOrCreateRelations(existing, baseCategories, allRelations);
            var mergedBaseRelations = MergeBaseRelations(existing, baseRelations);

            existing.BaseCategoryLinks.Clear();

            foreach (var relation in mergedBaseRelations)
            {
                existing.BaseCategoryLinks.Add(relation);
            }

            var childRelations = GetOrCreateRelations(existing, childCategories, allRelations);

            existing.ChildCategoryLinks.Clear();

            foreach (var relation in childRelations)
            {
                existing.ChildCategoryLinks.Add(relation);
            }

            existing.Name = payload.Name;
            existing.UpdatedAtUtc = clock.UtcNow;
            existing.UpdatedBy = payload.UpdatedBy;
            existing.Authority = payload.Authority;

            await db.SaveChangesAsync(ct).ConfigureAwait(false);

            return APIResult<MovementCategoryPutDTO>
                .Updated(existing.ToPutDTO(clock, UpsertOutcome.Updated)!);
        }
        catch (Exception e)
        {
            return APIResult<MovementCategoryPutDTO>.Exception(e.Message);
        }
    }
    
    private async Task<APIResult<MovementCategoryRecord>> GetTrackedRecordByIdAsync(
        Guid id,
        CancellationToken ct)
    {
        var record = await db.MovementCategories
            .Include(c => c.Descriptor)
            .Include(c => c.ParentCategory)
            .Include(c => c.BaseCategoryLinks)
            .ThenInclude(r => r.ParentCategory)
            .Include(c => c.ChildCategoryLinks)
            .ThenInclude(r => r.ChildCategory)
            .FirstOrDefaultAsync(c => c.GUID == id && !c.IsDeleted, ct);

        return record == null
            ? APIResult<MovementCategoryRecord>.NotFound($"Movement category {id} not found")
            : APIResult<MovementCategoryRecord>.Found(record);
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
            allCategories = await db.MovementCategories.AsNoTracking().ToListAsync();
        
        if(!allCategories.Any()) return new List<MovementCategoryRecord>();
        
        return  allCategories.Where(c => c.ParentAndBaseCategoryGUIDs.Contains(existingGuid)).ToList();
    }

    private async Task<IEnumerable<MovementCategoryRecord>?> GetCategoriesByNames(HashSet<string> normalizedNames, List<MovementCategoryRecord>? allCategories = null)
    {
        if(!normalizedNames.Any()) return new List<MovementCategoryRecord>();
        
        if(allCategories == null) 
            allCategories = await db.MovementCategories.AsNoTracking().ToListAsync();
       
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
            var category = await db.MovementCategories
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
                requestedParent = await db.MovementCategories
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

            log.Log(nameof(MovementCategoryRepository),
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
                    var parent = db.MovementCategories.Local.FirstOrDefault(c => c.GUID == guidToAdd);
                    parent ??= await db.MovementCategories
                        .FirstOrDefaultAsync(c => c.GUID == guidToAdd && !c.IsDeleted, ct);

                    if (parent == null)
                    {
                        log.LogWarning(nameof(MovementCategoryRepository),
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

                log.Log(nameof(MovementCategoryRepository),
                    $"Base diff: removed {guidsToRemove.Count}, added {guidsToAdd.Count}");
            }

            var categoryChanged = category.TryUpdate(
                payload,
                clock,
                payload.ParentCategory != null ? requestedParent : null,
                null,
                log);
            categoryChanged = categoryChanged || basesChanged;
            
            log.Log(nameof(MovementCategoryRepository), $"Category Changed : {categoryChanged}");
            
            var descriptorOutcomeState = payload.Descriptor == null ? UpdateOutcome.NotRequested : UpdateOutcome.NotUpdated;
            var descriptorChanged = false;
            DescriptorUpdateOutcome? descriptorOutcome = null;

            if (payload.Descriptor != null)
            {
                //not sure it make sense to make movement category depend on description
                if (category.Descriptor == null)
                {
                    log.LogWarning(nameof(MovementCategoryRepository), $"No descriptor provided");

                    return APIResult<MovementCategoryUpdateOutcome>.Problem("Descriptor missing for movement category");
                }

                descriptorChanged = category.Descriptor.TryUpdate(payload.Descriptor, clock);
                descriptorOutcomeState = descriptorChanged ? UpdateOutcome.Updated : UpdateOutcome.NotUpdated;
                descriptorOutcome = new DescriptorUpdateOutcome(descriptorOutcomeState, category.Descriptor.ToGetDTO());
            }

            if (!categoryChanged && !descriptorChanged)
            {
                return APIResult<MovementCategoryUpdateOutcome>.NothingChanged("Nothing changed");
            }

            await db.SaveChangesAsync(ct).ConfigureAwait(false);

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
            log.LogError(nameof(MovementCategoryRepository), $"Exception: {e.GetBaseException().Message}");

            return APIResult<MovementCategoryUpdateOutcome>.Exception(e.Message);
        }
    }

    public async Task<APIResult<MovementCategoryGetDTO>> DeleteAsync(Guid id, CancellationToken ct)
    {
        if (id == Guid.Empty) return APIResult<MovementCategoryGetDTO>.BadRequest("Invalid id for delete");
        try
        {
            var existing = await db.MovementCategories
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
            log.Log(nameof(MovementCategoryRepository),$"Deleted {existing.Name} with descriptor id : {(existing.Descriptor != null ? existing.Descriptor.Iguid : "null")}");
            
            db.MovementCategories.Remove(existing);
            await db.SaveChangesAsync(ct).ConfigureAwait(false);

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
            log.Log(nameof(MovementCategoryRepository), $"Trying to add MovementCategory Entity {entity.Id} - {entity.Name} - {entity.Iguid} " );

            
            var entry = await db.MovementCategories.AddAsync(entity, ct).ConfigureAwait(false);
            if (entry is { State: EntityState.Added, Entity: not null })
            {
                await db.SaveChangesAsync(ct).ConfigureAwait(false);
                return APIResult<MovementCategoryRecord>.Created(entry.Entity);
            }
            
            log.LogError(nameof(MovementCategoryRepository), $"Could not Insert MovementCategory Entity {entity.GUID} - {entity.Name} - State : {entry.State} " );

            return APIResult<MovementCategoryRecord>.Problem($"Not inserted - State : {entry.State}");
        }
        catch (Exception e)
        {
            log.LogError(nameof(MovementCategoryRepository), $"An exception happened while trying to add MovementCategory Entity {entity.Id} - {entity.Name} - {entity.Iguid} :  {e.GetBaseException().Message}" );

            return APIResult<MovementCategoryRecord>.Exception(e.Message);
        }
    }

}