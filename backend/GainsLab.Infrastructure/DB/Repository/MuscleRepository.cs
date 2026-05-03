using GainsLab.Application.DomainMappers;
using GainsLab.Application.DTOs.Extensions;
using GainsLab.Application.DTOs.Muscle;
using GainsLab.Application.Interfaces.DataManagement.Repository;
using GainsLab.Application.Results.APIResults;
using GainsLab.Contracts;
using GainsLab.Contracts.Dtos.GetDto;
using GainsLab.Contracts.Dtos.PostDto;
using GainsLab.Contracts.Dtos.PutDto;
using GainsLab.Contracts.Dtos.UpdateDto;
using GainsLab.Contracts.Dtos.UpdateDto.Outcome;
using GainsLab.Domain;
using GainsLab.Domain.Comparison;
using GainsLab.Domain.Entities.Identifier;
using GainsLab.Domain.Interfaces;
using GainsLab.Infrastructure.DB.Context;
using GainsLab.Infrastructure.Utilities;
using Microsoft.EntityFrameworkCore;

namespace GainsLab.Infrastructure.DB.Repository;

public class MuscleRepository(GainLabPgDBContext db, IDescriptorRepository descriptorRepository, IClock clock, ILogger log) : IMuscleRepository
{
    
    public async Task<APIResult<MuscleGetDTO>> PullByIdAsync(MuscleId id, CancellationToken ct)
    {
        if (id == Guid.Empty) return APIResult<MuscleGetDTO>.BadRequest("Id cannot be empty");

        try
        {
            var muscle = await GetRecordByIdAsync(id, ct);
            
            return CrudResultUtilities.DispatchResult<MuscleGetDTO, MuscleRecord>(muscle, record => record.ToGetDto()!);
        }
        
        catch (OperationCanceledException)
        {
            return APIResult<MuscleGetDTO>.Exception("Operation was canceled");
        }
        catch (Exception e)
        {
            return APIResult<MuscleGetDTO>.Exception(e.Message);
        }
    }
    
    
    public async Task<APIResult<MuscleGetDTO>> PostAsync(
        MusclePostDTO payload,
        CancellationToken ct)
    {
        try
        {
            if (!payload.Id.HasValue)
            {
                payload = payload with { Id = CoreUtilities.GuidGenerator.New() };
            }
            
            var buildResult = await ValidateAndBuildMuscleForCreateAsync(payload, ct);

            if (!buildResult.Success)
                return buildResult.Error!;

            var entity = buildResult.Entity!;

            entity = await ResolveDescriptor(entity, ct);
            
            log.Log(
                nameof(MuscleRepository),
                $"Post Muscle {entity.Id} {entity.Name} {entity.BodySection}");

            var createdEntity = await CreateMuscleRecordAsync(entity, ct);

            await AddAntagonistRelationsForCreatedMuscleAsync(
                createdEntity,
                payload.AntagonistIds,
                payload.Id!.Value,
                ct);

            var created = await LoadMuscleForGetDtoAsync(createdEntity.Id, ct);

            if (created == null)
                return APIResult<MuscleGetDTO>.Problem("Failed to load created muscle");

            return APIResult<MuscleGetDTO>.Created(created.ToGetDto()!);
        }
        catch (OperationCanceledException)
        {
            return APIResult<MuscleGetDTO>.Exception("Operation was canceled");
        }
        catch (Exception e)
        {
            return APIResult<MuscleGetDTO>.Exception(e.Message);
        }
    }

    private async Task<MuscleRecord?> ResolveDescriptor(MuscleRecord? entity, CancellationToken ct)
    {
        var providedDescriptor = entity?.Descriptor;
        if (providedDescriptor == null)
        {
            return entity;
        }

       var descriptor = await descriptorRepository.GetOrCreateAsync(entity.Descriptor, ct);
       if (!descriptor.Success)
       {
           return entity;
       }

       entity.DescriptorID = descriptor.Value.Id;
       entity.Descriptor = null;
       return entity;
    }

    public async Task<APIResult<MusclePutDTO>> PutAsync(
        MuscleId id,
        MusclePutDTO payload,
        CancellationToken ct)
    {
        if (id == Guid.Empty)
            return APIResult<MusclePutDTO>.BadRequest("Id cannot be empty");

        if (payload == null)
            return APIResult<MusclePutDTO>.BadRequest("Payload cannot be null");

        try
        {
            payload.Id = id;

            var existing = await LoadMuscleForUpdateAsync(id, ct);

            if (existing == null)
            {
                return await CreateMuscleFromPutAsync(id, payload, ct);
            }

            var replacement = payload.ToEntity(clock);

            if (replacement == null)
            {
                return APIResult<MusclePutDTO>.BadRequest(
                    "Could not create muscle entity from payload");
            }

            OverwriteMuscleFields(existing, replacement, payload);

            await ReplaceAntagonistRelationsAsync(existing, payload.AntagonistIds, id, ct);

            await db.SaveChangesAsync(ct).ConfigureAwait(false);

            var updated = await LoadMuscleForPutDtoAsync(existing.Id, ct);

            if (updated == null)
            {
                return APIResult<MusclePutDTO>.Problem(
                    "Failed to load updated muscle");
            }

            return APIResult<MusclePutDTO>.Updated(updated.ToPutDTO(clock, UpsertOutcome.Updated)!);
        }
        catch (OperationCanceledException)
        {
            return APIResult<MusclePutDTO>.Exception("Operation was canceled");
        }
        catch (Exception e)
        {
            return APIResult<MusclePutDTO>.Exception(e.Message);
        }
    }

    public async Task<APIResult<MuscleUpdateOutcome>> PatchAsync(MuscleId id, MuscleUpdateDTO payload, CancellationToken ct)
    {
        if(id == Guid.Empty) return APIResult<MuscleUpdateOutcome>.BadRequest("Payload ID cannot be empty");

        try
        {
            var existing = db.Muscles
                .Where(c=> !c.IsDeleted)
                .Include(m=>m.Descriptor)
                .Include(m=>m.Antagonists)
                    .ThenInclude(link => link.Antagonist)
                .FirstOrDefault(c => c.GUID == id);
            
            if(existing is null) return APIResult<MuscleUpdateOutcome>.NotFound($"Could not find existing muscle with id: {id}");

           
            // Deduplicate antagonist IDs and prevent accidental self-references.
            var antagonistGuids = payload.AntagonistIds?
                .Where(g => g != Guid.Empty && g != id)
                .Distinct()
                .ToList();
            
            var existingGuid =existing.AntagonistGUIDs?
                .Where(g => g != Guid.Empty && g != id)
                .Distinct()
                .ToList();
            
           
            bool antagonistChanged = !SequenceComparison.SetEqualWithDiff(
                antagonistGuids,
                existingGuid,
                g => g,
                out SetDifference<Guid> difference
            );
            
            var antagonistToAdd= antagonistChanged ? difference.ToAdd.ToList() : new List<Guid>();
            var antagonistToRemove  = antagonistChanged ? difference.ToRemove.ToList() : new List<Guid>();
                

            //handle the adding of new antagonist relations
            if (antagonistToAdd.Any())
            {
                await AddNewAntagonistsRelations(existing, antagonistToAdd, ct);
            }
            else log.Log(nameof(MuscleRepository),"No new Antagonists to add");
            
            //handle the removing of new antagonist relations
            if (antagonistToRemove.Any())
            {
                await RemoveAntagonistsRelations(existing, antagonistToRemove, ct);
            }
            else log.Log(nameof(MuscleRepository),"No Antagonists to remove");
                
            var muscleContentChanged = existing.TryUpdate(payload, clock, log) || antagonistChanged;
            
            var descriptorOutcomeState = payload.Descriptor == null ? UpdateOutcome.NotRequested : UpdateOutcome.NotUpdated;
            var descriptorChanged = false;
            DescriptorUpdateOutcome? descriptorOutcome = null;

            if (payload.Descriptor != null)
            {
                //not sure it make sense to make movement category depend on description
                if (existing.Descriptor == null)
                {
                    log.LogWarning(nameof(MuscleRepository), $"No descriptor provided");

                    return APIResult<MuscleUpdateOutcome>.Problem("Descriptor missing for movement category");
                }

                descriptorChanged = existing.Descriptor.TryUpdate(payload.Descriptor, clock);
                descriptorOutcomeState = descriptorChanged ? UpdateOutcome.Updated : UpdateOutcome.NotUpdated;
                descriptorOutcome = new DescriptorUpdateOutcome(descriptorOutcomeState, existing.Descriptor.ToGetDTO());
            }

            if (!muscleContentChanged && !descriptorChanged)
            {
                return APIResult<MuscleUpdateOutcome>.NothingChanged("Nothing changed");
            }

            await db.SaveChangesAsync(ct).ConfigureAwait(false);

            var updatedRecord = await db.Muscles
                .AsNoTracking()
                .Where(m => m.Id == existing.Id)
                .Include(m => m.Descriptor)
                .Include(m => m.Antagonists)
                    .ThenInclude(link => link.Antagonist)
                .FirstOrDefaultAsync(ct)
                .ConfigureAwait(false);

            var updatedState = updatedRecord?.ToGetDto();
            var outcome = new MuscleUpdateOutcome(
                muscleContentChanged ? UpdateOutcome.Updated : UpdateOutcome.NotUpdated,
                descriptorOutcomeState,
                descriptorOutcome,
                updatedState);

            return APIResult<MuscleUpdateOutcome>.Updated(outcome);
        }
        
        
        catch (OperationCanceledException)
        {
            return APIResult<MuscleUpdateOutcome>.Exception("Operation was canceled");
        }
        catch (Exception e)
        {
            return APIResult<MuscleUpdateOutcome>.Exception(e.Message);
        }

    }
    
    public async Task<APIResult<MuscleGetDTO>> DeleteAsync(MuscleId id, CancellationToken ct)
    {
        if(id == Guid.Empty) return APIResult<MuscleGetDTO>.BadRequest("Payload ID cannot be empty");

        log.Log(nameof(MuscleRepository),$"Trying to delete muscle with id {id})");

      
        try
        {
       
          
            var recordResult = await GetRecordByIdAsync(id, ct);
            if (!recordResult.Success || recordResult.Value == null)
            {
                return APIResult<MuscleGetDTO>.NotDeleted(id, EntityType.Muscle);
            }
            var existing = recordResult.Value;
          
            var dto = existing.ToGetDto();
           
            db.Muscles.Remove(existing);
            await db.SaveChangesAsync(ct).ConfigureAwait(false);

            return APIResult<MuscleGetDTO>.Deleted(dto!);
          
        }
        catch (OperationCanceledException)
        {
            return APIResult<MuscleGetDTO>.Exception("Operation was canceled");
        }
        catch (Exception e)
        {
            return APIResult<MuscleGetDTO>.Exception(e.Message);
        }
    }
    
        private async Task<APIResult<MuscleRecord>> GetRecordByIdAsync(Guid? id, CancellationToken ct)
    {
        if(!id.HasValue || id == Guid.Empty) 
            return APIResult<MuscleRecord>.BadRequest("Id cannot be null or empty");
        
        try
        {
            var muscle = await db.Muscles
                .AsNoTracking()
                .Include(e=>e.Descriptor)
                .Include(m=>m.Antagonists)
                .ThenInclude(link => link.Antagonist)
                .Where(e => !e.IsDeleted)
                .FirstOrDefaultAsync(e=> e.GUID == id.Value, ct);
            
            
            return muscle != null ? 
                APIResult<MuscleRecord>.Found(muscle!):
                APIResult<MuscleRecord>.NotFound(id.Value.ToString());

        }
        catch (Exception e)
        {
            return APIResult<MuscleRecord>.Exception(e.Message);
        }
    }
    
    private async Task<APIResult<MuscleRecord>> GetRecordByNameAsync(string name, CancellationToken ct)
    {
        var nameValidation =
            CrudValidation.ValidateRequiredText<MuscleRecord>(
                name,
                "Name",
                text => text.Trim());

        if (nameValidation != null)
            return nameValidation;
        
        try
        {
            var muscle = await db.Muscles
                .AsNoTracking()
                .Include(e=>e.Descriptor)
                .Include(m=>m.Antagonists)
                .ThenInclude(link => link.Antagonist)
                .Where(e => !e.IsDeleted)
                .FirstOrDefaultAsync(e=> e.Name.ToLower().Trim() == name.ToLower(), ct);
            
            return muscle != null ? 
                APIResult<MuscleRecord>.Found(muscle!):
                APIResult<MuscleRecord>.NotFound(name);

        }
        catch (Exception e)
        {
            return APIResult<MuscleRecord>.Exception(e.Message);
        }
    }
    
    private Task<UniqueValidationResult<MuscleGetDTO>> TryValidateUnique(
        MuscleRecord entity,
        CancellationToken ct)
    {
        return CrudValidation.TryValidateUniqueAsync<MuscleRecord, MuscleGetDTO>(
            entity,
            EntityType.Muscle,
            getId: x => x.GUID,
            getName: x => x.Name,
            getContent: x => null,
            getOther: x => null,
            getExistingRecordAsync: (x, token) => GetExistingRecordAsync(x.GUID, x.Name, token),
            ct);
    }
    
    private Task<MatchingResult<MuscleRecord>> GetExistingRecordAsync(
        Guid? id,
        string name,
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
    
    private async Task<APIResult<MusclePutDTO>> CreateMuscleFromPutAsync(
        MuscleId id,
        MusclePutDTO payload,
        CancellationToken ct)
    {
        payload.Id = id;

        var postPayload = payload.ToPostDto(id);
       
        var createdResult = await PostAsync(postPayload, ct);

        if (!createdResult.Success || createdResult.Value == null)
        {
            return APIResult<MusclePutDTO>.NotCreated(
                createdResult.GetErrorMessage(),
                NotCreatedReason.Other);
        }

        var created = await db.Muscles
            .AsNoTracking()
            .Where(m => m.GUID == id && !m.IsDeleted)
            .Include(m => m.Descriptor)
            .Include(m => m.Antagonists)
            .ThenInclude(link => link.Antagonist)
            .FirstOrDefaultAsync(ct)
            .ConfigureAwait(false);

        if (created == null)
        {
            return APIResult<MusclePutDTO>.Problem(
                "Muscle was created but could not be loaded");
        }

        return APIResult<MusclePutDTO>.Created(
            created.ToPutDTO(clock, UpsertOutcome.Created)!);
    }
    
    private Task<MuscleRecord?> LoadMuscleForUpdateAsync(
        MuscleId id,
        CancellationToken ct)
    {
        return db.Muscles
            .Where(m => !m.IsDeleted)
            .Include(m => m.Descriptor)
            .Include(m => m.Antagonists)
            .ThenInclude(link => link.Antagonist)
            .FirstOrDefaultAsync(m => m.GUID == id, ct);
    }
    
    private void OverwriteMuscleFields(
        MuscleRecord existing,
        MuscleRecord replacement,
        MusclePutDTO payload)
    {
        existing.Name = replacement.Name;
        existing.BodySection = replacement.BodySection;
        existing.Authority = replacement.Authority;

        existing.UpdatedAtUtc = clock.UtcNow;
        existing.UpdatedBy = payload.UpdatedBy;

        if (replacement.Descriptor != null)
        {
            if (existing.Descriptor == null)
            {
                existing.Descriptor = replacement.Descriptor;
            }
            else
            {
                existing.Descriptor.Content = replacement.Descriptor.Content;
                existing.Descriptor.UpdatedAtUtc = clock.UtcNow;
                existing.Descriptor.UpdatedBy = payload.UpdatedBy;
                existing.Descriptor.Authority = payload.Authority;
            }
        }
    }
    
    private async Task ReplaceAntagonistRelationsAsync(
        MuscleRecord existing,
        IEnumerable<Guid>? requestedAntagonistIds,
        Guid ownGuid,
        CancellationToken ct)
    {
        var requested = requestedAntagonistIds?
                            .Where(g => g != Guid.Empty && g != ownGuid)
                            .Distinct()
                            .ToList()
                        ?? new List<Guid>();

        var existingGuids = existing.AntagonistGUIDs?
                                .Where(g => g != Guid.Empty && g != ownGuid)
                                .Distinct()
                                .ToList()
                            ?? new List<Guid>();

        var changed = !SequenceComparison.SetEqualWithDiff(
            requested,
            existingGuids,
            g => g,
            out SetDifference<Guid> difference);

        if (!changed)
        {
            log.Log(nameof(MuscleRepository), $"No antagonist relation changes for {existing.GUID}");
            return;
        }

        var toAdd = difference.ToAdd.ToList();
        var toRemove = difference.ToRemove.ToList();

        if (toRemove.Count > 0)
        {
            await RemoveAntagonistsRelations(existing, toRemove, ct)
                .ConfigureAwait(false);
        }

        if (toAdd.Count > 0)
        {
            await AddNewAntagonistsRelations(existing, toAdd, ct)
                .ConfigureAwait(false);
        }
    }
    
    private Task<MuscleRecord?> LoadMuscleForPutDtoAsync(
        int muscleDbId,
        CancellationToken ct)
    {
        return db.Muscles
            .AsNoTracking()
            .Where(m => m.Id == muscleDbId && !m.IsDeleted)
            .Include(m => m.Descriptor)
            .Include(m => m.Antagonists)
            .ThenInclude(link => link.Antagonist)
            .FirstOrDefaultAsync(ct);
    }
    
    private async Task<MuscleRecord> CreateMuscleRecordAsync(
        MuscleRecord entity,
        CancellationToken ct)
    {
        var entry = await db.Muscles
            .AddAsync(entity, ct)
            .ConfigureAwait(false);

        await db.SaveChangesAsync(ct).ConfigureAwait(false);

        return entry.Entity;
    }
    
    private static List<Guid> GetValidAntagonistGuids(
        IEnumerable<Guid>? antagonistIds,
        Guid ownGuid)
    {
        return antagonistIds?
                   .Where(g => g != Guid.Empty && g != ownGuid)
                   .Distinct()
                   .ToList()
               ?? new List<Guid>();
    }
    
    
    private async Task AddAntagonistRelationsForCreatedMuscleAsync(
        MuscleRecord createdMuscle,
        IEnumerable<Guid>? antagonistIds,
        Guid payloadId,
        CancellationToken ct)
    {
        var antagonistGuids = GetValidAntagonistGuids(antagonistIds, payloadId);

        if (antagonistGuids.Count == 0)
            return;

        var resolvedAntagonists = await ResolveAntagonistIdsAsync(
            antagonistGuids,
            payloadId,
            ct);

        if (resolvedAntagonists.Count == 0)
            return;

        var newRelations = await BuildMissingAntagonistRelationsAsync(
            createdMuscle.Id,
            resolvedAntagonists,
            payloadId,
            ct);

        if (newRelations.Count == 0)
            return;

        await db.MuscleAntagonists
            .AddRangeAsync(newRelations, ct)
            .ConfigureAwait(false);

        await db.SaveChangesAsync(ct).ConfigureAwait(false);
    }
    private async Task<EntityBuildResult<MuscleRecord, MuscleGetDTO>>
        ValidateAndBuildMuscleForCreateAsync(
            MusclePostDTO payload,
            CancellationToken ct)
    {
        var validation =
            CrudValidation.ValidatePayloadAndBuildEntity<
                MusclePostDTO,
                MuscleRecord,
                MuscleGetDTO>(
                payload,
                EntityType.Muscle,
                p=> p.Id,
                p => p.ToEntity(clock));

        if (!validation.Success)
            return validation;

        var entity = validation.Entity!;

        var uniqueValidation = await TryValidateUnique(entity, ct);

        if (uniqueValidation.AlreadyExists)
        {
            return EntityBuildResult<MuscleRecord, MuscleGetDTO>.Fail(
                uniqueValidation.ExistingResult!);
        }

        return EntityBuildResult<MuscleRecord, MuscleGetDTO>.Ok(entity);
    }
    
    private async Task<List<MuscleAntagonistRecord>> BuildMissingAntagonistRelationsAsync(
        int muscleDbId,
        Dictionary<Guid, int> resolvedAntagonists,
        Guid muscleGuid,
        CancellationToken ct)
    {
        var antagonistIdSet = resolvedAntagonists.Values.ToHashSet();

        var existingRelationIds = await db.MuscleAntagonists
            .Where(link =>
                link.MuscleId == muscleDbId &&
                antagonistIdSet.Contains(link.AntagonistId))
            .Select(link => link.AntagonistId)
            .ToListAsync(ct)
            .ConfigureAwait(false);

        var existingSet = existingRelationIds.ToHashSet();

        var newRelations = new List<MuscleAntagonistRecord>();

        foreach (var (guid, antagonistId) in resolvedAntagonists)
        {
            if (existingSet.Contains(antagonistId))
            {
                log.Log(
                    nameof(MuscleRepository),
                    $"Reusing existing muscle-antagonist relation for {muscleGuid} <-> {guid}");

                continue;
            }

            newRelations.Add(new MuscleAntagonistRecord
            {
                MuscleId = muscleDbId,
                AntagonistId = antagonistId
            });
        }

        return newRelations;
    }
    
    private Task<MuscleRecord?> LoadMuscleForGetDtoAsync(
        int muscleDbId,
        CancellationToken ct)
    {
        return db.Muscles
            .AsNoTracking()
            .Where(m => m.Id == muscleDbId)
            .Include(m => m.Descriptor)
            .Include(m => m.Antagonists)
            .ThenInclude(link => link.Antagonist)
            .FirstOrDefaultAsync(ct);
    }
    
    private async Task<Dictionary<Guid, int>> ResolveAntagonistIdsAsync(
        List<Guid> antagonistGuids,
        Guid muscleGuid,
        CancellationToken ct)
    {
        var antagonists = await db.Muscles
            .AsNoTracking()
            .Where(m => antagonistGuids.Contains(m.GUID) && !m.IsDeleted)
            .Select(m => new { m.GUID, m.Id })
            .ToListAsync(ct)
            .ConfigureAwait(false);

        var resolvedIds = antagonists.ToDictionary(x => x.GUID, x => x.Id);

        var missing = antagonistGuids
            .Except(resolvedIds.Keys)
            .ToList();

        if (missing.Count > 0)
        {
            log.LogWarning(
                nameof(MuscleRepository),
                $"Antagonists not found for muscle {muscleGuid}: {string.Join(", ", missing)}");
        }

        return resolvedIds;
    }

    private async Task RemoveAntagonistsRelations(MuscleRecord existing, List<Guid> antagonistToRemove, CancellationToken ct)
    {
        var toRemoveRecords = existing.Antagonists
            .Where(link => link.Antagonist != null && antagonistToRemove.Contains(link.Antagonist.GUID))
            .ToList();

        if (toRemoveRecords.Count == 0)
        {
            var antagonistIds = await db.Muscles
                .Where(m => antagonistToRemove.Contains(m.GUID) && !m.IsDeleted)
                .Select(m => m.Id)
                .ToListAsync(ct)
                .ConfigureAwait(false);

            if (antagonistIds.Count > 0)
            {
                toRemoveRecords = await db.MuscleAntagonists
                    .Where(link => link.MuscleId == existing.Id && antagonistIds.Contains(link.AntagonistId))
                    .ToListAsync(ct)
                    .ConfigureAwait(false);
            }
        }

        if (toRemoveRecords.Count > 0)
        {
            db.MuscleAntagonists.RemoveRange(toRemoveRecords);
            foreach (var relation in toRemoveRecords)
            {
                existing.Antagonists.Remove(relation);
            }

            log.Log(nameof(MuscleRepository),
                $"Removed {toRemoveRecords.Count} antagonist relations for {existing.GUID}");
        }
    }

    private async Task AddNewAntagonistsRelations(
        MuscleRecord existing, 
        List<Guid> antagonistToAdd, 
        CancellationToken ct)
    {
        var resolvedAntagonists = await db.Muscles
            .Where(m => antagonistToAdd.Contains(m.GUID) && !m.IsDeleted)
            .Select(m => new { m.GUID, m.Id })
            .ToListAsync(ct)
            .ConfigureAwait(false);

        var resolved = resolvedAntagonists.ToDictionary(x => x.GUID, x => x.Id);
        var missing = antagonistToAdd.Where(g => !resolved.ContainsKey(g)).ToList();
        if (missing.Count > 0)
        {
            log.LogWarning(nameof(MuscleRepository),
                $"Unable to resolve antagonists for {existing.GUID}: {string.Join(", ", missing)}");
        }

        var newRelations = new List<MuscleAntagonistRecord>();
        foreach (var guid in antagonistToAdd)
        {
            if (!resolved.TryGetValue(guid, out var antagonistId))
            {
                continue;
            }

            newRelations.Add(new MuscleAntagonistRecord
            {
                MuscleId = existing.Id,
                AntagonistId = antagonistId
            });
        }

        if (newRelations.Count > 0)
        {
            await db.MuscleAntagonists.AddRangeAsync(newRelations, ct).ConfigureAwait(false);
            foreach (var relation in newRelations)
            {
                existing.Antagonists.Add(relation);
            }

            log.Log(nameof(MuscleRepository),
                $"Added {newRelations.Count} antagonist relations for {existing.GUID}");
        }
    }
}
