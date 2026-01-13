using GainsLab.Application.Results.APIResults;
using GainsLab.Contracts.Dtos.GetDto;
using GainsLab.Contracts.Dtos.PostDto;
using GainsLab.Contracts.Dtos.PutDto;
using GainsLab.Contracts.Dtos.UpdateDto;
using GainsLab.Contracts.Dtos.UpdateDto.Outcome;

namespace GainsLab.Application.Interfaces.DataManagement.Repository;

public interface IMovementCategoryRepository
{
    Task<APIResult<MovementCategoryGetDTO>> PullByIdAsync(Guid id, CancellationToken ct);
    Task<APIResult<MovementCategoryGetDTO>> PostAsync(MovementCategoryPostDTO payload, CancellationToken ct);
    Task<APIResult<MovementCategoryPutDTO>> PutAsync(Guid id, MovementCategoryPutDTO payload, CancellationToken ct);
    Task<APIResult<MovementCategoryUpdateOutcome>> PatchAsync(Guid id, MovementCategoryUpdateDTO payload, CancellationToken ct);
    Task<APIResult<MovementCategoryGetDTO>> DeleteAsync(Guid id, CancellationToken ct);
}