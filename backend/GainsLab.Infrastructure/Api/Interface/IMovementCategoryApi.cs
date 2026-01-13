using GainsLab.Application.Results;
using GainsLab.Contracts.Dtos.Delete.Outcome;
using GainsLab.Contracts.Dtos.GetDto;
using GainsLab.Contracts.Dtos.ID;
using GainsLab.Contracts.Dtos.PostDto;
using GainsLab.Contracts.Dtos.PostDto.Outcome;
using GainsLab.Contracts.Dtos.UpdateDto.Outcome;
using GainsLab.Contracts.Dtos.UpdateDto.Request;
using GainsLab.Contracts.Interface;

namespace GainsLab.Infrastructure.Api.Interface;

public interface IMovementCategoryApi
{
    Task<Result<ISyncPage<ISyncDto>>> PullMovementCategoryPageAsync(ISyncCursor cursor, int take, CancellationToken ct);
    
    Task<Result<MovementCategoryGetDTO>> GetMovementCategoryAsync(MovementCategoryEntityId entity, CancellationToken ct);
    
    Task<Result<MovementCategoryCreateOutcome>>  CreateMovementCategoryAsync(MovementCategoryPostDTO entity, CancellationToken ct);
    
    Task<Result<MovementCategoryUpdateOutcome>> UpdateMovementCategoryAsync(MovementCategoryUpdateRequest request, CancellationToken ct);
    
    Task<Result<MovementCategoryDeleteOutcome>> DeleteMovementCategoryAsync(MovementCategoryEntityId entity, CancellationToken ct);

}