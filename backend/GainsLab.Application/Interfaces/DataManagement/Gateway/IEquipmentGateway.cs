using GainsLab.Application.DTOs.Equipment;
using GainsLab.Application.Results;
using GainsLab.Contracts.Dtos.GetDto;

namespace GainsLab.Application.Interfaces.DataManagement.Gateway;

public interface IEquipmentGateway
{
    Task<Result<IReadOnlyList<EquipmentGetDTO>>> GetAllEquipmentsAsync();
    
    Task<Result<EquipmentGetDTO>> GetEquipmentByIdAsync(Guid id);
}