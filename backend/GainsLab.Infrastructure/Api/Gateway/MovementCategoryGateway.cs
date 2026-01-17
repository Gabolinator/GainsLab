using GainsLab.Application.Interfaces.DataManagement.Gateway;
using GainsLab.Application.Interfaces.DataManagement.Provider;
using GainsLab.Application.Results;
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
        return Result<MovementCategoryGetDTO>.NotImplemented(nameof(GetMovementCategoryAsync));
    }

    public async Task<Result<MovementCategoryUpdateCombinedOutcome>> UpdateMovementCategoryAsync(MovementCategoryUpdateRequest request,
        DescriptorUpdateRequest? descriptorUpdateRequest)
    {
        return Result<MovementCategoryUpdateCombinedOutcome>.NotImplemented(nameof(UpdateMovementCategoryAsync));
    }

    public async Task<Result<MovementCategoryDeleteOutcome>> DeleteMovementCategoryAsync(MovementCategoryEntityId id)
    {
        return Result<MovementCategoryDeleteOutcome>.NotImplemented(nameof(DeleteMovementCategoryAsync));
    }

    public async Task<Result<MovementCategoryCreateCombineOutcome>> CreateMovementCategoryAsync(MovementCategoryCombineCreateRequest request)
    {
        return Result<MovementCategoryCreateCombineOutcome>.NotImplemented(nameof(CreateMovementCategoryAsync));
    }
}