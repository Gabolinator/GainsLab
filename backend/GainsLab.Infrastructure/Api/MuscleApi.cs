using GainsLab.Application.Interfaces;
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

public class MuscleApi : IMuscleApi
{
    private readonly HttpClient _http;
    private readonly ILogger _logger;
    private readonly INetworkChecker _networkChecker;
    
    public  MuscleApi(HttpClient http, ILogger logger, INetworkChecker networkChecker)
    {
        _http = http;
        _logger = logger;
        _networkChecker = networkChecker;
    }
    
    public Task<Result<ISyncPage<ISyncDto>>> PullMusclePageAsync(ISyncCursor cursor, int take, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public Task<Result<MuscleGetDTO>> GetMuscleAsync(MuscleEntityId entity, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public Task<Result<MuscleCreateOutcome>> CreateMuscleAsync(MusclePostDTO entity, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public Task<Result<MuscleUpdateOutcome>> UpdateMuscleAsync(MuscleUpdateRequest request, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public Task<Result<MuscleDeleteOutcome>> DeleteMuscleAsync(MuscleEntityId entity, CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}