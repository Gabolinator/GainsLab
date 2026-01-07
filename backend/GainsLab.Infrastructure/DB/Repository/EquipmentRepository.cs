using GainsLab.Application.DomainMappers;
using GainsLab.Application.DTOs;
using GainsLab.Application.DTOs.Description;
using GainsLab.Application.DTOs.Equipment;
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

public class EquipmentRepository : IEquipmentRepository
{
    private readonly GainLabPgDBContext _db;
    private readonly IClock _clock;
    private readonly ILogger _log;
    private readonly IDescriptorRepository _descriptorRepository;
    
    public EquipmentRepository(GainLabPgDBContext db, IDescriptorRepository descriptorRepository ,IClock clock, ILogger log)
    {
        _db = db;
        _clock = clock;
        _log = log;
        _descriptorRepository =  descriptorRepository;
    }
    
    public async Task<APIResult<EquipmentGetDTO>> PullByIdAsync(Guid id, CancellationToken ct)
    {
        try
        {
            var equipment = await _db.Equipments
                .AsNoTracking()
                .Include(e=>e.Descriptor)
                .Where(e => !e.IsDeleted)
                .FirstOrDefaultAsync(e=> e.GUID == id, ct);
            
            return equipment != null ? 
                APIResult<EquipmentGetDTO>.Found(equipment.ToGetDTO()!) 
                :  APIResult<EquipmentGetDTO>.NotFound(id.ToString());
        }
        
        catch (Exception e)
        {
            return APIResult<EquipmentGetDTO>.Exception(e.Message);
        }
    }

    public async Task<APIResult<EquipmentGetDTO>> PostAsync(EquipmentPostDTO payload, CancellationToken ct)
    {
        try
        {
            var entity = payload.ToEntity(_clock);           // GUID created here
            if (entity == null) return APIResult<EquipmentGetDTO>.BadRequest("Could not create record from dto");
       
            //descriptor is created inside create async
            var record = await CreateAsync(entity, ct);
            
            //if success => value != null
            return record.Success  ? 
                APIResult<EquipmentGetDTO>.Created(record.Value.ToGetDTO()!) 
                : APIResult<EquipmentGetDTO>.NotCreated("Failed to create record");
            
        }
        catch (Exception e)
        {
            return APIResult<EquipmentGetDTO>.Exception(e.Message);
        }
    }

  
    public async Task<APIResult<EquipmentPutDTO>> PutAsync(Guid id, EquipmentPutDTO payload, CancellationToken ct)
    {
        try
        {
            if(id == Guid.Empty) return APIResult<EquipmentPutDTO>.BadRequest("Id cannot be empty");

            var existing = 
                await _db.Equipments
                    .Where(e=> !e.IsDeleted)
                    .Include(equipmentRecord => equipmentRecord.Descriptor)
                    .FirstOrDefaultAsync(d => d.GUID == id, ct);

            if (existing is null)
            {
                // create via shared method
                var entity = payload.ToEntity(_clock, id)!; // guid may be null -> create a new one inside mapping OR enforce not null
                var created = await CreateAsync(entity, ct);

                
                return !created.Success ? 
                    APIResult<EquipmentPutDTO>.NotCreated(created.ErrorMessage ?? "Create failed") : 
                    APIResult<EquipmentPutDTO>.Created(created.Value!.ToPutDTO(_clock, UpsertOutcome.Created)!);
            }

            payload.Id = id;
            
            //nothing changed
            if(!existing.AnythingChanged(payload)) 
                return APIResult<EquipmentPutDTO>.NothingChanged($"For entity : {payload.Id}");

            // update branch
            existing.Name = payload.Name;
            existing.UpdatedAtUtc = _clock.UtcNow;
            existing.UpdatedBy = payload.UpdatedBy;
            existing.Authority = payload.Authority;

            // ensure descriptor identifier is set
            if (payload.Descriptor.Id == null || payload.Descriptor.Id == Guid.Empty)
            {
                payload.Descriptor.Id = existing.Descriptor?.GUID ?? Guid.NewGuid();
            }
            
            var descriptorResult = await _descriptorRepository.PutAsync(payload.Descriptor.Id.Value, payload.Descriptor, ct);
            if (!descriptorResult.Success || descriptorResult.Value?.Id == null)
            {
                var error = descriptorResult.ErrorMessage ?? "Descriptor update failed";
                return APIResult<EquipmentPutDTO>.Exception(error);
            }

            // refresh descriptor reference on the equipment
            var descriptorRecord = await _db.Descriptors
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.GUID == payload.Descriptor.Id!.Value, ct);

            if (descriptorRecord == null)
            {
                return APIResult<EquipmentPutDTO>.Problem($"Descriptor {payload.Descriptor.Id} not found after update");
            }

            existing.Descriptor = descriptorRecord;
            existing.DescriptorID = descriptorRecord.Id;
            
            await _db.SaveChangesAsync(ct).ConfigureAwait(false);
            return APIResult<EquipmentPutDTO>.Updated(existing.ToPutDTO(_clock, UpsertOutcome.Updated)!);
        }
        catch (Exception e)
        {
            return APIResult<EquipmentPutDTO>.Exception(e.Message);
        }
    }

    public async Task<APIResult<EquipmentUpdateDTO>> PatchAsync(Guid id, EquipmentUpdateDTO payload, CancellationToken ct)
    {
        try
        {
            var equipment = id.Equals(Guid.Empty)? null: 
                await _db.Equipments
                    .Include(equipmentRecord => equipmentRecord.Descriptor)
                    .FirstOrDefaultAsync(d => d.GUID == id  && !d.IsDeleted, ct);
           
            if(equipment == null) 
                return APIResult<EquipmentUpdateDTO>.NotUpdated("Not found for update");


            if (!equipment.TryUpdate(payload, _clock))
                return APIResult<EquipmentUpdateDTO>.NothingChanged("Nothing changed");
            
            
            await  _db.SaveChangesAsync(ct);
            return APIResult<EquipmentUpdateDTO>.Updated(payload);

        }
        catch (Exception e)
        {
            return APIResult<EquipmentUpdateDTO>.Exception(e.Message);
        }
    }

    public async Task<APIResult<EquipmentGetDTO>> DeleteAsync(Guid id, CancellationToken ct)
    {
        if (id == Guid.Empty) return APIResult<EquipmentGetDTO>.BadRequest($"Invalid id for delete");
        try
        {
            var existing = 
                await _db.Equipments
                    .Where(e=> !e.IsDeleted)
                    .Include(equipmentRecord => equipmentRecord.Descriptor)
                    .FirstOrDefaultAsync(d => d.GUID == id, ct);
        
            if (existing is null) return APIResult<EquipmentGetDTO>.NotFound($"Equipment {id} not found for deletion");

            //existing.IsDeleted = true;
            var dto = existing.ToGetDTO();

            _db.Equipments.Remove(existing);
        
            await _db.SaveChangesAsync(ct).ConfigureAwait(false);
        
            return APIResult<EquipmentGetDTO>.Deleted(dto!);
        }
        catch (Exception e)
        {
            return APIResult<EquipmentGetDTO>.Exception(e.Message);
        }
        
    }
    
    

    
    
    public async Task<APIResult<EquipmentRecord>> CreateAsync(
        EquipmentRecord entity,
        CancellationToken ct)
    {
        try
        {
            //make sure we added the descriptor 
            if (entity.Descriptor != null)
            {
                var descriptorRecord = await _descriptorRepository.CreateAsync(entity.Descriptor, ct);
                entity.Descriptor = descriptorRecord.Value;
                if(descriptorRecord.Value != null) entity.DescriptorID = descriptorRecord.Value.Id;
            }
            
            
            var entry = await _db.Equipments.AddAsync(entity, ct).ConfigureAwait(false);
            if (entry is { State: EntityState.Added, Entity: not null })
            {
                await _db.SaveChangesAsync(ct).ConfigureAwait(false);
                return APIResult<EquipmentRecord>.Created(entry.Entity);
            }

            return APIResult<EquipmentRecord>.Problem("Not inserted");
        }
        catch (Exception e)
        {
            return APIResult<EquipmentRecord>.Exception(e.Message);
        }
    }
}
