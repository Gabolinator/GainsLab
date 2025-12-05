using GainsLab.Contracts.Interface;
using GainsLab.Contracts.SyncDto;
using GainsLab.Contracts.SyncService.Mapper;
using GainsLab.Core.Models.Core;
using GainsLab.Core.Models.Core.Interfaces.DataManagement;
using GainsLab.Core.Models.Core.Results;
using GainsLab.Infrastructure.DB.DTOs;
using ILogger = GainsLab.Core.Models.Core.Utilities.Logging.ILogger;

namespace GainsLab.Contracts.SyncService;

public class EntitySyncClient :IEntitySyncClient
{
    public readonly IRemoteProvider RemoteProvider;

    public EntitySyncClient (IRemoteProvider remoteProvider, ILogger logger)
    {
        RemoteProvider = remoteProvider;
        _logger = logger;
    }
    
    private Dictionary<Guid, DescriptorDTO>  _descriptorCache = new Dictionary<Guid, DescriptorDTO>();
    private readonly ILogger _logger;

    public async Task<Result<IReadOnlyList<EquipmentSyncDTO>>> GetAllEquipmentsSyncDtoAsync()
    {
        
        var result =  await RemoteProvider.PullAsync(EntityType.Equipment, SyncCursorUtil.MinValue);
        if (!result.Success)
        {
            return Result<IReadOnlyList<EquipmentSyncDTO>>.Failure(result.GetErrorMessage());
        }
        
        return Result<IReadOnlyList<EquipmentSyncDTO>>.SuccessResult(result.Value != null ? 
            result.Value.ItemsList.Cast<EquipmentSyncDTO>().ToList():
            new());
    }

    private async Task UpdateDescriptorCache()
    {
       if(await IsDescriptorCacheUpToDate()) return;


       var desc = await GetAllDescriptorDtoAsync();
       
       //todo add log
       if (!desc.Success)
       {
           _logger.LogWarning(nameof(EntitySyncClient) + "." + nameof(UpdateDescriptorCache), $"Failed to retrieve descriptor dtos - {desc.GetErrorMessage()}");
           return;
       }
       
       if (!desc.HasValue)
       {
           _logger.LogWarning(nameof(EntitySyncClient) + "." + nameof(UpdateDescriptorCache), "Failed to retrieve descriptor dtos - no values");
           return;
       }
       
       _descriptorCache.Clear();
       _descriptorCache = desc.Value.ToDictionary(x => x.GUID, x=>x);
       
    }

    private async Task<bool> IsDescriptorCacheUpToDate()
    {
        //todo
       //return true for now
       return DescriptorCached;
    }

    public bool DescriptorCached => _descriptorCache.Count > 0;

    public async Task<Result<IReadOnlyList<DescriptorSyncDTO>>> GetAllDescriptorSyncDtoAsync()
    {
        var result =  await RemoteProvider.PullAsync(EntityType.Descriptor, SyncCursorUtil.MinValue);
        if (!result.Success)
        {
            return Result<IReadOnlyList<DescriptorSyncDTO>>.Failure(result.GetErrorMessage());
        }
        
        return Result<IReadOnlyList<DescriptorSyncDTO>>.SuccessResult(result.Value != null ? 
            result.Value.ItemsList.Cast<DescriptorSyncDTO>().ToList():
            new());
    }

    public Task<Result<IReadOnlyList<MuscleSyncDTO>>> GetAllMusclesSyncDtoAsync()
    {
        throw new NotImplementedException();
    }

    public Task<Result<IReadOnlyList<MovementSyncDTO>>> GetAllMovementSyncDtoAsync()
    {
        throw new NotImplementedException();
    }

    public async Task<Result<IReadOnlyList<EquipmentDTO>>> GetAllEquipmentsDtoAsync()
    {
      
        //make sure descriptor are up to date
        await UpdateDescriptorCache();

        var syncDtos = await GetAllEquipmentsSyncDtoAsync();
        if(!syncDtos.Success) return Result<IReadOnlyList<EquipmentDTO>>.Failure(syncDtos.GetErrorMessage());
        return Result<IReadOnlyList<EquipmentDTO>>.SuccessResult(syncDtos.Value != null
            ? syncDtos.Value.Select(s => EquipmentSyncMapper.FromSyncDTO(s, TryGetDescriptor(s.DescriptorGUID, out DescriptorDTO descriptorDto) ? descriptorDto : null ,"sync")).ToList(): new());

        
    }

    private bool TryGetDescriptor(Guid? descriptorGuid, out DescriptorDTO? descriptorDto)
    {
       descriptorDto = null; 
       if(descriptorGuid == null || _descriptorCache.Count ==0 ) return false;
       
       return  _descriptorCache.TryGetValue(descriptorGuid.Value, out descriptorDto);
       
    }

    public async Task<Result<IReadOnlyList<DescriptorDTO>>> GetAllDescriptorDtoAsync()
    {
        var syncDtos = await GetAllDescriptorSyncDtoAsync();
        if(!syncDtos.Success) return Result<IReadOnlyList<DescriptorDTO>>.Failure(syncDtos.GetErrorMessage());
        return Result<IReadOnlyList<DescriptorDTO>>.SuccessResult(syncDtos.Value != null
            ? syncDtos.Value.Select(s => DescriptorSyncMapper.FromSyncDTO(s, "sync")).ToList(): new());
    }

    public Task<Result<IReadOnlyList<MuscleDTO>>> GetAllMusclesDtoAsync()
    {
        throw new NotImplementedException();
    }

    public Task<Result<IReadOnlyList<MovementDTO>>> GetAllMovementDtoAsync()
    {
        throw new NotImplementedException();
    }
}