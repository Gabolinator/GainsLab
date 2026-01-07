using System.Text.Json;
using GainsLab.Application.DomainMappers;
using GainsLab.Application.DTOs;
using GainsLab.Application.DTOs.Description;
using GainsLab.Application.Interfaces;
using GainsLab.Application.Results.APIResults;
using GainsLab.Contracts.Dtos.GetDto;
using GainsLab.Contracts.Dtos.PostDto;
using GainsLab.Contracts.Dtos.PutDto;
using GainsLab.Contracts.Dtos.UpdateDto;
using GainsLab.Domain.Interfaces;
using GainsLab.Infrastructure.DB.Context;
using Microsoft.EntityFrameworkCore;

namespace GainsLab.Infrastructure.DB.Repository;


//for crud operation on descriptor
public class DescriptorRepository : IDescriptorRepository
{
    
    //note : no Delete() as its handled in cascading delete of owning parent aggregate 
    
    private readonly GainLabPgDBContext _db;
    private readonly IClock _clock;
    private readonly ILogger _log;
    
    
    private const string SyncActor = "repo";

    public DescriptorRepository(GainLabPgDBContext db, IClock clock, ILogger log)
    {
        _db = db;
        _clock = clock;
        _log = log;
    }
    
    public async Task<APIResult<DescriptorGetDTO>> PullByIdAsync(Guid id, CancellationToken ct)
    {
        try
        {
            var descriptions = await _db.Descriptors
                .AsNoTracking()
                .Where(d => !d.IsDeleted)
                .FirstOrDefaultAsync(d => d.GUID == id, ct);

            
            
            return descriptions != null ? 
                APIResult<DescriptorGetDTO>.Found(descriptions.ToGetDTO()!):
                APIResult<DescriptorGetDTO>.NotFound(id.ToString());

        }
        catch (Exception e)
        {
            return APIResult<DescriptorGetDTO>.Exception(e.Message);
        }
    }

    public async Task<APIResult<DescriptorGetDTO>> PostAsync(DescriptorPostDTO payload, CancellationToken ct)
    {
        try
        {
            var entity = payload.ToEntity(_clock);           // GUID created here
            if (entity == null) return APIResult<DescriptorGetDTO>.BadRequest("Could not create record from dto");
       
            var record = await CreateAsync(entity, ct);
            
            //if success => value != null
            return record.Success  ? APIResult<DescriptorGetDTO>.Created(record.Value.ToGetDTO()!) : APIResult<DescriptorGetDTO>.NotCreated("Failed to create record");
            
        }
        catch (Exception e)
        {
            return APIResult<DescriptorGetDTO>.Exception(e.Message);
        }
        
    }
    
    
    public async Task<APIResult<DescriptorPutDTO>> PutAsync(Guid id, DescriptorPutDTO payload, CancellationToken ct)
    {
        try
        {
           if(id == Guid.Empty) return APIResult<DescriptorPutDTO>.BadRequest("Id cannot be empty");

            var existing = await _db.Descriptors.FirstOrDefaultAsync(d => d.GUID == id && !d.IsDeleted, ct);

            if (existing is null)
            {
                // create via shared method
                var entity = payload.ToEntity(_clock, id)!; // guid may be null -> create a new one inside mapping OR enforce not null
                var created = await CreateAsync(entity, ct);

                
                return !created.Success ? 
                    APIResult<DescriptorPutDTO>.NotCreated(created.ErrorMessage ?? "Create failed") : 
                    APIResult<DescriptorPutDTO>.Created(created.Value!.ToPutDTO(_clock, UpsertOutcome.Created)!);
            }

            payload.Id = id;
            
            //nothing changed
            if(!existing.AnythingChanged(payload)) 
                return APIResult<DescriptorPutDTO>.NothingChanged($"For entity : {payload.Id}");
            
            //put always updates all field
            
            // update branch
            existing.Content = payload.DescriptionContent;
            existing.UpdatedAtUtc = _clock.UtcNow;
            existing.UpdatedBy = payload.UpdatedBy;
            existing.Authority = payload.Authority;

            await _db.SaveChangesAsync(ct).ConfigureAwait(false);
            return APIResult<DescriptorPutDTO>.Updated(existing.ToPutDTO(_clock, UpsertOutcome.Updated)!);
        }
        catch (Exception e)
        {
            return APIResult<DescriptorPutDTO>.Exception(e.Message);
        }
    }

    public async Task<APIResult<DescriptorUpdateDTO>> PatchAsync(Guid id, DescriptorUpdateDTO payload, CancellationToken ct)
    {

        try
        {
            var description = id.Equals(Guid.Empty)? null: await _db.Descriptors.FirstOrDefaultAsync(d => d.GUID == id  && !d.IsDeleted, ct);
            if(description == null) 
                return APIResult<DescriptorUpdateDTO>.NotUpdated("Not found for update");

        
            
            if (description.TryUpdate(payload, _clock))
            {
              
                await  _db.SaveChangesAsync(ct);
                return APIResult<DescriptorUpdateDTO>.Updated(payload);
            }
            
            return  APIResult<DescriptorUpdateDTO>.NothingChanged("Nothing changed");

        }
        catch (Exception e)
        {
            return APIResult<DescriptorUpdateDTO>.Exception(e.Message);
        }
        
    }
    
    public async Task<APIResult<DescriptorRecord>> CreateAsync(
        DescriptorRecord entity,
        CancellationToken ct)
    {
        try
        {
            var entry = await _db.Descriptors.AddAsync(entity, ct).ConfigureAwait(false);
            if (entry is { State: EntityState.Added, Entity: not null })
            {
                await _db.SaveChangesAsync(ct).ConfigureAwait(false);
                return APIResult<DescriptorRecord>.Created(entry.Entity);
            }

            return APIResult<DescriptorRecord>.Problem("Not inserted");
        }
        catch (Exception e)
        {
            return APIResult<DescriptorRecord>.Exception(e.Message);
        }
    }
    
    
}