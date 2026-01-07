using GainsLab.Application.DTOs.Description;
using GainsLab.Application.DTOs.Equipment;
using GainsLab.Application.Interfaces.DataManagement.Gateway;
using GainsLab.Application.Interfaces.DataManagement.Provider;
using GainsLab.Application.Results;
using GainsLab.Contracts.Dtos.GetDto;
using GainsLab.Contracts.Dtos.SyncDto;
using GainsLab.Contracts.SyncService.Mapper;
using GainsLab.Domain.Interfaces;
using GainsLab.Infrastructure.SyncService;

namespace GainsLab.Infrastructure.Api.Gateway;

public class EquipmentGateway : IEquipmentGateway
{
    private readonly IEquipmentProvider _provider;
    private readonly ILogger _logger;
    private IDescriptorGateway _descriptorGateway;
  
    
    public EquipmentGateway(IEquipmentProvider equipmentProvider, IDescriptorGateway descriptorGateway ,ILogger logger)
    {
        _provider = equipmentProvider;
        _descriptorGateway = descriptorGateway;
        _logger = logger;
    }

    public async Task<Result<IReadOnlyList<EquipmentGetDTO>>> GetAllEquipmentsAsync()
    {
        var syncDtos = await GetAllEquipmentsSyncDtoAsync();
        if(!syncDtos.Success) return Result<IReadOnlyList<EquipmentGetDTO>>.Failure(syncDtos.GetErrorMessage());
        await _descriptorGateway.UpdateCacheAsync();
       
        return Result<IReadOnlyList<EquipmentGetDTO>>.SuccessResult((syncDtos.Value != null
            ? syncDtos.Value.Select(s => EquipmentSyncMapper.ToGetDTO(s, GetDescriptor(s.DescriptorGUID) ,s.UpdatedAtUtc,"sync")).ToList(): new())!);
        
    }

   

    private DescriptorRecord? GetDescriptor(Guid? descriptorGuid)
    {
       if(descriptorGuid == null) return null;
       return _descriptorGateway.TryGetDescriptor(descriptorGuid.Value, out var result) ? result : null;
    }
    
    
    public async Task<Result<IReadOnlyList<EquipmentSyncDTO>>> GetAllEquipmentsSyncDtoAsync()
    {
        var result =  await _provider.PullEquipmentPageAsync(SyncCursorUtil.MinValue, 200, default);;
        if (!result.Success)
        {
            return Result<IReadOnlyList<EquipmentSyncDTO>>.Failure(result.GetErrorMessage());
        }
        
        return Result<IReadOnlyList<EquipmentSyncDTO>>.SuccessResult(result.Value != null ? 
            result.Value.ItemsList.Cast<EquipmentSyncDTO>().ToList():
            new());

    }
    
    public async Task<Result<EquipmentGetDTO>> GetEquipmentByIdAsync(Guid id)
        => await  _provider.GetEquipmentAsync(new(id,null), default);
    
    
    
  
    
}