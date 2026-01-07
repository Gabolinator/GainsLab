using GainsLab.Application.Results;
using GainsLab.Contracts.Dtos.GetDto;
using GainsLab.Contracts.Dtos.PostDto;
using GainsLab.Contracts.Dtos.RequestDto;
using GainsLab.Contracts.Interface;

namespace GainsLab.Infrastructure.Api.Interface;

public interface IEquipmentApi
{
    Task<Result<ISyncPage<ISyncDto>>> PullEquipmentPageAsync(ISyncCursor cursor, int take, CancellationToken ct);
    
    Task<Result<EquipmentGetDTO>> GetEquipmentAsync(EquipmentRequestDTO entity, CancellationToken ct);
    
    Task<Result<EquipmentPostDTO>>  CreateEquipmentAsync(EquipmentPostDTO entity, CancellationToken ct);
    
    Task<Result<EquipmentPostDTO>> UpdateEquipmentAsync(EquipmentPostDTO entity, CancellationToken ct);
    
    Task<Result<EquipmentGetDTO>> DeleteEquipmentAsync(EquipmentGetDTO entity, CancellationToken ct);

}