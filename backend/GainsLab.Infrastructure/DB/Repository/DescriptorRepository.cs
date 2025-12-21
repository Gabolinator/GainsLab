using System.Text.Json;
using GainsLab.Application.DomainMappers;
using GainsLab.Application.DTOs;
using GainsLab.Application.Interfaces;
using GainsLab.Application.Results;
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
    
    public async Task<Result<DescriptorGetDTO>> PullByIdAsync(Guid id, CancellationToken ct)
    {
        try
        {
            var descriptions = await _db.Descriptors
                .AsNoTracking()
                .Where(d => !d.IsDeleted)
                .FirstOrDefaultAsync(d => d.GUID == id, ct);

            return descriptions != null ? Result<DescriptorGetDTO>.SuccessResult(descriptions.ToGetDTO()!):Result<DescriptorGetDTO>.Failure("Not Found");

        }
        catch (Exception e)
        {
            return Result<DescriptorGetDTO>.Failure(e.Message);
        }
    }

    public async Task<Result<DescriptorRecord>> PostAsync(DescriptorPostDTO payload, CancellationToken ct)
    {
        try
        {
            var newEntry = await _db.AddAsync(payload.ToEntity(_clock), ct).ConfigureAwait(false);
            if (newEntry is { State: EntityState.Added, Entity: not null })
            {
                await _db.SaveChangesAsync(ct);
                
                return Result<DescriptorRecord>.SuccessResult(newEntry.Entity);
            }
            
            return Result<DescriptorRecord>.Failure("Not inserted");

        }
        catch (Exception exception)
        {
            return Result<DescriptorRecord>.Failure(exception.Message);
        }
    }


    public async Task<Result<DescriptorPutDTO>> PutAsync(Guid? id, DescriptorPutDTO payload, CancellationToken ct)
    {
        try
        {
            var description = id == null || id.Equals(Guid.Empty)? null: await _db.Descriptors.FirstOrDefaultAsync(d => d.GUID == id  && !d.IsDeleted, ct);

            if (description ==null )
            {
                //new entity 
                //insert
               var newEntry = await _db.Descriptors.AddAsync(payload.ToEntity(_clock, id)!, ct).ConfigureAwait(false);
               
               if (newEntry is { State: EntityState.Added, Entity: not null })
               {
                   await _db.SaveChangesAsync(ct);
                
                   return Result<DescriptorPutDTO>.SuccessResult(newEntry.Entity.ToPutDTO(_clock)!);
               }
            
               return Result<DescriptorPutDTO>.Failure("Not inserted for updates");
               
            }
            
            //replace content of existing entity
            description.Content = payload.DescriptionContent;
            description.UpdatedAtUtc = _clock.UtcNow;
            description.UpdatedBy = payload.UpdatedBy;
            description.Authority = payload.Authority;
            
            await _db.SaveChangesAsync(ct);

            return  Result<DescriptorPutDTO>.SuccessResult(description.ToPutDTO(_clock)!);
            
        }
        catch (Exception exception)
        {
            return Result<DescriptorPutDTO>.Failure(exception.Message);
        }
    }

    public async Task<Result<DescriptorUpdateDTO>> PatchAsync(Guid id, DescriptorUpdateDTO payload, CancellationToken ct)
    {

        try
        {
            var description = id.Equals(Guid.Empty)? null: await _db.Descriptors.FirstOrDefaultAsync(d => d.GUID == id  && !d.IsDeleted, ct);
            if(description == null) return Result<DescriptorUpdateDTO>.Failure("Not found for update");

        
            bool anyUpdate = false;
            if (payload.DescriptionContent != null)
            {
                description.Content = payload.DescriptionContent;
                anyUpdate = true;
            }
        
            if (payload.Authority != null)
            {
                description.Authority = payload.Authority.Value;
                anyUpdate = true;
            }

            if (anyUpdate)
            {
                description.UpdatedAtUtc = _clock.UtcNow;
                description.UpdatedBy = payload.UpdatedBy;
                await  _db.SaveChangesAsync(ct);
            }
            
            return Result<DescriptorUpdateDTO>.SuccessResult(payload);

        }
        catch (Exception e)
        {
            return Result<DescriptorUpdateDTO>.Failure(e.Message);
        }
        
    }
    
    
}