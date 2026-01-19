using GainsLab.Application.DTOs.Description;
using GainsLab.Application.Interfaces.DataManagement.Gateway;
using GainsLab.Application.Results;
using GainsLab.Contracts.Dtos.GetDto;
using GainsLab.Contracts.Dtos.UpdateDto.Outcome;
using GainsLab.Contracts.Dtos.UpdateDto.Request;
using GainsLab.Domain.Interfaces;
using GainsLab.Infrastructure.Caching.QueryCache;

namespace GainsLab.Infrastructure.Caching.Registry;

public sealed class DescriptorRegistry
{
    private readonly IDescriptorGateway _gateway;
    private readonly DescriptorQueryCache _cache;
    private readonly ILogger _logger;
    public Dictionary<Guid,DescriptorGetDTO> Descriptors { get; private set; } = new Dictionary<Guid, DescriptorGetDTO>();

    public DescriptorRegistry(IDescriptorGateway gateway, DescriptorQueryCache cache, ILogger logger)
    {
        _gateway = gateway;
        _cache = cache;
        _logger = logger;
    }

    public async Task<Result<IReadOnlyList<DescriptorGetDTO>>> GetAllAsync(bool forceRefresh = false)
    {
        if (forceRefresh)
        {
            _cache.Invalidate();
        }
        else if (_cache.TryGetCompleted(out var cached))
        {
            if ( cached.Success &&  cached.HasValue)
            {
                Descriptors =  cached.Value.ToDictionary(d => d.Id, d => d);
            }
            return cached;
        }

        //return _cache.GetAllAsync(() => _gateway.GetAllDescriptorAsync());
        var result = await _cache.GetAllAsync(() => _gateway.GetAllDescriptorAsync());
        if (result.Success && result.HasValue)
        {
            Descriptors = result.Value.ToDictionary(d => d.Id, d => d);
        }
        return result;

        
    }

    public void Invalidate()
    {
        _cache.Invalidate();
        Descriptors.Clear();
    }



    public async Task<Result<DescriptorUpdateOutcome>> UpdateDescriptorAsync(DescriptorUpdateRequest updateDescriptor)
    {
       //
       var result = await _gateway.UpdateDescriptorAsync(updateDescriptor, _cache);
       
       if(!result.Success) return result;
       Invalidate();
        
       return result;
    }

    public async Task<DescriptorGetDTO?> GetDescriptorByIdAsync(Guid? id)
    {
      
        _logger.Log(nameof(GetDescriptorByIdAsync), $"Trying  to get descriptor for id {(id!=null ?id.ToString():"null" )}");
        if (id is null || id == Guid.Empty) return null;

        _logger.Log(nameof(GetDescriptorByIdAsync), $"Trying to get descriptor from descriptor cache");
        if (Descriptors.TryGetValue(id.Value, out var cached))
            return cached;

        _logger.Log(nameof(GetDescriptorByIdAsync), $"Didnt find in cache - Trying to get descriptor from query cache");

        var result = await GetAllAsync();
        return result.Success && Descriptors.TryGetValue(id.Value, out var refreshed)
            ? refreshed
            : null;
    }
}
