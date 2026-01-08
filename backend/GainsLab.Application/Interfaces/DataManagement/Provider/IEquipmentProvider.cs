using GainsLab.Application.Results;
using GainsLab.Contracts.Dtos.GetDto;
using GainsLab.Contracts.Dtos.PostDto;
using GainsLab.Contracts.Dtos.RequestDto;
using GainsLab.Contracts.Dtos.UpdateDto;
using GainsLab.Contracts.Dtos.UpdateDto.Outcome;
using GainsLab.Contracts.Interface;

namespace GainsLab.Application.Interfaces.DataManagement.Provider;

public interface IEquipmentProvider
{
    Task<Result<ISyncPage<ISyncDto>>> PullEquipmentPageAsync(ISyncCursor cursor, int take, CancellationToken ct);
    
    Task<Result<EquipmentGetDTO>> GetEquipmentAsync(EquipmentRequestDTO entity, CancellationToken ct);
    
    Task<Result<EquipmentPostDTO>>  CreateEquipmentAsync(EquipmentPostDTO entity, CancellationToken ct);
    
    Task<Result<EquipmentUpdateOutcome>> UpdateEquipmentAsync(EquipmentUpdateRequest request, CancellationToken ct);
    
    Task<Result<EquipmentGetDTO>> DeleteEquipmentAsync(EquipmentRequestDTO entity, CancellationToken ct);

}