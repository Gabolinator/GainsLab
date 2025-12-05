using GainsLab.Contracts.SyncDto;
using GainsLab.Core.Models.Core.Results;
using GainsLab.Infrastructure.DB.DTOs;

namespace GainsLab.Contracts.Interface;

public interface IEntitySyncClient
{
    public Task<Result<IReadOnlyList<EquipmentSyncDTO>>> GetAllEquipmentsSyncDtoAsync();
    public Task<Result<IReadOnlyList<DescriptorSyncDTO>>> GetAllDescriptorSyncDtoAsync();
    public Task<Result<IReadOnlyList<MuscleSyncDTO>>> GetAllMusclesSyncDtoAsync();
    public Task<Result<IReadOnlyList<MovementSyncDTO>>> GetAllMovementSyncDtoAsync();
    
    public Task<Result<IReadOnlyList<EquipmentDTO>>> GetAllEquipmentsDtoAsync();
    public Task<Result<IReadOnlyList<DescriptorDTO>>> GetAllDescriptorDtoAsync();
    public Task<Result<IReadOnlyList<MuscleDTO>>> GetAllMusclesDtoAsync();
    public Task<Result<IReadOnlyList<MovementDTO>>> GetAllMovementDtoAsync();

}