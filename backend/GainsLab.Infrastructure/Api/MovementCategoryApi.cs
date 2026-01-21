using System.Net.Http.Json;
using GainsLab.Application.Interfaces;
using GainsLab.Application.Results;
using GainsLab.Contracts;
using GainsLab.Contracts.Dtos.Delete.Outcome;
using GainsLab.Contracts.Dtos.GetDto;
using GainsLab.Contracts.Dtos.ID;
using GainsLab.Contracts.Dtos.PostDto;
using GainsLab.Contracts.Dtos.PostDto.Outcome;
using GainsLab.Contracts.Dtos.SyncDto;
using GainsLab.Contracts.Dtos.UpdateDto.Outcome;
using GainsLab.Contracts.Dtos.UpdateDto.Request;
using GainsLab.Contracts.Interface;
using GainsLab.Domain;
using GainsLab.Domain.Interfaces;
using GainsLab.Infrastructure.Api.Interface;
using GainsLab.Infrastructure.SyncService;
using GainsLab.Infrastructure.Utilities;

namespace GainsLab.Infrastructure.Api;

public class MovementCategoryApi : IMovementCategoryApi
{
    private readonly HttpClient _http;
    private readonly ILogger _logger;
    private readonly INetworkChecker _networkChecker;

    public  MovementCategoryApi(HttpClient http, ILogger logger, INetworkChecker networkChecker)
    {
        _http = http;
        _logger = logger;
        _networkChecker = networkChecker;
    }
    
    
    public async Task<Result<ISyncPage<ISyncDto>>> PullMovementCategoryPageAsync(ISyncCursor cursor, int take, CancellationToken ct)
    {
        if (!await _networkChecker.HasInternetAsync(_logger))
        {
            var message = $"Unable to reach sync server at {_http.DescribeBaseAddress()} - no internet connection detected.";
            _logger.LogWarning(nameof(MovementCategoryApi), message);
            return Result<ISyncPage<ISyncDto>>.Failure(message);
        }
        
        try
        {
            var type = EntityType.MovementCategory;
            var syncType = type.ToString().ToLowerInvariant();
            var url = $"/sync/{syncType}?ts={Uri.EscapeDataString(cursor.ITs.ToString("o"))}&seq={cursor.ISeq}&take={take}";
            using var res = await _http.GetAsync(url, ct);
            res.EnsureSuccessStatusCode();
            
            _logger.Log(nameof(MovementCategoryApi), $"Pull MovementCategory page - take {take} - {res.Content}" );

            var payload = await res.Content.ReadFromJsonAsync<SyncPage<MovementCategorySyncDTO>>(cancellationToken: ct);
            
            _logger.Log(nameof(MovementCategoryApi),
                $"Pull MovementCategory page - take {take} - payload items count: {payload?.Items.Count ?? 0} payload items[0] {(payload?.Items.Count > 0 ? payload?.Items[0] : "none")}");

            return payload == null
                ? Result<ISyncPage<ISyncDto>>.Failure("Remote pull for MovementCategory failed: server returned an empty payload.")
                : Result<ISyncPage<ISyncDto>>.SuccessResult(payload);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception e)
        {
            var message =
                $"Remote pull for MovementCategory failed while contacting {_http.DescribeBaseAddress()}: {e.GetBaseException().Message}";
            _logger.LogError(nameof(MovementCategoryApi), message);
            return Result<ISyncPage<ISyncDto>>.Failure(message);
        }
    }

    public async Task<Result<MovementCategoryGetDTO>> GetMovementCategoryAsync(MovementCategoryEntityId entity, CancellationToken ct)
    {
        var id = entity.Id;
        
        if(string.IsNullOrWhiteSpace(entity.Name) && (id == null || id == Guid.Empty))
        {
            return  Result<MovementCategoryGetDTO>.Failure("Did not create MovementCategory - ID and Name invalid");
        }
        
        
        if (!await _networkChecker.HasInternetAsync(_logger))
        {
            var message = $"Unable to reach sync server at {_http.DescribeBaseAddress()} - no internet connection detected.";
            _logger.LogWarning(nameof(MovementCategoryApi), message);
            return Result<MovementCategoryGetDTO>.Failure(message);
        }

        
        try
        {
            
                var url = $"movementcategories/{Uri.EscapeDataString(id!.Value.ToString())}";
                using var res =  await _http.GetAsync(url, ct);
                res.EnsureSuccessStatusCode();
                
                _logger.Log(nameof(MovementCategoryApi), $"Pull MovementCategory - id {id} - {res.Content}" );
                
            var payload = await res.Content.ReadFromJsonAsync<MovementCategoryGetDTO>(ct);
            return payload == null
                ? Result<MovementCategoryGetDTO>.Failure($"Remote pull for MovementCategory {id} failed: server returned an empty payload.")
                : Result<MovementCategoryGetDTO>.SuccessResult(payload);
        }
        catch (OperationCanceledException)
        {
            var message =
                $"Remote pull for MovementCategory failed was cancelled";
            _logger.LogError(nameof(MovementCategoryApi), message);
            return  Result<MovementCategoryGetDTO>.Failure(message);
        }
        catch (Exception e)
        {
            var message =
                $"Remote pull for MovementCategory failed while contacting {_http.DescribeBaseAddress()}: {e.GetBaseException().Message}";
            _logger.LogError(nameof(MovementCategoryApi), message);
            return  Result<MovementCategoryGetDTO>.Failure(message);
        }
    }

    public async Task<Result<MovementCategoryCreateOutcome>> CreateMovementCategoryAsync(MovementCategoryPostDTO entity, CancellationToken ct)
    {
        var id = entity.Id;
        if (id == Guid.Empty)
        {
            return  Result<MovementCategoryCreateOutcome>.Failure("Did not create MovementCategory - ID invalid");
        }
        
        if (string.IsNullOrWhiteSpace(entity.Name))
        {
            return  Result<MovementCategoryCreateOutcome>.Failure("Did not create MovementCategory - name empty");
        }
        
        if (!await _networkChecker.HasInternetAsync(_logger))
        {
            var message = $"Unable to reach sync server at {_http.DescribeBaseAddress()} - no internet connection detected.";
            _logger.LogWarning(nameof(MovementCategoryApi), message);
            return Result<MovementCategoryCreateOutcome>.Failure(message);
        }

        try
        {
            _logger.Log(nameof(MovementCategoryApi), $"Try Post MovementCategory - {entity.Print()}" );

            var url = $"/movementcategories/";
            using var res = await _http.PostAsync(url, JsonContent.Create(entity), ct);
            res.EnsureSuccessStatusCode();
            
            _logger.Log(nameof(MovementCategoryApi), $"Post MovementCategory - id {id} - {res.Content}" );
            
            var payload = await res.Content.ReadFromJsonAsync<MovementCategoryGetDTO>(cancellationToken:ct);
            if (payload == null)
            {
                return Result<MovementCategoryCreateOutcome>.Failure($"Did not create MovementCategory - id : {res.ReasonPhrase}");
            }

            return Result<MovementCategoryCreateOutcome>.SuccessResult(
                new MovementCategoryCreateOutcome(CreateOutcome.Created, payload));

        }
        
        catch (OperationCanceledException)
        {
            var message =
                $"Remote post for MovementCategory failed because operation vas cancelled";
            _logger.LogError(nameof(MovementCategoryApi), message);
            return  Result<MovementCategoryCreateOutcome>.Failure(message);
        }
        catch (Exception e)
        {
            var message =
                $"Remote post for MovementCategory failed while contacting {_http.DescribeBaseAddress()}: {e.GetBaseException().Message}";
            _logger.LogError(nameof(MovementCategoryApi), message);
            return  Result<MovementCategoryCreateOutcome>.Failure(message);
        }

    }

    public async Task<Result<MovementCategoryUpdateOutcome>> UpdateMovementCategoryAsync(MovementCategoryUpdateRequest request, CancellationToken ct)
    {
        if (request.UpdateRequest == UpdateRequest.DontUpdate)
        {
            return Result<MovementCategoryUpdateOutcome>.Failure("Did not update MovementCategory - Marked as DontUpdate");
        }

        var id = request.CorrelationId;
        
        if (id == Guid.Empty)
        {
            return Result<MovementCategoryUpdateOutcome>.Failure("Did not update MovementCategory - ID invalid");
        }
        
        if (request.MovementCategory == null)
        {
            return Result<MovementCategoryUpdateOutcome>.Failure("Did not update MovementCategory - payload missing");
        }
        
        if (!await _networkChecker.HasInternetAsync(_logger))
        {
            var message = $"Unable to reach sync server at {_http.DescribeBaseAddress()} - no internet connection detected.";
            _logger.LogWarning(nameof(MovementCategoryApi), message);
            return Result<MovementCategoryUpdateOutcome>.Failure(message);
        }

        try
        {
            _logger.Log(nameof(MovementCategoryApi),
                $"Try Patch MovementCategory - id {id} - {request.MovementCategory?.Name ?? "no-name"} - {(request.MovementCategory!.BaseCategories == null? "no base":request.MovementCategory!.BaseCategories .Count)}");

            var url = $"/movementcategories/{Uri.EscapeDataString(id.ToString()!)}";
            using var res = await _http.PatchAsync(url, JsonContent.Create(request.MovementCategory), ct);
            res.EnsureSuccessStatusCode();
            
            _logger.Log(nameof(MovementCategoryApi), $"Patch MovementCategory - id {id} - {res.Content}" );

            var payload = await res.Content.ReadFromJsonAsync<MovementCategoryUpdateOutcome>(cancellationToken: ct);
            
            if (payload == null)
            {
                return Result<MovementCategoryUpdateOutcome>.Failure(res.ReasonPhrase ?? "Unknown error");
            }

            return Result<MovementCategoryUpdateOutcome>.SuccessResult(payload);
        }
        
        
        catch (OperationCanceledException)
        {
            var message =
                $"Remote patch for MovementCategory failed because operation vas cancelled";
            _logger.LogError(nameof(MovementCategoryApi), message);
            return Result<MovementCategoryUpdateOutcome>.Failure(message);
        }
        
        catch (Exception e)
        {
            var message =
                $"Remote patch for MovementCategory failed while contacting {_http.DescribeBaseAddress()}: {e.GetBaseException().Message}";
            _logger.LogError(nameof(MovementCategoryApi), message);
            return Result<MovementCategoryUpdateOutcome>.Failure(message);
        }
    }

    public async Task<Result<MovementCategoryDeleteOutcome>> DeleteMovementCategoryAsync(MovementCategoryEntityId entity, CancellationToken ct)
    {
        if (!entity.IsValid())
        {
            return Result<MovementCategoryDeleteOutcome>.Failure("Invalid entity ID");
        }

        if (!entity.IsIdValid())
        {
            return Result<MovementCategoryDeleteOutcome>.Failure("Invalid entity ID");
        }
        
        if (!await _networkChecker.HasInternetAsync(_logger))
        {
            var message = $"Unable to reach sync server at {_http.DescribeBaseAddress()} - no internet connection detected.";
            _logger.LogWarning(nameof(MovementCategoryApi), message);
            return Result<MovementCategoryDeleteOutcome>.Failure(message);
        }

        try
        {
            var id= entity.Id!.Value;
            _logger.Log(nameof(MovementCategoryApi), $"Try Delete MovementCategory - id {id}" );

            
            var url = $"/movementcategories/{Uri.EscapeDataString(id.ToString()!)}";
            using var res = await _http.DeleteAsync(url, ct);
            res.EnsureSuccessStatusCode();
            
            
            _logger.Log(nameof(MovementCategoryApi), $"Delete MovementCategory - id {id} - {res.Content}" );

            
            return Result<MovementCategoryDeleteOutcome>.SuccessResult(new MovementCategoryDeleteOutcome(new MovementCategoryEntityId(id),DeleteOutcome.Deleted));
        }
        
        
        catch (OperationCanceledException)
        {
            var message =
                $"Remote Delete for MovementCategory failed because operation vas cancelled";
            _logger.LogError(nameof(MovementCategoryApi), message);
            return Result<MovementCategoryDeleteOutcome>.Failure(message);
        }
        
        catch (Exception e)
        {
            var message =
                $"Remote Delete for MovementCategory failed while contacting {_http.DescribeBaseAddress()}: {e.GetBaseException().Message}";
            _logger.LogError(nameof(MovementCategoryApi), message);
            return Result<MovementCategoryDeleteOutcome>.Failure(message);
        }
    }
}
