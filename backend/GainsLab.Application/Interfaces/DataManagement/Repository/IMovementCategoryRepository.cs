using GainsLab.Application.Results.APIResults;
using GainsLab.Contracts.Dtos.GetDto;
using GainsLab.Contracts.Dtos.PostDto;
using GainsLab.Contracts.Dtos.PutDto;
using GainsLab.Contracts.Dtos.UpdateDto;
using GainsLab.Contracts.Dtos.UpdateDto.Outcome;
using GainsLab.Domain.Entities.Identifier;

namespace GainsLab.Application.Interfaces.DataManagement.Repository;

public interface IMovementCategoryRepository
{
    Task<APIResult<MovementCategoryGetDTO>> PullByIdAsync(MovementCategoryId id, CancellationToken ct);
    Task<APIResult<MovementCategoryGetDTO>> PostAsync(MovementCategoryPostDTO payload, CancellationToken ct);
    Task<APIResult<MovementCategoryPutDTO>> PutAsync(MovementCategoryId id, MovementCategoryPutDTO payload, CancellationToken ct);
    Task<APIResult<MovementCategoryUpdateOutcome>> PatchAsync(MovementCategoryId id, MovementCategoryUpdateDTO payload, CancellationToken ct);
    Task<APIResult<MovementCategoryGetDTO>> DeleteAsync(MovementCategoryId id, CancellationToken ct);
}