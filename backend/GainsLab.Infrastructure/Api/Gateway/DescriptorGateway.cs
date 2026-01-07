using GainsLab.Application.DTOs.Description;
using GainsLab.Application.Interfaces.DataManagement.Gateway;
using GainsLab.Application.Interfaces.DataManagement.Provider;
using GainsLab.Application.Results;
using GainsLab.Contracts.Dtos.SyncDto;
using GainsLab.Contracts.SyncService.Mapper;
using GainsLab.Domain.Interfaces;
using GainsLab.Infrastructure.SyncService;

namespace GainsLab.Infrastructure.Api.Gateway;

public class DescriptorGateway : IDescriptorGateway
{
    private readonly IDescriptorProvider _provider;
    private readonly ILogger _logger;

    public DescriptorGateway(IDescriptorProvider provider, ILogger logger)
    {
        _provider = provider;
        _logger = logger;
    }

    private Dictionary<Guid, DescriptorRecord>  _descriptorCache = new Dictionary<Guid, DescriptorRecord>();
    public bool DescriptorCached => _descriptorCache.Count > 0;
    
    private async Task<bool> IsDescriptorCacheUpToDate()
    {
        //todo
        //return true for now
        return DescriptorCached;
    }
    
    private async Task UpdateDescriptorCache()
    {
        if(await IsDescriptorCacheUpToDate()) return;


        var desc = await GetAllDescriptorAsync();
       
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

    public bool TryGetDescriptor(Guid descriptorGuid, out DescriptorRecord? descriptor)
        => _descriptorCache.TryGetValue(descriptorGuid, out descriptor);

   

    public async Task<Result<IReadOnlyList<DescriptorRecord>>> GetAllDescriptorAsync()
    {
        var syncDtos = await GetAllDescriptorSyncDtoAsync();
        if(!syncDtos.Success) return Result<IReadOnlyList<DescriptorRecord>>.Failure(syncDtos.GetErrorMessage());
        return Result<IReadOnlyList<DescriptorRecord>>.SuccessResult(syncDtos.Value != null
            ? syncDtos.Value.Select(s => DescriptorSyncMapper.FromSyncDTO(s, "sync")).ToList(): new());
    }

    public Task UpdateCacheAsync() => UpdateDescriptorCache();

    public async Task<Result<IReadOnlyList<DescriptorSyncDTO>>> GetAllDescriptorSyncDtoAsync()
    {
        var result =  await _provider.PullDescriptorPageAsync(SyncCursorUtil.MinValue,200,default);
        if (!result.Success)
        {
            return Result<IReadOnlyList<DescriptorSyncDTO>>.Failure(result.GetErrorMessage());
        }
        
        return Result<IReadOnlyList<DescriptorSyncDTO>>.SuccessResult(result.Value != null ? 
            result.Value.ItemsList.Cast<DescriptorSyncDTO>().ToList():
            new());
    }
    
    public async Task<Result<DescriptorRecord>> TryGetDescriptorAsync(Guid? descriptorGuid)
    {
        if(descriptorGuid == null) return Result<DescriptorRecord>.Failure("No descriptor found : Invalid Guid");
        
        await UpdateDescriptorCache();
        
        
        if(_descriptorCache.Count ==0 ) return Result<DescriptorRecord>.Failure("No descriptors found");

       
        return !TryGetDescriptor(descriptorGuid.Value, out var descriptor) || descriptor == null
            ? Result<DescriptorRecord>.Failure("No descriptor found") 
            : Result<DescriptorRecord>.SuccessResult(descriptor);
    }
    
}