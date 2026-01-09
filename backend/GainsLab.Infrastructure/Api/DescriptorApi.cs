using System.Net.Http.Json;
using GainsLab.Application.Results;
using GainsLab.Contracts;
using GainsLab.Contracts.Dtos.GetDto;
using GainsLab.Contracts.Dtos.PostDto;
using GainsLab.Contracts.Dtos.SyncDto;
using GainsLab.Contracts.Dtos.UpdateDto;
using GainsLab.Contracts.Dtos.UpdateDto.Outcome;
using GainsLab.Contracts.Interface;
using GainsLab.Domain;
using GainsLab.Domain.Interfaces;
using GainsLab.Infrastructure.Api.Interface;
using GainsLab.Infrastructure.SyncService;
using GainsLab.Infrastructure.Utilities;
using GainsLab.Models.Utilities;

namespace GainsLab.Infrastructure.Api;

public class DescriptorApi : IDescriptorApi
{
    private readonly HttpClient _http;
    private readonly ILogger _logger;

    public DescriptorApi(HttpClient http, ILogger logger)
    {
        _http = http;
        _logger = logger;
    }
    
    
    /// <summary>
    /// Invokes the descriptor sync endpoint and materializes a page of DTOs.
    /// </summary>
    /// <param name="cursor">Cursor describing where to resume the descriptor stream.</param>
    /// <param name="take">Maximum number of records to request.</param>
    /// <param name="ct">Cancellation token propagated from the caller.</param>
    public async Task<Result<ISyncPage<ISyncDto>>> PullDescriptorPageAsync(ISyncCursor cursor, int take, CancellationToken ct)
    {
        if (!await NetworkChecker.HasInternetAsync(_logger))
        {
            var message = $"Unable to reach sync server at {_http.DescribeBaseAddress()} - no internet connection detected.";
            _logger.LogWarning(nameof(DescriptorApi), message);
            return Result<ISyncPage<ISyncDto>>.Failure(message);
        }
        
        
        try
        {
            var type = EntityType.Descriptor;
            var syncType = type.ToString().ToLowerInvariant();
            
            var url = $"/sync/{syncType}?ts={Uri.EscapeDataString(cursor.ITs.ToString("o"))}&seq={cursor.ISeq}&take={take}";
            using var res = await _http.GetAsync(url, ct);
            res.EnsureSuccessStatusCode();

            _logger.Log(nameof(DescriptorApi), $"Pull Descriptor page - take {take} - {res.Content}" );
        
            var payload = await res.Content.ReadFromJsonAsync<SyncPage<DescriptorSyncDTO>>(cancellationToken: ct);
        
            _logger.Log(nameof(DescriptorApi), $"Pull Descriptor page - take {take} - payload items count: {payload?.Items.Count ?? 0} payload items[0] {(payload?.Items.Count>0 ?payload?.Items[0] : "none" )} " );

            return payload == null
                ? Result<ISyncPage<ISyncDto>>.Failure("Remote pull for Descriptor failed: server returned an empty payload.")
                : Result<ISyncPage<ISyncDto>>.SuccessResult(payload);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception e)
        {
            var message =
                $"Remote pull for Descriptor failed while contacting {_http.DescribeBaseAddress()}: {e.GetBaseException().Message}";
            _logger.LogError(nameof(DescriptorApi), message);
            return Result<ISyncPage<ISyncDto>>.Failure(message);
        }
      
    }

    public Task<Result<DescriptorGetDTO>> GetDescriptorAsync(DescriptorGetDTO entity, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public Task<Result<DescriptorPostDTO>> CreateDescriptorAsync(DescriptorPostDTO entity, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public async Task<Result<DescriptorUpdateOutcome>> UpdateDescriptorAsync(DescriptorUpdateRequest request, CancellationToken ct)
    {
        if (request.UpdateRequest == UpdateRequest.DontUpdate)
        {
            return  Result<DescriptorUpdateOutcome>.Failure("Did not update descriptor - Marked as DontUpdate");
        }

        var id = request.CorrelationId;
        
        if (id == Guid.Empty)
        {
            return  Result<DescriptorUpdateOutcome>.Failure("Did not update descriptor - ID invalid");
        }
        
        if (!await NetworkChecker.HasInternetAsync(_logger))
        {
            var message = $"Unable to reach sync server at {_http.DescribeBaseAddress()} - no internet connection detected.";
            _logger.LogWarning(nameof(DescriptorApi), message);
            return Result<DescriptorUpdateOutcome>.Failure(message);
        }

        
        
        try
        {
            _logger.Log(nameof(DescriptorApi), $"Try Patch Descriptor - id {id} - {request.Descriptor.DescriptionContent}" );

            
            var url = $"/descriptions/{Uri.EscapeDataString(id.ToString()!)}";
            using var res = await _http.PatchAsync(url,JsonContent.Create(request.Descriptor) ,ct);
            res.EnsureSuccessStatusCode();
        
            _logger.Log(nameof(DescriptorApi), $"Patch Descriptor - id {id} - {res.Content}" );

            var payload = await res.Content.ReadFromJsonAsync<DescriptorUpdateOutcome>(cancellationToken: ct);
            
            if (payload == null)
            {
                return  Result<DescriptorUpdateOutcome>.Failure(res.ReasonPhrase!);
            }

            return Result<DescriptorUpdateOutcome>.SuccessResult(payload);
            
        }
        catch (OperationCanceledException)
        {
            var message =
                $"Remote patch for Descriptor failed because operation vas cancelled";
            _logger.LogError(nameof(DescriptorApi), message);
            return  Result<DescriptorUpdateOutcome>.Failure(message);
        }
        catch (Exception e)
        {
            var message =
                $"Remote patch for Descriptor failed while contacting {_http.DescribeBaseAddress()}: {e.GetBaseException().Message}";
            _logger.LogError(nameof(DescriptorApi), message);
            return  Result<DescriptorUpdateOutcome>.Failure(message);
        }
        
    }
}