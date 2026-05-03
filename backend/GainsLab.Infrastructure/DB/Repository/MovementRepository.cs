using GainsLab.Application.Interfaces.DataManagement.Repository;
using GainsLab.Application.Results.APIResults;
using GainsLab.Contracts.Dtos.GetDto;
using GainsLab.Contracts.Dtos.PostDto;
using GainsLab.Contracts.Dtos.PutDto;
using GainsLab.Contracts.Dtos.UpdateDto;
using GainsLab.Contracts.Dtos.UpdateDto.Outcome;
using GainsLab.Domain.Entities.Identifier;


namespace GainsLab.Infrastructure.DB.Repository;


public class MovementRepository : IMovementRepository
{
    public Task<APIResult<MovementGetDTO>> PullByIdAsync(MovementId id, CancellationToken ct)
    {
        throw new NotImplementedException();
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
}