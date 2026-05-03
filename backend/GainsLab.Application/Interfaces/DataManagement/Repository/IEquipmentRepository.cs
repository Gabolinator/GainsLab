using GainsLab.Application.Results.APIResults;
using GainsLab.Contracts.Dtos.GetDto;
using GainsLab.Contracts.Dtos.PostDto;
using GainsLab.Contracts.Dtos.PutDto;
using GainsLab.Contracts.Dtos.UpdateDto;
using GainsLab.Contracts.Dtos.UpdateDto.Outcome;
using GainsLab.Domain.Entities.Identifier;

namespace GainsLab.Application.Interfaces.DataManagement.Repository;

public interface IEquipmentRepository
{
    
    Task<APIResult<EquipmentGetDTO>> PullByIdAsync(EquipmentId id, CancellationToken ct);
    Task<APIResult<EquipmentGetDTO>> PostAsync(EquipmentPostDTO payload, CancellationToken ct);
    Task<APIResult<EquipmentPutDTO>> PutAsync(EquipmentId id, EquipmentPutDTO payload, CancellationToken ct);
    Task<APIResult<EquipmentUpdateOutcome>> PatchAsync(EquipmentId id, EquipmentUpdateDTO payload, CancellationToken ct);
    Task<APIResult<EquipmentGetDTO>> DeleteAsync(EquipmentId id, CancellationToken ct);
   

}