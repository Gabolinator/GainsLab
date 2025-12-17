using GainsLab.Application.DTOs;
using GainsLab.Application.Interfaces.Sync;
using GainsLab.Application.Results;
using GainsLab.Contracts.Dtos.SyncDto;
using GainsLab.Contracts.SyncService.Mapper;
using GainsLab.Domain;
using GainsLab.Domain.Interfaces;
using GainsLab.Infrastructure.SyncService.Mapper;


namespace GainsLab.Infrastructure.SyncService;

public class EntitySyncClient :IEntitySyncClient
{
    public readonly IRemoteProvider RemoteProvider;

    public EntitySyncClient (IRemoteProvider remoteProvider, ILogger logger)
    {
        RemoteProvider = remoteProvider;
        _logger = logger;
    }
    
    private Dictionary<Guid, DescriptorRecord>  _descriptorCache = new Dictionary<Guid, DescriptorRecord>();
    private Dictionary<Guid, MuscleRecord>  _musclesCache = new Dictionary<Guid, MuscleRecord>();
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


       var desc = await GetAllDescriptorRecordAsync();
       
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

 

    public async Task<Result<IReadOnlyList<EquipmentRecord>>> GetAllEquipmentsRecordAsync()
    {
      
        //make sure descriptor are up to date
        await UpdateDescriptorCache();

        var syncDtos = await GetAllEquipmentsSyncDtoAsync();
        if(!syncDtos.Success) return Result<IReadOnlyList<EquipmentRecord>>.Failure(syncDtos.GetErrorMessage());
        return Result<IReadOnlyList<EquipmentRecord>>.SuccessResult(syncDtos.Value != null
            ? syncDtos.Value.Select(s => EquipmentSyncMapper.FromSyncDTO(s, TryGetDescriptor(s.DescriptorGUID, out var descriptorDto) ? descriptorDto : null ,"sync")).ToList(): new());

        
    }

 

    public async Task<Result<IReadOnlyList<DescriptorRecord>>> GetAllDescriptorRecordAsync()
    {
        var syncDtos = await GetAllDescriptorSyncDtoAsync();
        if(!syncDtos.Success) return Result<IReadOnlyList<DescriptorRecord>>.Failure(syncDtos.GetErrorMessage());
        return Result<IReadOnlyList<DescriptorRecord>>.SuccessResult(syncDtos.Value != null
            ? syncDtos.Value.Select(s => DescriptorSyncMapper.FromSyncDTO(s, "sync")).ToList(): new());
    }

    public async Task<Result<IReadOnlyList<MuscleRecord>>> GetAllMusclesRecordAsync()
    {
        await UpdateDescriptorCache();
        
        var syncDtos = await GetAllMusclesSyncDtoAsync();
        if(!syncDtos.Success) return Result<IReadOnlyList<MuscleRecord>>.Failure(syncDtos.GetErrorMessage());

        var muscles = new List<MuscleRecord>();
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
                
        return Result<IReadOnlyList<MuscleRecord>>.SuccessResult(muscles);
    }

    private async Task ResolveAntagonists(
        List<MuscleRecord> Records,
        IReadOnlyDictionary<Guid, IReadOnlyList<Guid>> antagonistLookup)
    {
        if (Records.Count == 0) return;
        
        await UpdateMuscleMap(Records);
        if(_musclesCache.Count == 0) return;
        
        foreach (var Record in Records)
        {
            antagonistLookup.TryGetValue(Record.GUID, out var normalized);
            Record.Antagonists = TryGetAntagonist(Record, normalized);
        }
    }

    private ICollection<MuscleAntagonistRecord> TryGetAntagonist(
        MuscleRecord source,
        IReadOnlyList<Guid>? antagonistGuids)
    {
        if (antagonistGuids == null || antagonistGuids.Count == 0 || _musclesCache.Count == 0)
            return new List<MuscleAntagonistRecord>();

        var links = new List<MuscleAntagonistRecord>();
        
        foreach (var guid in antagonistGuids)
        {
            if (!_musclesCache.TryGetValue(guid, out var antagonist))
            {
                _logger.LogWarning(nameof(EntitySyncClient),
                    $"Unable to resolve antagonist {guid} for muscle {source.GUID}");
                continue;
            }

            var link = new MuscleAntagonistRecord
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


    private async Task UpdateMuscleMap(List<MuscleRecord> dtos)
    {
       _musclesCache.Clear();
       _musclesCache = dtos.ToDictionary(x => x.GUID, x => x);
    }

    public Task<Result<IReadOnlyList<MovementRecord>>> GetAllMovementRecordAsync()
    {
        throw new NotImplementedException();
    }
    
    
    private bool TryGetDescriptor(Guid? descriptorGuid, out DescriptorRecord? descriptorDto)
    {
        descriptorDto = null; 
        if(descriptorGuid == null || _descriptorCache.Count ==0 ) return false;
       
        return  _descriptorCache.TryGetValue(descriptorGuid.Value, out descriptorDto);
       
    }
}
