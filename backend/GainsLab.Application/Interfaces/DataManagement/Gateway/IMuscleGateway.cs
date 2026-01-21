using GainsLab.Application.DTOs.Muscle;
using GainsLab.Application.Results;
using GainsLab.Contracts.Dtos.Delete.Outcome;
using GainsLab.Contracts.Dtos.GetDto;
using GainsLab.Contracts.Dtos.ID;
using GainsLab.Contracts.Dtos.PostDto.Outcome;
using GainsLab.Contracts.Dtos.PostDto.Request;
using GainsLab.Contracts.Dtos.UpdateDto.Outcome;
using GainsLab.Contracts.Dtos.UpdateDto.Request;

namespace GainsLab.Application.Interfaces.DataManagement.Gateway;

public interface IMuscleGateway
{
    public Task<Result<IReadOnlyList<MuscleGetDTO>>> GetAllMusclesAsync();
    
    Task<Result<MuscleGetDTO>> GetMuscleAsync(MuscleEntityId id);

    Task<Result<MuscleUpdateCombinedOutcome>> UpdateMuscleAsync(MuscleUpdateRequest request,
        DescriptorUpdateRequest? descriptorUpdateRequest,
        ICache? cache);

    Task<Result<MuscleDeleteOutcome>> DeleteMuscleAsync(MuscleEntityId id, ICache? cache);
    Task<Result<MuscleCreateCombineOutcome>> CreateMuscleAsync(MuscleCombineCreateRequest request, ICache? cache);
}