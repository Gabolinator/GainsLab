using GainsLab.Application.DomainMappers;
using GainsLab.Application.DTOs;
using GainsLab.Application.DTOs.Description;
using GainsLab.Application.DTOs.Equipment;
using GainsLab.Application.Interfaces;
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

public class EquipmentRepository(
    GainLabPgDBContext db,
    IDescriptorRepository descriptorRepository,
    IClock clock,
    ILogger log)
    : IEquipmentRepository
{
    public async Task<APIResult<EquipmentGetDTO>> PullByIdAsync(Guid id, CancellationToken ct)
    {
        log.Log(nameof(PullByIdAsync), $"Trying to pull equipment by id {id}" );
        
        try
        {
            var equipment = await GetRecordByIdAsync(id, ct);
            
            return CrudResultUtilities.DispatchResult<EquipmentGetDTO, EquipmentRecord>(equipment, record => record.ToGetDTO()!);
        }
        
        catch (Exception e)
        {
            log.LogError(nameof(PullByIdAsync), $"Exeption Trying to pull equipment by id {id} - {e.GetBaseException()}" );
            return APIResult<EquipmentGetDTO>.Exception(e.Message);
        }
    }

    public async Task<APIResult<EquipmentGetDTO>> PostAsync(EquipmentPostDTO payload, CancellationToken ct)
    {
        try
        {
            var entity = payload.ToEntity(clock);           // GUID created here
            if (entity == null) return APIResult<EquipmentGetDTO>.BadRequest("Could not create record from dto");


            var result = await TryValidateUnique(entity, ct);
            if (result.AlreadyExists)
            {
                return result.ExistingResult!;
            }

            //descriptor is created inside create async
            var record = await CreateAsync(entity, ct);
            
            //if success => value != null
            return record.Success  ? 
                APIResult<EquipmentGetDTO>.Created(record.Value.ToGetDTO()!) 
                : APIResult<EquipmentGetDTO>.NotCreated("Failed to create record", NotCreatedReason.Other);
            
        }
        catch (Exception e)
        {
            return APIResult<EquipmentGetDTO>.Exception(e.Message);
        }
    }

    private Task<UniqueValidationResult<EquipmentGetDTO>> TryValidateUnique(
        EquipmentRecord entity,
        CancellationToken ct)
    {
        return CrudResultUtilities.TryValidateUniqueAsync<EquipmentRecord, EquipmentGetDTO>(
            entity,
            EntityType.Descriptor,
            getId: x => x.GUID,
            getName: x => x.Name,
            getContent: x => null,
            getOther: x => null,
            getExistingRecordAsync: (x, token) => GetExistingRecordAsync(x.GUID, x.Name, token),
            ct);
    }

    private Task<MatchingResult<EquipmentRecord>> GetExistingRecordAsync(
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
            getByName: GetRecordByNameAsync,
            getByContent: null,
            getByOther: null,
            ct: ct);
    }

    private async Task<APIResult<EquipmentRecord>> GetRecordByIdAsync(Guid? id, CancellationToken ct)
    {
        if(!id.HasValue || id == Guid.Empty) 
            return APIResult<EquipmentRecord>.BadRequest("Id cannot be null or empty");
        
        try
        {
            var equipment = await db.Equipments
                .AsNoTracking()
                .Include(e=>e.Descriptor)
                .Where(e => !e.IsDeleted)
                .FirstOrDefaultAsync(e=> e.GUID == id.Value, ct);
            
            
            return equipment != null ? 
                APIResult<EquipmentRecord>.Found(equipment!):
                APIResult<EquipmentRecord>.NotFound(id.Value.ToString());

        }
        catch (Exception e)
        {
            return APIResult<EquipmentRecord>.Exception(e.Message);
        }
    }
    
    private async Task<APIResult<EquipmentRecord>> GetRecordByNameAsync(string name, CancellationToken ct)
    {
        var formatedContent = name.Trim();
        if(string.IsNullOrWhiteSpace(formatedContent)) 
            return APIResult<EquipmentRecord>.BadRequest("Name cannot be empty");
        
        try
        {
            var equipment = await db.Equipments
                .AsNoTracking()
                .Include(e=>e.Descriptor)
                .Where(e => !e.IsDeleted)
                .FirstOrDefaultAsync(e=> e.Name.ToLower().Trim() == name, ct);
            
            return equipment != null ? 
                APIResult<EquipmentRecord>.Found(equipment!):
                APIResult<EquipmentRecord>.NotFound(name);

        }
        catch (Exception e)
        {
            return APIResult<EquipmentRecord>.Exception(e.Message);
        }
    }


    public async Task<APIResult<EquipmentPutDTO>> PutAsync(Guid id, EquipmentPutDTO payload, CancellationToken ct)
    {
        try
        {
            if(id == Guid.Empty) return APIResult<EquipmentPutDTO>.BadRequest("Id cannot be empty");

            var existing = 
                await db.Equipments
                    .Where(e=> !e.IsDeleted)
                    .Include(equipmentRecord => equipmentRecord.Descriptor)
                    .FirstOrDefaultAsync(d => d.GUID == id, ct);

            if (existing is null)
            {
                // create via shared method
                var entity = payload.ToEntity(clock, id)!; // guid may be null -> create a new one inside mapping OR enforce not null
                var created = await CreateAsync(entity, ct);

                
                return !created.Success ? 
                    APIResult<EquipmentPutDTO>.NotCreated(created.GetErrorMessage()?? "Create failed", NotCreatedReason.Other) : 
                    APIResult<EquipmentPutDTO>.Created(created.Value!.ToPutDTO(clock, UpsertOutcome.Created)!);
            }

            payload.Id = id;
            
            //nothing changed
            if(!existing.AnythingChanged(payload)) 
                return APIResult<EquipmentPutDTO>.NothingChanged($"For entity : {payload.Id}");

            // update branch
            existing.Name = payload.Name;
            existing.UpdatedAtUtc = clock.UtcNow;
            existing.UpdatedBy = payload.UpdatedBy;
            existing.Authority = payload.Authority;

            // ensure descriptor identifier is set
            if (payload.Descriptor.Id == null || payload.Descriptor.Id == Guid.Empty)
            {
                payload.Descriptor.Id = existing.Descriptor?.GUID ?? Guid.NewGuid();
            }
            
            var descriptorResult = await descriptorRepository.PutAsync(payload.Descriptor.Id.Value, payload.Descriptor, ct);
            if (!descriptorResult.Success || descriptorResult.Value?.Id == null)
            {
                var error = descriptorResult.GetErrorMessage() ?? "Descriptor update failed";
                return APIResult<EquipmentPutDTO>.Exception(error);
            }

            // refresh descriptor reference on the equipment
            var descriptorRecord = await db.Descriptors
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.GUID == payload.Descriptor.Id!.Value, ct);

            if (descriptorRecord == null)
            {
                return APIResult<EquipmentPutDTO>.Problem($"Descriptor {payload.Descriptor.Id} not found after update");
            }

            existing.Descriptor = descriptorRecord;
            existing.DescriptorID = descriptorRecord.Id;
            
            await db.SaveChangesAsync(ct).ConfigureAwait(false);
            return APIResult<EquipmentPutDTO>.Updated(existing.ToPutDTO(clock, UpsertOutcome.Updated)!);
        }
        catch (Exception e)
        {
            return APIResult<EquipmentPutDTO>.Exception(e.Message);
        }
    }

    public async Task<APIResult<EquipmentUpdateOutcome>> PatchAsync(Guid id, EquipmentUpdateDTO payload, CancellationToken ct)
    {
        try
        {
            var equipment = id.Equals(Guid.Empty)? null: 
                await db.Equipments
                    .Include(equipmentRecord => equipmentRecord.Descriptor)
                    .FirstOrDefaultAsync(d => d.GUID == id  && !d.IsDeleted, ct);
           
            if(equipment == null) 
                return APIResult<EquipmentUpdateOutcome>.NotUpdated("Not found for update");


            if (!equipment.TryUpdate(payload, clock))
                return APIResult<EquipmentUpdateOutcome>.NothingChanged("Nothing changed");
            
            
            await  db.SaveChangesAsync(ct);
            return APIResult<EquipmentUpdateOutcome>.Updated(new EquipmentUpdateOutcome(UpdateOutcome.Updated,UpdateOutcome.NotUpdated,null,equipment.ToGetDTO()));

        }
        catch (Exception e)
        {
            return APIResult<EquipmentUpdateOutcome>.Exception(e.Message);
        }
    }

    public async Task<APIResult<EquipmentGetDTO>> DeleteAsync(Guid id, CancellationToken ct)
    {
        if (id == Guid.Empty) return APIResult<EquipmentGetDTO>.BadRequest($"Invalid id for delete");
        try
        {
            var existing = 
                await db.Equipments
                    .Where(e=> !e.IsDeleted)
                    .Include(equipmentRecord => equipmentRecord.Descriptor)
                    .FirstOrDefaultAsync(d => d.GUID == id, ct);
        
            
            log.Log(nameof(EquipmentRepository),$"Trying to delete equipment - {id}");
            
            if (existing is null) return APIResult<EquipmentGetDTO>.NotFound($"Equipment {id} not found for deletion");
            log.Log(nameof(EquipmentRepository),$"Equipment - {id} found");

            
            existing.IsDeleted = true;
            existing.UpdatedAtUtc = clock.UtcNow;

            var dto = existing.ToGetDTO();
        
            db.Remove(existing);
            
            await db.SaveChangesAsync(ct).ConfigureAwait(false);
        
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
                var descriptorResult = await descriptorRepository.GetOrCreateAsync(entity.Descriptor, ct);
                if (!descriptorResult.Success || descriptorResult.Value is null)
                {
                    return APIResult<EquipmentRecord>.Problem("Could not resolve descriptor.");
                }

                entity.DescriptorID = descriptorResult.Value.Id;
                entity.Descriptor = null;
            }
            
            var entry = await db.Equipments.AddAsync(entity, ct).ConfigureAwait(false);
            if (entry is { State: EntityState.Added, Entity: not null })
            {
                await db.SaveChangesAsync(ct).ConfigureAwait(false);
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
