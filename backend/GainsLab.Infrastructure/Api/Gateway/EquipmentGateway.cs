using GainsLab.Application.DTOs.Description;
using GainsLab.Application.DTOs.Extensions;
using GainsLab.Application.Interfaces.DataManagement.Gateway;
using GainsLab.Application.Interfaces.DataManagement.Provider;
using GainsLab.Application.Results;
using GainsLab.Contracts;
using GainsLab.Contracts.Dtos.Delete.Outcome;
using GainsLab.Contracts.Dtos.GetDto;
using GainsLab.Contracts.Dtos.ID;
using GainsLab.Contracts.Dtos.PostDto.Outcome;
using GainsLab.Contracts.Dtos.PostDto.Request;
using GainsLab.Contracts.Dtos.SyncDto;
using GainsLab.Contracts.Dtos.UpdateDto;
using GainsLab.Contracts.Dtos.UpdateDto.Outcome;
using GainsLab.Contracts.Dtos.UpdateDto.Request;
using GainsLab.Contracts.SyncService.Mapper;
using GainsLab.Domain.Interfaces;
using GainsLab.Infrastructure.Caching.QueryCache;
using GainsLab.Infrastructure.Caching.Registry;
using GainsLab.Infrastructure.SyncService;

namespace GainsLab.Infrastructure.Api.Gateway;

public class EquipmentGateway : IEquipmentGateway
{
    private readonly IEquipmentProvider _provider;
    private readonly ILogger _logger;
    private readonly DescriptorRegistry _descriptorGateway;
    private readonly EquipmentQueryCache _cache;
    
    
    public EquipmentGateway(IEquipmentProvider equipmentProvider, DescriptorRegistry descriptorGateway ,ILogger logger, EquipmentQueryCache cache)
    {
        _provider = equipmentProvider;
        _descriptorGateway = descriptorGateway;
        _logger = logger;
        _cache = cache;
    }

    public async Task<Result<IReadOnlyList<EquipmentGetDTO>>> GetAllEquipmentsAsync()
    {
        var syncDtos = await GetAllEquipmentsSyncDtoAsync();

        if (!syncDtos.Success)
            return Result<IReadOnlyList<EquipmentGetDTO>>
                .Failure(syncDtos.GetErrorMessage());

        if (syncDtos.Value == null)
            return Result<IReadOnlyList<EquipmentGetDTO>>
                .SuccessResult(Array.Empty<EquipmentGetDTO>());

        var tasks = syncDtos.Value.Select(s =>
            EquipmentSyncMapper.ToGetDTOAsync(
                s,
                GetDescriptorAsync(s.DescriptorGUID),
                s.UpdatedAtUtc,
                "sync"
            )
        );

        var dtos = await Task.WhenAll(tasks);
        
        return Result<IReadOnlyList<EquipmentGetDTO>>
            .SuccessResult(dtos!);
        
    }

    private Task<DescriptorGetDTO?> GetDescriptorAsync(Guid? id)
        => _descriptorGateway.GetDescriptorByIdAsync(id);
        
    


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
    
    
    public async Task<Result<EquipmentGetDTO>> GetEquipmentAsync(EquipmentEntityId id)
    {
        
        if(!id.IsValid()) return Result<EquipmentGetDTO>.Failure("Invalid id");
        
        return await  _provider.GetEquipmentAsync(id, default);
    }
        

    public async Task<Result<EquipmentUpdateCombinedOutcome>> UpdateEquipmentAsync(EquipmentUpdateRequest request, DescriptorUpdateRequest? descriptorUpdateRequest)
    {
        
       MessagesContainer message = new MessagesContainer();
       DescriptorUpdateOutcome? descriptor = null;
    

       
        //start by trying to update description
        if (descriptorUpdateRequest != null &&
            descriptorUpdateRequest!.UpdateRequest == UpdateRequest.Update)
        {
            var descriptorOutcome = await _descriptorGateway.UpdateDescriptorAsync(descriptorUpdateRequest);
            
            if (!descriptorOutcome.Success || descriptorOutcome.Value == null ||
                descriptorOutcome.Value.Outcome == UpdateOutcome.Failed)
            {
                _logger.LogWarning(nameof(EquipmentGateway), $"Did not update descriptor {descriptorOutcome.GetMessages()}");
                message.Append(descriptorOutcome.GetMessages());
            }
            
            else 
            {
                descriptor = descriptorOutcome.Value!;
            }
        }
        
        if(descriptor == null) message.AddWarning("Did not update descriptor");
        else _descriptorGateway.Invalidate();
        
        EquipmentUpdateOutcome equipment = null;
        //them update this 
        var equipmentOutcome = await _provider.UpdateEquipmentAsync(request, CancellationToken.None);
        if (!equipmentOutcome.Success || equipmentOutcome.Value == null ||
            equipmentOutcome.Value.Outcome == UpdateOutcome.Failed)
        {
            message.Append(equipmentOutcome.GetMessages());
        }
        
        else equipment = equipmentOutcome.Value!;

        if(equipment != null) _cache.Invalidate();
       

        return equipment == null && descriptor == null
            ? Result<EquipmentUpdateCombinedOutcome>.Failure(message)
            : Result<EquipmentUpdateCombinedOutcome>.SuccessResult(new EquipmentUpdateCombinedOutcome(equipment, descriptor,
                message));


    }

    public async Task<Result<EquipmentDeleteOutcome>> DeleteEquipmentAsync(EquipmentEntityId request)
    {
        if (!request.IsValid())
        {
            return Result<EquipmentDeleteOutcome>.Failure("Invalid id");
        }

        var result = await _provider.DeleteEquipmentAsync(request, default);
        if (result.Success)
        {
           InvalidateCaches();
        }
        return result;
    }

    private void InvalidateCaches()
    {
        _descriptorGateway.Invalidate();
        _cache.Invalidate();
    }

    public async Task<Result<EquipmentCreateCombineOutcome>> CreateEquipmentAsync(EquipmentCombineCreateRequest request)
    {
        MessagesContainer message = new MessagesContainer();

        DescriptorCreateOutcome? descriptorCreateOutcome = null;
        EquipmentCreateOutcome? equipmentCreateOutcome = null;
        var createDescriptorRequest = request.Descriptor;
        var createEquipmentRequest = request.Equipment;


        var createDescriptorValidation = createDescriptorRequest.IsCreateRequestValid();

        //request is invalid - just get the errors
        if (!createDescriptorValidation.Success)
        {
            message.Append(createDescriptorValidation.Messages);
        }
        
        var createEquipmentValidation = createEquipmentRequest.IsCreateRequestValid();
        
        //valid
        if (createEquipmentValidation.Success)
        {
            Result<EquipmentCreateOutcome> equipmentOutcome =
                await _provider.CreateEquipmentAsync(createEquipmentRequest.Equipment!, default); //validated earlier
            
            
            if (!equipmentOutcome.Success ||equipmentOutcome.Value == null ||
                equipmentOutcome.Value.Outcome != CreateOutcome.Created)
            {
                _logger.LogWarning(nameof(EquipmentGateway),
                    $"Did not create equipment {equipmentOutcome.GetMessages()}");
                message.Append(equipmentOutcome.GetMessages());
            }

            else
            {
                equipmentCreateOutcome = equipmentOutcome.Value!;
                var createdDescriptor = equipmentCreateOutcome.CreatedEquipment?.Descriptor;
                descriptorCreateOutcome = createdDescriptor == null ? null : new DescriptorCreateOutcome(CreateOutcome.Created,createdDescriptor);
            }

            InvalidateCaches();
        }
        
        //invalid
        else
        {
            message.Append(createEquipmentValidation.Messages);
        }

        

        return equipmentCreateOutcome == null && descriptorCreateOutcome == null
            ? Result<EquipmentCreateCombineOutcome>.Failure(message)
            : Result<EquipmentCreateCombineOutcome>.SuccessResult(new EquipmentCreateCombineOutcome(equipmentCreateOutcome, descriptorCreateOutcome,
                message));
        
        
    }
}