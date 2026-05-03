using GainsLab.Application.DomainMappers;
using GainsLab.Application.DTOs.Movement;
using GainsLab.Application.Interfaces.DataManagement.Repository;
using GainsLab.Application.Results.APIResults;
using GainsLab.Contracts.Dtos.GetDto;
using GainsLab.Contracts.Dtos.PostDto;
using GainsLab.Contracts.Dtos.PutDto;
using GainsLab.Contracts.Dtos.UpdateDto;
using GainsLab.Contracts.Dtos.UpdateDto.Outcome;
using GainsLab.Domain.Entities.Identifier;
using GainsLab.Domain.Interfaces;
using GainsLab.Infrastructure.DB.Context;
using GainsLab.Infrastructure.Utilities;
using Microsoft.EntityFrameworkCore;


namespace GainsLab.Infrastructure.DB.Repository;


public class MovementRepository(
    GainLabPgDBContext db,
    IEquipmentRepository equipmentRepository,
    IMovementCategoryRepository movementCategoryRepository,
    IMuscleRepository muscleRepository,
    IClock clock) : IMovementRepository
{
    public async Task<APIResult<MovementGetDTO>> PullByIdAsync(MovementId id, CancellationToken ct)
    {
        if(id == Guid.Empty) return APIResult<MovementGetDTO>.BadRequest("Id cannot be empty");
        
        try
        {
            var description = await GetRecordByIdAsync(id, ct);
            return CrudResultUtilities.DispatchResult<MovementGetDTO, MovementRecord>(description, record => record.ToGetDto(clock)!);
        }
        catch (Exception e)
        {
            return APIResult<MovementGetDTO>.Exception(e.Message);
        }
    }

    public Task<APIResult<MovementGetDTO>> PostAsync(MovementPostDTO payload, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public Task<APIResult<MovementPutDTO>> PutAsync(MovementId id, MovementPutDTO payload, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public Task<APIResult<MovementUpdateOutcome>> PatchAsync(MovementId id, MovementUpdateDTO payload, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public Task<APIResult<MovementGetDTO>> DeleteAsync(MovementId id, CancellationToken ct)
    {
        throw new NotImplementedException();
    }
    
    private async Task<APIResult<MovementRecord>> GetRecordByIdAsync(Guid? id, CancellationToken ct)
    {
        if(!id.HasValue || id == Guid.Empty) 
            return APIResult<MovementRecord>.BadRequest("Id cannot be null or empty");
        try
        {
            var movement = await db.Movement
                .AsNoTracking()
                .AsSplitQuery()
                .Include(m => m.Descriptor)
                .Include(m => m.Category)
                .Include(m => m.MuscleRelations)
                .ThenInclude(r => r.Muscle)
                .Include(m => m.EquipmentRelations)
                .ThenInclude(r => r.Equipment)
                .Include(m => m.VariantOfMovement)
                .Where(m => !m.IsDeleted)
                .FirstOrDefaultAsync(m => m.GUID == id.Value, ct);
         
            return movement != null ? 
                APIResult<MovementRecord>.Found(movement!):
                APIResult<MovementRecord>.NotFound(id.Value.ToString());

        }
        catch (Exception e)
        {
            return APIResult<MovementRecord>.Exception(e.Message);
        }
    }
}