using GainsLab.Application.Interfaces.DataManagement.Gateway;
using GainsLab.Application.Interfaces.DataManagement.Provider;
using GainsLab.Application.Results;
using GainsLab.Contracts.Dtos.Delete.Outcome;
using GainsLab.Contracts.Dtos.GetDto;
using GainsLab.Contracts.Dtos.ID;
using GainsLab.Contracts.Dtos.PostDto.Outcome;
using GainsLab.Contracts.Dtos.PostDto.Request;
using GainsLab.Contracts.Dtos.UpdateDto.Outcome;
using GainsLab.Contracts.Dtos.UpdateDto.Request;
using GainsLab.Domain.Interfaces;
using GainsLab.Infrastructure.Caching.QueryCache;
using GainsLab.Infrastructure.Caching.Registry;

namespace GainsLab.Infrastructure.Api.Gateway;

public class MovementCategoryGateway : IMovementCategoryGateway
{
    private readonly IMovementCategoryProvider _provider;
    private readonly ILogger _logger;
    private readonly DescriptorRegistry _descriptorGateway;
    private readonly MovementCategoryQueryCache _cache;
    
    public Task<Result<IReadOnlyList<MovementCategoryGetDTO>>> GetAllCategoryAsync()
    {
        throw new NotImplementedException();
    }

    public Task<Result<MovementCategoryGetDTO>> GetMovementCategoryAsync(MovementCategoryEntityId id)
    {
        throw new NotImplementedException();
    }

    public Task<Result<MovementCategoryUpdateCombinedOutcome>> UpdateMovementCategoryAsync(MovementCategoryUpdateRequest request,
        DescriptorUpdateRequest? descriptorUpdateRequest)
    {
        throw new NotImplementedException();
    }

    public Task<Result<MovementCategoryDeleteOutcome>> DeleteMovementCategoryAsync(MovementCategoryEntityId id)
    {
        throw new NotImplementedException();
    }

    public Task<Result<MovementCategoryCreateCombineOutcome>> CreateMovementCategoryAsync(MovementCategoryCombineCreateRequest request)
    {
        throw new NotImplementedException();
    }
}