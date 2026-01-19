using GainsLab.Application.Interfaces.DataManagement.Gateway;
using GainsLab.Application.Results;
using GainsLab.Contracts.Dtos.Delete.Outcome;
using GainsLab.Contracts.Dtos.GetDto;
using GainsLab.Contracts.Dtos.ID;
using GainsLab.Contracts.Dtos.PostDto.Outcome;
using GainsLab.Contracts.Dtos.PostDto.Request;
using GainsLab.Contracts.Dtos.UpdateDto.Outcome;
using GainsLab.Contracts.Dtos.UpdateDto.Request;
using GainsLab.Infrastructure.Caching.QueryCache;

namespace GainsLab.Infrastructure.Caching.Registry;

public sealed class EquipmentRegistry
{
    private readonly IEquipmentGateway _gateway;
    private readonly EquipmentQueryCache _cache;
    public Dictionary<Guid,EquipmentGetDTO> Equipments { get; private set; } = new Dictionary<Guid, EquipmentGetDTO>();


    public EquipmentRegistry(IEquipmentGateway gateway, EquipmentQueryCache cache)
    {
        _gateway = gateway;
        _cache = cache;
    }

    public async Task<Result<IReadOnlyList<EquipmentGetDTO>>> GetAllAsync(bool forceRefresh = false)
    {
        if (forceRefresh)
        {
            _cache.Invalidate();
        }
        else if (_cache.TryGetCompleted(out var cached))
        {
            return cached;
        }
        
       var result = await _cache.GetAllAsync(() => _gateway.GetAllEquipmentsAsync());
       if (result.Success && result.HasValue)
       {
           Equipments = result.Value.ToDictionary(d => d.Id, d => d);
       }
       return result;
    }

    public void Invalidate()
    {
        
        _cache.Invalidate();
        Equipments.Clear();
    }

    public async Task<Result<EquipmentGetDTO>> GetByIdAsync(Guid id, bool forceRefresh = false)
    {
        if (id == Guid.Empty)
        {
            return Result<EquipmentGetDTO>.Failure("Invalid equipment id");
        }

        var listResult = await GetAllAsync(forceRefresh);
        if (!listResult.Success)
        {
            return Result<EquipmentGetDTO>.Failure(listResult.GetErrorMessage());
        }

        if (!listResult.HasValue || listResult.Value is null)
        {
            return Result<EquipmentGetDTO>.Failure("No equipments available");
        }

        var match = listResult.Value.FirstOrDefault(e => e.Id == id);
        return match is null
            ? Result<EquipmentGetDTO>.Failure($"Equipment {id} not found")
            : Result<EquipmentGetDTO>.SuccessResult(match);
    }

    public async Task<Result<EquipmentDeleteOutcome>> DeleteEquipmentAsync(EquipmentEntityId equipmentEntityId)
    {
        var result = await _gateway.DeleteEquipmentAsync(equipmentEntityId, _cache);
        if(result.Success) Invalidate();
        return result;
    }
    
    
    public async Task<EquipmentGetDTO?> GetEquipmentByIdAsync(Guid? id)
    {
        if (id is null || id == Guid.Empty) return null;

        if (Equipments.TryGetValue(id.Value, out var cached))
            return cached;

        var result = await GetAllAsync();
        return result.Success && Equipments.TryGetValue(id.Value, out var refreshed)
            ? refreshed
            : null;
    }

    public async  Task<Result<EquipmentUpdateCombinedOutcome>> UpdateEquipmentAsync(EquipmentUpdateRequest request, DescriptorUpdateRequest toUpdateRequest)
    {
        var result = await _gateway.UpdateEquipmentAsync(request,toUpdateRequest, _cache);
        if(result.Success) _cache.Invalidate();
        return result;
    }

    public async Task<Result<EquipmentCreateCombineOutcome>> CreateEquipmentAsync(EquipmentCombineCreateRequest request)
        => await _gateway.CreateEquipmentAsync(request, _cache);
}
