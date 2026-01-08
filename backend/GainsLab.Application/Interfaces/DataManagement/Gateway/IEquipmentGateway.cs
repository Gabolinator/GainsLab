using GainsLab.Application.DTOs.Equipment;
using GainsLab.Application.Results;
using GainsLab.Contracts;
using GainsLab.Contracts.Dtos.GetDto;
using GainsLab.Contracts.Dtos.UpdateDto;
using GainsLab.Contracts.Dtos.UpdateDto.Outcome;

namespace GainsLab.Application.Interfaces.DataManagement.Gateway;

public interface IEquipmentGateway
{
    Task<Result<IReadOnlyList<EquipmentGetDTO>>> GetAllEquipmentsAsync();
    
    Task<Result<EquipmentGetDTO>> GetEquipmentByIdAsync(Guid id);

    Task<Result<EquipmentCombinedOutcome>> UpdateEquipmentAsync(EquipmentUpdateRequest request,
        DescriptorUpdateRequest? descriptorUpdateRequest);
}