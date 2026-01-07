using GainsLab.Application.DTOs;
using GainsLab.Application.DTOs.Equipment;
using GainsLab.Application.Results.APIResults;
using GainsLab.Contracts.Dtos.GetDto;
using GainsLab.Contracts.Dtos.PostDto;
using GainsLab.Contracts.Dtos.PutDto;
using GainsLab.Contracts.Dtos.UpdateDto;

namespace GainsLab.Application.Interfaces;

public interface IEquipmentRepository
{
    
    Task<APIResult<EquipmentGetDTO>> PullByIdAsync(Guid id, CancellationToken ct);
    Task<APIResult<EquipmentGetDTO>> PostAsync(EquipmentPostDTO payload, CancellationToken ct);
    Task<APIResult<EquipmentPutDTO>> PutAsync(Guid id, EquipmentPutDTO payload, CancellationToken ct);
    Task<APIResult<EquipmentUpdateDTO>> PatchAsync(Guid id, EquipmentUpdateDTO payload, CancellationToken ct);
    Task<APIResult<EquipmentGetDTO>> DeleteAsync(Guid id, CancellationToken ct);
    Task<APIResult<EquipmentRecord>> CreateAsync(EquipmentRecord entity, CancellationToken ct);

}