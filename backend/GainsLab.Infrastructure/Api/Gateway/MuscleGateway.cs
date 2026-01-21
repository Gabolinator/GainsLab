using System.Security.Cryptography.Xml;
using GainsLab.Application.DTOs.Extensions;
using GainsLab.Application.DTOs.Muscle;
using GainsLab.Application.Interfaces.DataManagement;
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
using GainsLab.Contracts.Dtos.UpdateDto.Outcome;
using GainsLab.Contracts.Dtos.UpdateDto.Request;
using GainsLab.Domain.Interfaces;
using GainsLab.Infrastructure.Caching.Registry;
using GainsLab.Infrastructure.SyncService;
using GainsLab.Infrastructure.SyncService.Mapper;

namespace GainsLab.Infrastructure.Api.Gateway;

public class MuscleGateway : IMuscleGateway
{
    private readonly IMuscleProvider _provider;
    private readonly ILogger _logger;
    private readonly DescriptorRegistry _descriptorGateway;
    
    public  MuscleGateway(IMuscleProvider provider, DescriptorRegistry descriptorGateway ,ILogger logger)
    {
        _provider = provider;
        _descriptorGateway = descriptorGateway;
        _logger = logger;
    }

    public async Task<Result<IReadOnlyList<MuscleGetDTO>>> GetAllMusclesAsync()
    {
        var syncDtos = await GetAllMuscleSyncDtoAsync();

        if (!syncDtos.Success)
            return Result<IReadOnlyList<MuscleGetDTO>>
                .Failure(syncDtos.GetErrorMessage());

        if (syncDtos.Value == null)
            return Result<IReadOnlyList<MuscleGetDTO>>
                .SuccessResult(Array.Empty<MuscleGetDTO>());

       
        var refs = syncDtos.Value!.ToDictionary(x => x.GUID, x=> x.ToRefDto());
        
        
        
        var tasks = syncDtos.Value.Select(s =>
           MuscleSyncMapper.ToGetDTOAsync(
                s,
                GetDescriptorAsync(s.DescriptorGUID),
                GetAntagonistRefs(refs,s.AntagonistGuids),
                s.UpdatedAtUtc,
                "sync"
            )
        );

        var dtos = await Task.WhenAll(tasks);
        
        return Result<IReadOnlyList<MuscleGetDTO>>
            .SuccessResult(dtos!);
        
    }

    public async Task<Result<MuscleGetDTO>> GetMuscleAsync(MuscleEntityId id)
    {
       return Result<MuscleGetDTO>.NotImplemented(nameof(GetMuscleAsync));
    }

    public async Task<Result<MuscleUpdateCombinedOutcome>> UpdateMuscleAsync(MuscleUpdateRequest request, DescriptorUpdateRequest? descriptorUpdateRequest, ICache? cache)
    {
      return Result<MuscleUpdateCombinedOutcome>.NotImplemented(nameof(UpdateMuscleAsync));
    }

    public async Task<Result<MuscleDeleteOutcome>> DeleteMuscleAsync(MuscleEntityId id, ICache? cache)
    {
        if (!id.IsValid())
        {
            return Result<MuscleDeleteOutcome>.Failure("Invalid id");
        }
        
        var result = await _provider.DeleteMuscleAsync(id, default);
        if (result.Success)
        {
            InvalidateCaches(cache);
        }
        return result;
    }

    public async Task<Result<MuscleCreateCombineOutcome>> CreateMuscleAsync(MuscleCombineCreateRequest request, ICache? cache)
    {
        MessagesContainer message = new MessagesContainer();

     DescriptorCreateOutcome? descriptorCreateOutcome = null;
     MuscleCreateOutcome? equipmentCreateOutcome = null;
     var createDescriptorRequest = request.Descriptor;
     var createCategoryRequest = request.Muscle;

     var createDescriptorValidation = createDescriptorRequest.IsCreateRequestValid();
     
     //request is invalid - just get the errors
     if (!createDescriptorValidation.Success)
     {
         message.Append(createDescriptorValidation.Messages);
     }
        
     Result createCategoryValidation = createCategoryRequest.IsCreateRequestValid();
     
     //valid
     if (createCategoryValidation.Success)
     {
         Result<MuscleCreateOutcome> equipmentOutcome =
             await _provider.CreateMuscleAsync(createCategoryRequest.Muscle!, default); //validated earlier
            
            
         if (!equipmentOutcome.Success ||equipmentOutcome.Value == null ||
             equipmentOutcome.Value.Outcome != CreateOutcome.Created)
         {
             _logger.LogWarning(nameof(MuscleGateway),
                 $"Did not create Muscle {equipmentOutcome.GetMessages()}");
             message.Append(equipmentOutcome.GetMessages());
         }

         else
         {
             equipmentCreateOutcome = equipmentOutcome.Value!;
             var createdDescriptor = equipmentCreateOutcome.CreatedMuscle?.Descriptor;
             descriptorCreateOutcome = createdDescriptor == null ? null : new DescriptorCreateOutcome(CreateOutcome.Created,createdDescriptor);
         }

         InvalidateCaches(cache);
     }
        
     //invalid
     else
     {
         message.Append(createCategoryValidation.Messages);
     }

        

     return equipmentCreateOutcome == null && descriptorCreateOutcome == null
         ? Result<MuscleCreateCombineOutcome>.Failure(message)
         : Result<MuscleCreateCombineOutcome>.SuccessResult(new MuscleCreateCombineOutcome(equipmentCreateOutcome, descriptorCreateOutcome,
             message));
     
    }

    private void InvalidateCaches(ICache? cache)
    {
        _descriptorGateway.Invalidate();
        cache?.Invalidate();
    }

    IReadOnlyList<MuscleRefDTO>? GetAntagonistRefs(Dictionary<Guid, MuscleRefDTO> dict, IReadOnlyList<Guid>? antonistGuids)
    {
        if(antonistGuids == null || !antonistGuids.Any() || !dict.Any()) return null;
        
        
        MuscleRefDTO GetRef(Guid guid)
        {
           return dict.TryGetValue(guid, out var result) ? result :new MuscleRefDTO(guid, "");
        }
        
        return antonistGuids.Select(GetRef).ToList();
        
    }

   

    private Task<DescriptorGetDTO?> GetDescriptorAsync(Guid? id)
        => _descriptorGateway.GetDescriptorByIdAsync(id);


    public async Task<Result<IReadOnlyList<MuscleSyncDTO>>>GetAllMuscleSyncDtoAsync()
    {
        var result =  await _provider.PullMusclePageAsync(SyncCursorUtil.MinValue, 200, default);;
        if (!result.Success)
        {
            return Result<IReadOnlyList<MuscleSyncDTO>>.Failure(result.GetErrorMessage());
        }
        
        return Result<IReadOnlyList<MuscleSyncDTO>>.SuccessResult(result.Value != null ? 
            result.Value.ItemsList.Cast<MuscleSyncDTO>().ToList():
            new());

    }

    
}