using GainsLab.Application.Results;
using GainsLab.Contracts.Dtos.Delete.Outcome;
using GainsLab.Contracts.Dtos.GetDto;
using GainsLab.Contracts.Dtos.ID;
using GainsLab.Contracts.Dtos.PostDto.Outcome;
using GainsLab.Contracts.Dtos.PostDto.Request;
using GainsLab.Contracts.Dtos.UpdateDto.Outcome;
using GainsLab.Contracts.Dtos.UpdateDto.Request;

namespace GainsLab.Application.Interfaces.DataManagement.Gateway;

public interface IMovementCategoryGateway
{
    Task<Result<IReadOnlyList<MovementCategoryGetDTO>>> GetAllCategoryAsync();
    
    Task<Result<MovementCategoryGetDTO>> GetMovementCategoryAsync(MovementCategoryEntityId id);

    Task<Result<MovementCategoryUpdateCombinedOutcome>> UpdateMovementCategoryAsync(MovementCategoryUpdateRequest request,
        DescriptorUpdateRequest? descriptorUpdateRequest);

    Task<Result<MovementCategoryDeleteOutcome>> DeleteMovementCategoryAsync(MovementCategoryEntityId id);
    Task<Result<MovementCategoryCreateCombineOutcome>> CreateMovementCategoryAsync(MovementCategoryCombineCreateRequest request);

}