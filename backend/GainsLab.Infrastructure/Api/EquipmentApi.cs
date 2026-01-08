using System.Net.Http.Json;
using GainsLab.Application.Results;
using GainsLab.Contracts.Dtos.GetDto;
using GainsLab.Contracts.Dtos.PostDto;
using GainsLab.Contracts.Dtos.RequestDto;
using GainsLab.Contracts.Dtos.SyncDto;
using GainsLab.Contracts.Dtos.UpdateDto;
using GainsLab.Contracts.Dtos.UpdateDto.Outcome;
using GainsLab.Contracts.Interface;
using GainsLab.Domain.Interfaces;
using GainsLab.Infrastructure.Api.Interface;
using GainsLab.Infrastructure.SyncService;
using GainsLab.Infrastructure.Utilities;
using GainsLab.Models.Utilities;

namespace GainsLab.Infrastructure.Api;

public class EquipmentApi :IEquipmentApi
{
    private readonly HttpClient _http;
    private readonly ILogger _logger;

    public EquipmentApi(HttpClient http, ILogger logger)
    {
       _http = http;
       _logger = logger;
    }
    
    
    /// <summary>
    /// Invokes the equipment sync endpoint and materializes a page of DTOs.
    /// </summary>
    /// <param name="cursor">Cursor describing where to resume the equipment stream.</param>
    /// <param name="take">Maximum number of records to request.</param>
    /// <param name="ct">Cancellation token propagated from the caller.</param>
    public async Task<Result<ISyncPage<ISyncDto>>> PullEquipmentPageAsync(
        ISyncCursor cursor, int take = 200, CancellationToken ct = default)
    {
        
        if (!await NetworkChecker.HasInternetAsync(_logger))
        {
            var message = $"Unable to reach sync server at {_http.DescribeBaseAddress()} - no internet connection detected.";
            _logger.LogWarning(nameof(EquipmentApi), message);
            return Result<ISyncPage<ISyncDto>>.Failure(message);
        }
        
        try
        {
            var url = $"/sync/equipment?ts={Uri.EscapeDataString(cursor.ITs.ToString("o"))}&seq={cursor.ISeq}&take={take}";
            using var res = await _http.GetAsync(url, ct);
            res.EnsureSuccessStatusCode();
        
            _logger.Log(nameof(EquipmentApi), $"Pull Equipment page - take {take} - {res.Content}" );

            var payload = await res.Content.ReadFromJsonAsync<SyncPage<EquipmentSyncDTO>>(cancellationToken: ct);
        
            _logger.Log(nameof(EquipmentApi), $"Pull Equipment page - take {take} - payload items count: {payload?.Items.Count ?? 0} payload items[0] {(payload?.Items.Count>0 ?payload?.Items[0] : "none" )} " );
            
            return payload == null
                ? Result<ISyncPage<ISyncDto>>.Failure("Remote pull for Equipment failed: server returned an empty payload.")
                : Result<ISyncPage<ISyncDto>>.SuccessResult(payload);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception e)
        {
            var message =
                $"Remote pull for Equipment failed while contacting {_http.DescribeBaseAddress()}: {e.GetBaseException().Message}";
            _logger.LogError(nameof(EquipmentApi), message);
            return Result<ISyncPage<ISyncDto>>.Failure(message);
        }
    }

    public async Task<Result<EquipmentGetDTO>> GetEquipmentAsync(EquipmentRequestDTO requestDto, CancellationToken ct)
    {
        if (!await NetworkChecker.HasInternetAsync(_logger))
        {
            var message = $"Unable to reach sync server at {_http.DescribeBaseAddress()} - no internet connection detected.";
            _logger.LogWarning(nameof(EquipmentApi), message);
            return Result<EquipmentGetDTO>.Failure(message);
        }

        var id = requestDto.Id;
        
        if ( id != null && id != Guid.Empty)
        {
            try
            {
                var url = $"/equipments/{Uri.EscapeDataString(id.ToString()!)}";
                using var res = await _http.GetAsync(url, ct);
                res.EnsureSuccessStatusCode();
        
                _logger.Log(nameof(EquipmentApi), $"Pull Equipment - id {id} - {res.Content}" );

                var payload = await res.Content.ReadFromJsonAsync<EquipmentGetDTO>(cancellationToken: ct);
        
               
                return payload == null
                    ? Result<EquipmentGetDTO>.Failure($"Remote pull for Equipment {id} failed: server returned an empty payload.")
                    : Result<EquipmentGetDTO>.SuccessResult(payload);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception e)
            {
                var message =
                    $"Remote pull for Equipment failed while contacting {_http.DescribeBaseAddress()}: {e.GetBaseException().Message}";
                _logger.LogError(nameof(EquipmentApi), message);
                return  Result<EquipmentGetDTO>.Failure(message);
            }
        }

        //by name not yet supported 

        return Result<EquipmentGetDTO>.Failure("No Id or Name to retrieve the Equipment");
    }

    public Task<Result<EquipmentPostDTO>> CreateEquipmentAsync(EquipmentPostDTO entity, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public async Task<Result<EquipmentUpdateOutcome>> UpdateEquipmentAsync(EquipmentUpdateRequest request, CancellationToken ct)
    {
     return  Result<EquipmentUpdateOutcome>.NotImplemented(nameof(UpdateEquipmentAsync));
    }

    public Task<Result<EquipmentGetDTO>> DeleteEquipmentAsync(EquipmentRequestDTO entity, CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}