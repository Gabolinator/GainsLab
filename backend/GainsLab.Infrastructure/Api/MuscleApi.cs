using System.Net.Http.Json;
using GainsLab.Application.Interfaces;
using GainsLab.Application.Results;
using GainsLab.Contracts;
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
using GainsLab.Infrastructure.Utilities;

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
    
    public async Task<Result<ISyncPage<ISyncDto>>> PullMusclePageAsync(ISyncCursor cursor, int take, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public async Task<Result<MuscleGetDTO>> GetMuscleAsync(MuscleEntityId entity, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public async Task<Result<MuscleCreateOutcome>> CreateMuscleAsync(MusclePostDTO entity, CancellationToken ct)
    {
         var id = entity.Id;
        if (id == Guid.Empty)
        {
            return  Result<MuscleCreateOutcome>.Failure("Did not create Muscle - ID invalid");
        }
        
        if (string.IsNullOrWhiteSpace(entity.Name))
        {
            return  Result<MuscleCreateOutcome>.Failure("Did not create Muscle - name empty");
        }
        
        if (!await _networkChecker.HasInternetAsync(_logger))
        {
            var message = $"Unable to reach sync server at {_http.DescribeBaseAddress()} - no internet connection detected.";
            _logger.LogWarning(nameof(MuscleApi), message);
            return Result<MuscleCreateOutcome>.Failure(message);
        }

        try
        {
            _logger.Log(nameof(MuscleApi), $"Try Post Muscle - {entity.Print()}" );

            var url = $"/muscles/";
            using var res = await _http.PostAsync(url, JsonContent.Create(entity), ct);
            res.EnsureSuccessStatusCode();
            
            _logger.Log(nameof(MuscleApi), $"Post Muscle - id {id} - {res.Content}" );
            
            var payload = await res.Content.ReadFromJsonAsync<MuscleGetDTO>(cancellationToken:ct);
            if (payload == null)
            {
                return Result<MuscleCreateOutcome>.Failure($"Did not create Muscle - id : {res.ReasonPhrase}");
            }

            return Result<MuscleCreateOutcome>.SuccessResult(
                new MuscleCreateOutcome(CreateOutcome.Created, payload));

        }
        
        catch (OperationCanceledException)
        {
            var message =
                $"Remote post for Muscle failed because operation vas cancelled";
            _logger.LogError(nameof(MuscleApi), message);
            return  Result<MuscleCreateOutcome>.Failure(message);
        }
        catch (Exception e)
        {
            var message =
                $"Remote post for Muscle failed while contacting {_http.DescribeBaseAddress()}: {e.GetBaseException().Message}";
            _logger.LogError(nameof(MuscleApi), message);
            return  Result<MuscleCreateOutcome>.Failure(message);
        }

    }

    public async Task<Result<MuscleUpdateOutcome>> UpdateMuscleAsync(MuscleUpdateRequest request, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public async Task<Result<MuscleDeleteOutcome>> DeleteMuscleAsync(MuscleEntityId entity, CancellationToken ct)
    {
      if(!entity.IsValid()) return Result<MuscleDeleteOutcome>.Failure("Id invalid");
      
      if (!entity.IsIdValid())
      {
          return Result<MuscleDeleteOutcome>.Failure("Invalid entity ID");
      }
        
      if (!await _networkChecker.HasInternetAsync(_logger))
      {
          var message = $"Unable to reach sync server at {_http.DescribeBaseAddress()} - no internet connection detected.";
          _logger.LogWarning(nameof(MuscleApi), message);
          return Result<MuscleDeleteOutcome>.Failure(message);
      }

      try
      {
          var id= entity.Id!.Value;
          _logger.Log(nameof(MuscleApi), $"Try Delete Muscle - id {id}" );

            
          var url = $"/muscles/{Uri.EscapeDataString(id.ToString()!)}";
          using var res = await _http.DeleteAsync(url, ct);
          res.EnsureSuccessStatusCode();
            
            
          _logger.Log(nameof(MuscleApi), $"Delete Muscle - id {id} - {res.Content}" );

            
          return Result<MuscleDeleteOutcome>.SuccessResult(new MuscleDeleteOutcome(new MuscleEntityId(id),DeleteOutcome.Deleted));
      }
        
        
      catch (OperationCanceledException)
      {
          var message =
              $"Remote Delete for Muscle failed because operation vas cancelled";
          _logger.LogError(nameof(MuscleApi), message);
          return Result<MuscleDeleteOutcome>.Failure(message);
      }
        
      catch (Exception e)
      {
          var message =
              $"Remote Delete for Muscle failed while contacting {_http.DescribeBaseAddress()}: {e.GetBaseException().Message}";
          _logger.LogError(nameof(MuscleApi), message);
          return Result<MuscleDeleteOutcome>.Failure(message);
      }
      
    }
}