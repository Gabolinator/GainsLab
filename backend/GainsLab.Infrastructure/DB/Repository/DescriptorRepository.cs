using GainsLab.Application.DomainMappers;
using GainsLab.Application.DTOs.Description;
using GainsLab.Application.DTOs.Extensions;
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


//for crud operation on descriptor
public class DescriptorRepository(GainLabPgDBContext db, IClock clock, ILogger log) : IDescriptorRepository
{
    
    //note : no Delete() as its handled in cascading delete of owning parent aggregate 

    public async Task<APIResult<DescriptorGetDTO>> PullByIdAsync(Guid id, CancellationToken ct)
    {
        if(id == Guid.Empty) return APIResult<DescriptorGetDTO>.BadRequest("Id cannot be empty");
        
        try
        {
            var description = await GetRecordByIdAsync(id, ct);
            return CrudResultUtilities.DispatchResult<DescriptorGetDTO, DescriptorRecord>(description, record => record.ToGetDTO()!);
        }
        catch (Exception e)
        {
            return APIResult<DescriptorGetDTO>.Exception(e.Message);
        }
    }

    private async Task<APIResult<DescriptorRecord>> GetRecordByIdAsync(Guid? id, CancellationToken ct)
    {
         if(!id.HasValue || id == Guid.Empty) 
                    return APIResult<DescriptorRecord>.BadRequest("Id cannot be null or empty");
         try
         {
             var descriptions = await db.Descriptors
                 .AsNoTracking()
                 .Where(d => !d.IsDeleted)
                 .FirstOrDefaultAsync(d => d.GUID == id.Value, ct);

            
            
             return descriptions != null ? 
                 APIResult<DescriptorRecord>.Found(descriptions!):
                 APIResult<DescriptorRecord>.NotFound(id.Value.ToString());

         }
         catch (Exception e)
         {
             return APIResult<DescriptorRecord>.Exception(e.Message);
         }
    }
    
    private async Task<APIResult<DescriptorRecord>> GetRecordByContentAsync(string content, CancellationToken ct)
    {
        var formatedContent = content.Trim();
        if(string.IsNullOrWhiteSpace(formatedContent)) return APIResult<DescriptorRecord>.BadRequest("Content cannot be empty");
        
        try
        {
            var descriptions = await db.Descriptors
                .AsNoTracking()
                .Where(d => !d.IsDeleted)
                .FirstOrDefaultAsync(d => d.Content.Trim().ToLower() == formatedContent, ct);
            
            return descriptions != null ? 
                APIResult<DescriptorRecord>.Found(descriptions!):
                APIResult<DescriptorRecord>.NotFound(content);

        }
        catch (Exception e)
        {
            return APIResult<DescriptorRecord>.Exception(e.Message);
        }
    }

    public async Task<APIResult<DescriptorGetDTO>> PostAsync(DescriptorPostDTO payload, CancellationToken ct)
    {
        try
        {
            var entity = payload.ToEntity(clock);           // GUID created here
            if (entity == null) return APIResult<DescriptorGetDTO>.BadRequest("Could not create record from dto");
            
            var existing = await TryValidateUnique(entity, ct);
            if (existing.AlreadyExists)
            {
                return existing.ExistingResult!;
            }

            var record = await CreateAsync(entity, ct);
            
            //if success => value != null
            return record.Success  ? APIResult<DescriptorGetDTO>.Created(record.Value.ToGetDTO()!) : APIResult<DescriptorGetDTO>.NotCreated("Failed to create record", NotCreatedReason.Other);
            
        }
        catch (Exception e)
        {
            return APIResult<DescriptorGetDTO>.Exception(e.Message);
        }
        
    }

    private Task<MatchingResult<DescriptorRecord>> GetExistingRecordAsync(
        Guid? id,
        string content,
        CancellationToken ct)
    {
        return CrudResultUtilities.GetExistingRecordAsync(
            id: id,
            name: null,
            content: content,
            other: null,
            getById: GetRecordByIdAsync,
            getByName: null,
            getByContent: GetRecordByContentAsync,
            getByOther: null,
            ct: ct);
    }

    
    private Task<UniqueValidationResult<DescriptorGetDTO>> TryValidateUnique(
        DescriptorRecord entity,
        CancellationToken ct)
    {
        return CrudResultUtilities.TryValidateUniqueAsync<DescriptorRecord, DescriptorGetDTO>(
            entity,
            EntityType.Descriptor,
            getId: x => x.GUID,
            getName: x => null,
            getContent: x => x.Content,
            getOther: x => null,
            getExistingRecordAsync: (x, token) => GetExistingRecordAsync(x.GUID, x.Content, token),
            ct);
    }

    public async Task<APIResult<DescriptorPutDTO>> PutAsync(Guid id, DescriptorPutDTO payload, CancellationToken ct)
    {
        try
        {
           if(id == Guid.Empty) return APIResult<DescriptorPutDTO>.BadRequest("Id cannot be empty");

            var existing = await db.Descriptors.FirstOrDefaultAsync(d => d.GUID == id && !d.IsDeleted, ct);

            if (existing is null)
            {
                // create via shared method
                var entity = payload.ToEntity(clock, id)!; // guid may be null -> create a new one inside mapping OR enforce not null
                var created = await CreateAsync(entity, ct);

                
                return !created.Success ? 
                    APIResult<DescriptorPutDTO>.NotCreated(created.GetErrorMessage() ?? "Create failed", NotCreatedReason.Other) : 
                    APIResult<DescriptorPutDTO>.Created(created.Value!.ToPutDTO(clock, UpsertOutcome.Created)!);
            }

            payload.Id = id;
            
            
            //nothing changed
            if(!existing.AnythingChanged(payload)) 
                return APIResult<DescriptorPutDTO>.NothingChanged($"For entity : {payload.Id}");
            
            //put always updates all field
            
            // update branch
            existing.Content = payload.DescriptionContent;
            existing.UpdatedAtUtc = clock.UtcNow;
            existing.UpdatedBy = payload.UpdatedBy;
            existing.Authority = payload.Authority;

            await db.SaveChangesAsync(ct).ConfigureAwait(false);
            return APIResult<DescriptorPutDTO>.Updated(existing.ToPutDTO(clock, UpsertOutcome.Updated)!);
        }
        catch (Exception e)
        {
            return APIResult<DescriptorPutDTO>.Exception(e.Message);
        }
    }

    public async Task<APIResult<DescriptorUpdateOutcome>> PatchAsync(Guid id, DescriptorUpdateDTO payload, CancellationToken ct)
    {

        try
        {
            var description = id.Equals(Guid.Empty)? null: await db.Descriptors.FirstOrDefaultAsync(d => d.GUID == id  && !d.IsDeleted, ct);
            if(description == null) 
                return APIResult<DescriptorUpdateOutcome>.NotUpdated("Not found for update");
            
            if (description.TryUpdate(payload, clock))
            {
                await  db.SaveChangesAsync(ct);
                
                return APIResult<DescriptorUpdateOutcome>.Updated(new DescriptorUpdateOutcome(UpdateOutcome.Updated, description.ToGetDTO()));
            }
            
            return  APIResult<DescriptorUpdateOutcome>.NothingChanged("Nothing changed");

        }
        catch (Exception e)
        {
            return APIResult<DescriptorUpdateOutcome>.Exception(e.Message);
        }
        
    }
    
    public async Task<APIResult<DescriptorRecord>> CreateAsync(
        DescriptorRecord entity,
        CancellationToken ct)
    {
        try
        {
            var entry = await db.Descriptors.AddAsync(entity, ct).ConfigureAwait(false);
            if (entry is { State: EntityState.Added, Entity: not null })
            {
                await db.SaveChangesAsync(ct).ConfigureAwait(false);
                return APIResult<DescriptorRecord>.Created(entry.Entity);
            }

            return APIResult<DescriptorRecord>.Problem("Not inserted");
        }
        catch (Exception e)
        {
            return APIResult<DescriptorRecord>.Exception(e.Message);
        }
    }

    public async Task<APIResult<DescriptorRecord>> GetOrCreateAsync(DescriptorRecord entity, CancellationToken ct)
    {
        var existing = await GetExistingRecordAsync(entity.GUID, entity.Content, ct);
        if (existing.MatchFound)
        {
            return APIResult<DescriptorRecord>.Found(existing.Result!.Value!);
        }

        return await CreateAsync(entity, ct);
    }

    
}