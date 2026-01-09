
using GainsLab.Application.Results;
using GainsLab.Contracts.Dtos.Delete.Outcome;
using GainsLab.Contracts.Dtos.GetDto;
using GainsLab.Contracts.Dtos.ID;
using GainsLab.Contracts.Dtos.PostDto.Outcome;
using GainsLab.Contracts.Dtos.PostDto.Request;
using GainsLab.Contracts.Dtos.UpdateDto.Outcome;
using GainsLab.Contracts.Dtos.UpdateDto.Request;

namespace GainsLab.Application.Interfaces.DataManagement.Gateway;

public interface IEquipmentGateway
{
    Task<Result<IReadOnlyList<EquipmentGetDTO>>> GetAllEquipmentsAsync();
    
    Task<Result<EquipmentGetDTO>> GetEquipmentAsync(EquipmentEntityId id);

    Task<Result<EquipmentUpdateCombinedOutcome>> UpdateEquipmentAsync(EquipmentUpdateRequest request,
        DescriptorUpdateRequest? descriptorUpdateRequest);

    Task<Result<EquipmentDeleteOutcome>> DeleteEquipmentAsync(EquipmentEntityId id);
    Task<Result<EquipmentCreateCombineOutcome>> CreateEquipmentAsync(EquipmentCombineCreateRequest request);
}