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
        if(request.CorrelationId == Guid.Empty) return Result<MuscleUpdateCombinedOutcome>.Failure("Invalid correlation id");
        
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
                _logger.LogWarning(nameof(MuscleGateway), $"Did not update descriptor {descriptorOutcome.GetMessages()}");
                message.Append(descriptorOutcome.GetMessages());
            }
            
            else 
            {
                descriptor = descriptorOutcome.Value!;
            }
        }
        
        if(descriptor == null) message.AddWarning("Did not update descriptor");
        else _descriptorGateway.Invalidate();
        
        MuscleUpdateOutcome? category = null;
        var categoryOutcome =  await _provider.UpdateMuscleAsync(request, default);
        
        if (!categoryOutcome.Success
            || categoryOutcome.Value == null
            || categoryOutcome.Value.Outcome == UpdateOutcome.Failed)
        {
            message.Append(categoryOutcome.GetMessages());
        }
        else category = categoryOutcome.Value!;
        
        if(category != null) cache?.Invalidate();
        
        return category == null && descriptor == null
            ? Result<MuscleUpdateCombinedOutcome>.Failure(message)
            : Result<MuscleUpdateCombinedOutcome>.SuccessResult(new MuscleUpdateCombinedOutcome(category, descriptor,
                message));
        
  //    return Result<MuscleUpdateCombinedOutcome>.NotImplemented(nameof(UpdateMuscleAsync));
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
     MuscleCreateOutcome? MuscleCreateOutcome = null;
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
         Result<MuscleCreateOutcome> MuscleOutcome =
             await _provider.CreateMuscleAsync(createCategoryRequest.Muscle!, default); //validated earlier
            
            
         if (!MuscleOutcome.Success ||MuscleOutcome.Value == null ||
             MuscleOutcome.Value.Outcome != CreateOutcome.Created)
         {
             _logger.LogWarning(nameof(MuscleGateway),
                 $"Did not create Muscle {MuscleOutcome.GetMessages()}");
             message.Append(MuscleOutcome.GetMessages());
         }

         else
         {
             MuscleCreateOutcome = MuscleOutcome.Value!;
             var createdDescriptor = MuscleCreateOutcome.CreatedMuscle?.Descriptor;
             descriptorCreateOutcome = createdDescriptor == null ? null : new DescriptorCreateOutcome(CreateOutcome.Created,createdDescriptor);
         }

         InvalidateCaches(cache);
     }
        
     //invalid
     else
     {
         message.Append(createCategoryValidation.Messages);
     }

        

     return MuscleCreateOutcome == null && descriptorCreateOutcome == null
         ? Result<MuscleCreateCombineOutcome>.Failure(message)
         : Result<MuscleCreateCombineOutcome>.SuccessResult(new MuscleCreateCombineOutcome(MuscleCreateOutcome, descriptorCreateOutcome,
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