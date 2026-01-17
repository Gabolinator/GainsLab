using GainsLab.Application.Results;
using GainsLab.Contracts.Dtos.Delete.Outcome;
using GainsLab.Contracts.Dtos.GetDto;
using GainsLab.Contracts.Dtos.ID;
using GainsLab.Contracts.Dtos.PostDto;
using GainsLab.Contracts.Dtos.PostDto.Outcome;
using GainsLab.Contracts.Dtos.UpdateDto.Outcome;
using GainsLab.Contracts.Dtos.UpdateDto.Request;
using GainsLab.Contracts.Interface;
using GainsLab.Domain.Interfaces;
using GainsLab.Infrastructure.Api.Interface;

namespace GainsLab.Infrastructure.Api;

public class MovementCategoryApi : IMovementCategoryApi
{
    private readonly HttpClient _http;
    private readonly ILogger _logger;

    public  MovementCategoryApi(HttpClient http, ILogger logger)
    {
        _http = http;
        _logger = logger;
    }
    
    
    public Task<Result<ISyncPage<ISyncDto>>> PullMovementCategoryPageAsync(ISyncCursor cursor, int take, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public Task<Result<MovementCategoryGetDTO>> GetMovementCategoryAsync(MovementCategoryEntityId entity, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public Task<Result<MovementCategoryCreateOutcome>> CreateMovementCategoryAsync(MovementCategoryPostDTO entity, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public Task<Result<MovementCategoryUpdateOutcome>> UpdateMovementCategoryAsync(MovementCategoryUpdateRequest request, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public Task<Result<MovementCategoryDeleteOutcome>> DeleteMovementCategoryAsync(MovementCategoryEntityId entity, CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}