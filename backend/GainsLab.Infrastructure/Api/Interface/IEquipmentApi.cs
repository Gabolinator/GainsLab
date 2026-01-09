using GainsLab.Application.Results;
using GainsLab.Contracts.Dtos.Delete.Outcome;
using GainsLab.Contracts.Dtos.GetDto;
using GainsLab.Contracts.Dtos.ID;
using GainsLab.Contracts.Dtos.PostDto;
using GainsLab.Contracts.Dtos.PostDto.Outcome;
using GainsLab.Contracts.Dtos.UpdateDto;
using GainsLab.Contracts.Dtos.UpdateDto.Outcome;
using GainsLab.Contracts.Dtos.UpdateDto.Request;
using GainsLab.Contracts.Interface;

namespace GainsLab.Infrastructure.Api.Interface;

public interface IEquipmentApi
{
    Task<Result<ISyncPage<ISyncDto>>> PullEquipmentPageAsync(ISyncCursor cursor, int take, CancellationToken ct);
    
    Task<Result<EquipmentGetDTO>> GetEquipmentAsync(EquipmentEntityId entity, CancellationToken ct);
    
    Task<Result<EquipmentCreateOutcome>>  CreateEquipmentAsync(EquipmentPostDTO entity, CancellationToken ct);
    
    Task<Result<EquipmentUpdateOutcome>> UpdateEquipmentAsync(EquipmentUpdateRequest request, CancellationToken ct);
    
    Task<Result<EquipmentDeleteOutcome>> DeleteEquipmentAsync(EquipmentEntityId entity, CancellationToken ct);

}