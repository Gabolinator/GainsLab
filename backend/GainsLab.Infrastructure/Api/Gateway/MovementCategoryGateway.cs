using GainsLab.Application.DTOs.Extensions;
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
using GainsLab.Contracts.SyncService.Mapper;
using GainsLab.Domain;
using GainsLab.Domain.Interfaces;
using GainsLab.Infrastructure.Caching.QueryCache;
using GainsLab.Infrastructure.Caching.Registry;
using GainsLab.Infrastructure.SyncService;
using GainsLab.Infrastructure.SyncService.Mapper;

namespace GainsLab.Infrastructure.Api.Gateway;

public class MovementCategoryGateway : IMovementCategoryGateway
{
    private readonly IMovementCategoryProvider _provider;
    private readonly ILogger _logger;
    private readonly DescriptorRegistry _descriptorGateway;
  
    public MovementCategoryGateway(IMovementCategoryProvider provider, DescriptorRegistry descriptorGateway ,ILogger logger)
    {
        _provider = provider;
        _descriptorGateway = descriptorGateway;
        _logger = logger;
    }

    
    
    public async Task<Result<IReadOnlyList<MovementCategoryGetDTO>>> GetAllCategoryAsync()
    {
        var syncDtos = await GetAllCategorySyncDtoAsync();

        if (!syncDtos.Success)
            return Result<IReadOnlyList<MovementCategoryGetDTO>>
                .Failure(syncDtos.GetErrorMessage());

        if (syncDtos.Value == null)
            return Result<IReadOnlyList<MovementCategoryGetDTO>>
                .SuccessResult(Array.Empty<MovementCategoryGetDTO>());

        var refs = syncDtos.Value!.ToDictionary(x => x.GUID, x=> x.ToRefDto());
        var baseRefs = GetBaseCategoryRefs(refs);

        var tasks = syncDtos.Value.Select(s =>
            MovementCategorySyncMapper.ToGetDTOAsync(
                s,
                GetDescriptorAsync(s.DescriptorGUID),
                s.UpdatedAtUtc,
                "sync",
                GetRef(s.ParentCategoryGUID, refs),
                GetBasesRefs(s.BaseCategories, baseRefs),
                null
            )
        );

        var dtos = await Task.WhenAll(tasks);
       
        
       AssignChild(dtos, refs);
        
        return Result<IReadOnlyList<MovementCategoryGetDTO>>
            .SuccessResult(dtos.ToList());
    }

    private void AssignChild(MovementCategoryGetDTO?[] dtos, Dictionary<Guid, MovementCategoryRefDTO> refs)
    {
        if (!dtos.Any() || !refs.Any()) return;
        
        var validDtos = dtos
            .Where(d => d is { ParentCategoryId: not null } 
                        && d.ParentCategoryId != Guid.Empty);
        if (!validDtos.Any()) return;
        
        //we need to check which refs has dto as parent
        foreach (var dto in dtos)
        {
            if(dto == null) continue;
            
            var dtosWithThatParent = validDtos.Where(d => d != null && d!.ParentCategoryId == dto.Id);
            var childs = dtosWithThatParent.Select(c =>
                refs.TryGetValue(c.Id, out var refDto) ? refDto : new MovementCategoryRefDTO(c.Id, ""));

            dto.ChildCategories = childs.ToList();
        }
        
    }
    
    

    private IReadOnlyList<MovementCategoryRefDTO>? GetBasesRefs(IEnumerable<eMovementCategories> basesCat, Dictionary<eMovementCategories, MovementCategoryRefDTO> refs)
    {
        if(!basesCat.Any() || !refs.Any()) return null;

        return basesCat.Select(c => refs.TryGetValue(c, out var refDto) ? refDto : null).Where(c=>c != null).ToList();

    }

    private MovementCategoryRefDTO? GetRef(Guid? id, Dictionary<Guid, MovementCategoryRefDTO> refs)
    {
       if(id == null || id == Guid.Empty || !refs.Any()) return null;
       
       return refs.TryGetValue(id.Value, out var dto) ? dto : null;
       
    }

    private Dictionary<eMovementCategories,MovementCategoryRefDTO> GetBaseCategoryRefs(Dictionary<Guid, MovementCategoryRefDTO> refs)
    {
        if (!refs.Any()) return new ();
        
        return refs
            .Where(r=> 
                !string.IsNullOrWhiteSpace(r.Value.Name) 
                && Enum.TryParse(r.Value.Name, out eMovementCategories category))
            .ToDictionary(x => Enum.Parse<eMovementCategories>(x.Value.Name), x=> x.Value);
    }

    


    private Task<DescriptorGetDTO?> GetDescriptorAsync(Guid? id)
        => _descriptorGateway.GetDescriptorByIdAsync(id);

    public async Task<Result<IReadOnlyList<MovementCategorySyncDTO>>> GetAllCategorySyncDtoAsync()
    {
        var result =  await _provider.PullMovementCategoryPageAsync(SyncCursorUtil.MinValue, 200, default);;
        if (!result.Success)
        {
            return Result<IReadOnlyList<MovementCategorySyncDTO>>.Failure(result.GetErrorMessage());
        }
        
        return Result<IReadOnlyList<MovementCategorySyncDTO>>.SuccessResult(result.Value != null ? 
            result.Value.ItemsList.Cast<MovementCategorySyncDTO>().ToList():
            new());

    }
    
    
    public async Task<Result<MovementCategoryGetDTO>> GetMovementCategoryAsync(MovementCategoryEntityId id)
    {
        if(!id.IsValid()) return Result<MovementCategoryGetDTO>.Failure("Invalid id");
        
        return await  _provider.GetMovementCategoryAsync(id, default);
        
        
    }

    public async Task<Result<MovementCategoryUpdateCombinedOutcome>> UpdateMovementCategoryAsync(
        MovementCategoryUpdateRequest request,
        DescriptorUpdateRequest? descriptorUpdateRequest,
        ICache? cache)
    {
        //return Result<MovementCategoryUpdateCombinedOutcome>.NotImplemented(nameof(UpdateMovementCategoryAsync));
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
        
        MovementCategoryUpdateOutcome? category = null;
        var categoryOutcome =  await _provider.UpdateMovementCategoryAsync(request, default);
        
        if (!categoryOutcome.Success
            || categoryOutcome.Value == null
            || categoryOutcome.Value.Outcome == UpdateOutcome.Failed)
        {
            message.Append(categoryOutcome.GetMessages());
        }
        else category = categoryOutcome.Value!;
        
        if(category != null) cache?.Invalidate();
        
        return category == null && descriptor == null
            ? Result<MovementCategoryUpdateCombinedOutcome>.Failure(message)
            : Result<MovementCategoryUpdateCombinedOutcome>.SuccessResult(new MovementCategoryUpdateCombinedOutcome(category, descriptor,
                message));
        
    }

    public async Task<Result<MovementCategoryDeleteOutcome>> DeleteMovementCategoryAsync(MovementCategoryEntityId id, ICache? cache)
    {
        if (!id.IsValid())
        {
            return Result<MovementCategoryDeleteOutcome>.Failure("Invalid id");
        }
        
        var result = await _provider.DeleteMovementCategoryAsync(id, default);
        if (result.Success)
        {
            InvalidateCaches(cache);
        }
        return result;
    }

    public async Task<Result<MovementCategoryCreateCombineOutcome>> CreateMovementCategoryAsync(MovementCategoryCombineCreateRequest request, ICache? cache)
    {
     //   return Result<MovementCategoryCreateCombineOutcome>.NotImplemented(nameof(CreateMovementCategoryAsync));
     MessagesContainer message = new MessagesContainer();

     DescriptorCreateOutcome? descriptorCreateOutcome = null;
     MovementCategoryCreateOutcome? equipmentCreateOutcome = null;
     var createDescriptorRequest = request.Descriptor;
     var createCategoryRequest = request.MovementCategory;

     var createDescriptorValidation = createDescriptorRequest.IsCreateRequestValid();
     
     //request is invalid - just get the errors
     if (!createDescriptorValidation.Success)
     {
         message.Append(createDescriptorValidation.Messages);
     }
        
     var createCategoryValidation = createCategoryRequest.IsCreateRequestValid();
     
     //valid
     if (createCategoryValidation.Success)
     {
         Result<MovementCategoryCreateOutcome> equipmentOutcome =
             await _provider.CreateMovementCategoryAsync(createCategoryRequest.MovementCategory!, default); //validated earlier
            
            
         if (!equipmentOutcome.Success ||equipmentOutcome.Value == null ||
             equipmentOutcome.Value.Outcome != CreateOutcome.Created)
         {
             _logger.LogWarning(nameof(MovementCategoryGateway),
                 $"Did not create MovementCategory {equipmentOutcome.GetMessages()}");
             message.Append(equipmentOutcome.GetMessages());
         }

         else
         {
             equipmentCreateOutcome = equipmentOutcome.Value!;
             var createdDescriptor = equipmentCreateOutcome.CreatedMovementCategory?.Descriptor;
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
         ? Result<MovementCategoryCreateCombineOutcome>.Failure(message)
         : Result<MovementCategoryCreateCombineOutcome>.SuccessResult(new MovementCategoryCreateCombineOutcome(equipmentCreateOutcome, descriptorCreateOutcome,
             message));

     
    }
    
    private void InvalidateCaches(ICache? cache)
    {
        _descriptorGateway.Invalidate();
        cache?.Invalidate();
    }

}