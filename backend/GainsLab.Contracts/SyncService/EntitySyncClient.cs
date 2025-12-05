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
    private Dictionary<Guid, MuscleDTO>  _musclesCache = new Dictionary<Guid, MuscleDTO>();
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

    public async Task<Result<IReadOnlyList<MuscleSyncDTO>>> GetAllMusclesSyncDtoAsync()
    {
        var result =  await RemoteProvider.PullAsync(EntityType.Muscle, SyncCursorUtil.MinValue);
        if (!result.Success)
        {
            return Result<IReadOnlyList<MuscleSyncDTO>>.Failure(result.GetErrorMessage());
        }
        
        return Result<IReadOnlyList<MuscleSyncDTO>>.SuccessResult(result.Value != null ? 
            result.Value.ItemsList.Cast<MuscleSyncDTO>().ToList():
            new());
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
            ? syncDtos.Value.Select(s => EquipmentSyncMapper.FromSyncDTO(s, TryGetDescriptor(s.DescriptorGUID, out var descriptorDto) ? descriptorDto : null ,"sync")).ToList(): new());

        
    }

 

    public async Task<Result<IReadOnlyList<DescriptorDTO>>> GetAllDescriptorDtoAsync()
    {
        var syncDtos = await GetAllDescriptorSyncDtoAsync();
        if(!syncDtos.Success) return Result<IReadOnlyList<DescriptorDTO>>.Failure(syncDtos.GetErrorMessage());
        return Result<IReadOnlyList<DescriptorDTO>>.SuccessResult(syncDtos.Value != null
            ? syncDtos.Value.Select(s => DescriptorSyncMapper.FromSyncDTO(s, "sync")).ToList(): new());
    }

    public async Task<Result<IReadOnlyList<MuscleDTO>>> GetAllMusclesDtoAsync()
    {
        await UpdateDescriptorCache();
        
        var syncDtos = await GetAllMusclesSyncDtoAsync();
        if(!syncDtos.Success) return Result<IReadOnlyList<MuscleDTO>>.Failure(syncDtos.GetErrorMessage());

        var muscles = new List<MuscleDTO>();
        var antagonistLookup = new Dictionary<Guid, IReadOnlyList<Guid>>();

        if (syncDtos.Value != null)
        {
            foreach (var syncDto in syncDtos.Value)
            {
                var descriptor = TryGetDescriptor(syncDto.DescriptorGUID, out var descriptorDto) ? descriptorDto : null;
                var dto = MuscleSyncMapper.FromSyncDTO(syncDto, descriptor ,"sync");
                muscles.Add(dto);
                antagonistLookup[dto.GUID] = NormalizeAntagonistGuids(syncDto);
            }
        }
       
        await ResolveAntagonists(muscles, antagonistLookup);         
                
        return Result<IReadOnlyList<MuscleDTO>>.SuccessResult(muscles);
    }

    private async Task ResolveAntagonists(
        List<MuscleDTO> dtos,
        IReadOnlyDictionary<Guid, IReadOnlyList<Guid>> antagonistLookup)
    {
        if (dtos.Count == 0) return;
        
        await UpdateMuscleMap(dtos);
        if(_musclesCache.Count == 0) return;
        
        foreach (var dto in dtos)
        {
            antagonistLookup.TryGetValue(dto.GUID, out var normalized);
            dto.Antagonists = TryGetAntagonist(dto, normalized);
        }
    }

    private ICollection<MuscleAntagonistDTO> TryGetAntagonist(
        MuscleDTO source,
        IReadOnlyList<Guid>? antagonistGuids)
    {
        if (antagonistGuids == null || antagonistGuids.Count == 0 || _musclesCache.Count == 0)
            return new List<MuscleAntagonistDTO>();

        var links = new List<MuscleAntagonistDTO>();
        
        foreach (var guid in antagonistGuids)
        {
            if (!_musclesCache.TryGetValue(guid, out var antagonist))
            {
                _logger.LogWarning(nameof(EntitySyncClient),
                    $"Unable to resolve antagonist {guid} for muscle {source.GUID}");
                continue;
            }

            var link = new MuscleAntagonistDTO
            {
                Muscle = source,
                MuscleId = source.Id,
                Antagonist = antagonist,
                AntagonistId = antagonist.Id
            };
            
            links.Add(link);

            if (antagonist.Agonists.All(a => a.Muscle?.GUID != source.GUID))
            {
                antagonist.Agonists.Add(link);
            }
        }

        return links;
    }

    private static IReadOnlyList<Guid> NormalizeAntagonistGuids(MuscleSyncDTO dto)
    {
        if (dto.IsDeleted)
        {
            return Array.Empty<Guid>();
        }

        return dto.AntagonistGuids?
                   .Where(g => g != Guid.Empty)
                   .Distinct()
                   .ToList()
               ?? new List<Guid>();
    }


    private async Task UpdateMuscleMap(List<MuscleDTO> dtos)
    {
       _musclesCache.Clear();
       _musclesCache = dtos.ToDictionary(x => x.GUID, x => x);
    }

    public Task<Result<IReadOnlyList<MovementDTO>>> GetAllMovementDtoAsync()
    {
        throw new NotImplementedException();
    }
    
    
    private bool TryGetDescriptor(Guid? descriptorGuid, out DescriptorDTO? descriptorDto)
    {
        descriptorDto = null; 
        if(descriptorGuid == null || _descriptorCache.Count ==0 ) return false;
       
        return  _descriptorCache.TryGetValue(descriptorGuid.Value, out descriptorDto);
       
    }
}
