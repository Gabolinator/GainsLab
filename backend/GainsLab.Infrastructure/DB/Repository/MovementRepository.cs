using GainsLab.Application.Interfaces.DataManagement.Repository;
using GainsLab.Application.Results.APIResults;
using GainsLab.Contracts.Dtos.GetDto;
using GainsLab.Contracts.Dtos.PostDto;
using GainsLab.Contracts.Dtos.PutDto;
using GainsLab.Contracts.Dtos.UpdateDto;
using GainsLab.Contracts.Dtos.UpdateDto.Outcome;


namespace GainsLab.Infrastructure.DB.Repository;


public class MovementRepository : IMovementRepository
{
    public Task<APIResult<MovementGetDTO>> PullByIdAsync(Guid id, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public Task<APIResult<MovementGetDTO>> PostAsync(MovementPostDTO payload, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public Task<APIResult<MovementPutDTO>> PutAsync(Guid id, MovementPutDTO payload, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public Task<APIResult<MovementUpdateOutcome>> PatchAsync(Guid id, MovementUpdateDTO payload, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public Task<APIResult<MovementGetDTO>> DeleteAsync(Guid id, CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}