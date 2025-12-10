using GainsLab.Application.DTOs;
using GainsLab.Application.Results;
using GainsLab.Contracts.SyncDto;

namespace GainsLab.Application.Interfaces.Sync;

public interface IEntitySyncClient
{
    public Task<Result<IReadOnlyList<EquipmentSyncDTO>>> GetAllEquipmentsSyncDtoAsync();
    public Task<Result<IReadOnlyList<DescriptorSyncDTO>>> GetAllDescriptorSyncDtoAsync();
    public Task<Result<IReadOnlyList<MuscleSyncDTO>>> GetAllMusclesSyncDtoAsync();
    public Task<Result<IReadOnlyList<MovementSyncDTO>>> GetAllMovementSyncDtoAsync();
    
    public Task<Result<IReadOnlyList<EquipmentRecord>>> GetAllEquipmentsRecordAsync();
    public Task<Result<IReadOnlyList<DescriptorRecord>>> GetAllDescriptorRecordAsync();
    public Task<Result<IReadOnlyList<MuscleRecord>>> GetAllMusclesRecordAsync();
    public Task<Result<IReadOnlyList<MovementRecord>>> GetAllMovementRecordAsync();

}