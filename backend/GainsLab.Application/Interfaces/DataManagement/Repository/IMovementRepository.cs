using GainsLab.Application.Results.APIResults;
using GainsLab.Contracts.Dtos.GetDto;
using GainsLab.Contracts.Dtos.PostDto;
using GainsLab.Contracts.Dtos.PutDto;
using GainsLab.Contracts.Dtos.UpdateDto;
using GainsLab.Contracts.Dtos.UpdateDto.Outcome;

namespace GainsLab.Application.Interfaces.DataManagement.Repository;

public interface IMovementRepository 
{
    Task<APIResult<MovementGetDTO>> PullByIdAsync(Guid id, CancellationToken ct);
    Task<APIResult<MovementGetDTO>> PostAsync(MovementPostDTO payload, CancellationToken ct);
    Task<APIResult<MovementPutDTO>> PutAsync(Guid id, MovementPutDTO payload, CancellationToken ct);
    Task<APIResult<MovementUpdateOutcome>> PatchAsync(Guid id, MovementUpdateDTO payload, CancellationToken ct);
    Task<APIResult<MovementGetDTO>> DeleteAsync(Guid id, CancellationToken ct);
}