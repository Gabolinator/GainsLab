using GainsLab.Contracts.SyncDto;
using GainsLab.Contracts.SyncService;
using GainsLab.Core.Models.Core;
using GainsLab.Core.Models.Core.Interfaces.DataManagement;
using GainsLab.Core.Models.Core.Interfaces.DB;
using GainsLab.Core.Models.Core.Results;
using GainsLab.Models.Utilities;

namespace GainsLab.Contracts;

/// <summary>
/// Simple HTTP-based <see cref="IRemoteProvider"/> that hits the sync API.
/// </summary>
public class HttpDataProvider: IRemoteProvider
{
    
    private readonly HttpClient _http;
    private readonly Core.Models.Core.Utilities.Logging.ILogger _logger;
    /// <summary>
    /// Creates a provider that uses the supplied <see cref="HttpClient"/> (preconfigured via DI).
    /// </summary>
    /// <param name="http">The HTTP client configured with the sync API base address.</param>
    /// <param name="logger">Logger used to capture diagnostic information.</param>
    public HttpDataProvider(HttpClient http, Core.Models.Core.Utilities.Logging.ILogger logger)
    {
        _http = http;
        _logger = logger;
    }

    
    /// <inheritdoc />
    public Task<Result> InitializeAsync() => Task.FromResult(Result.SuccessResult());

    /// <inheritdoc />
    public async Task<Result<ISyncPage<ISyncDto>>> PullAsync(EntityType type, ISyncCursor cursor, int take = 200, CancellationToken ct = default)
    {
        _logger.Log(nameof(HttpDataProvider), $"Pull Entity Of Type : {type} from {_http.BaseAddress}");


        if (!await NetworkChecker.HasInternetAsync(_logger))
        {
            var message = $"Unable to reach sync server at {DescribeBaseAddress()} - no internet connection detected.";
            _logger.LogWarning(nameof(HttpDataProvider), message);
            return Result<ISyncPage<ISyncDto>>.Failure(message);
        }

        switch(type) 
        {
            case EntityType.Descriptor:
                return await PullDescriptorPageAsync(cursor, take, ct);
               
            case EntityType.Equipment:
                return await PullEquipmentPageAsync(cursor, take, ct);
               
            case EntityType.Muscle:
                return await PullMusclePageAsync(cursor, take, ct);
            default:
                return Result<ISyncPage<ISyncDto>>.Failure($"Remote pull for {type} is not implemented.");
        }

    }

    /// <summary>
    /// Invokes the descriptor sync endpoint and materializes a page of DTOs.
    /// </summary>
    /// <param name="cursor">Cursor describing where to resume the descriptor stream.</param>
    /// <param name="take">Maximum number of records to request.</param>
    /// <param name="ct">Cancellation token propagated from the caller.</param>
    private async Task<Result<ISyncPage<ISyncDto>>> PullDescriptorPageAsync(ISyncCursor cursor, int take, CancellationToken ct)
    {
        try
        {
            var url = $"/sync/descriptor?ts={Uri.EscapeDataString(cursor.ITs.ToString("o"))}&seq={cursor.ISeq}&take={take}";
            using var res = await _http.GetAsync(url, ct);
            res.EnsureSuccessStatusCode();

            _logger.Log(nameof(HttpDataProvider), $"Pull Descriptor page - take {take} - {res.Content}" );
        
            var payload = await res.Content.ReadFromJsonAsync<SyncPage<DescriptorSyncDTO>>(cancellationToken: ct);
        
            _logger.Log(nameof(HttpDataProvider), $"Pull Descriptor page - take {take} - payload items count: {payload?.Items.Count ?? 0} payload items[0] {(payload?.Items.Count>0 ?payload?.Items[0] : "none" )} " );

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
                $"Remote pull for Descriptor failed while contacting {DescribeBaseAddress()}: {e.GetBaseException().Message}";
            _logger.LogError(nameof(HttpDataProvider), message);
            return Result<ISyncPage<ISyncDto>>.Failure(message);
        }
      
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
        try
        {
            var url = $"/sync/equipment?ts={Uri.EscapeDataString(cursor.ITs.ToString("o"))}&seq={cursor.ISeq}&take={take}";
            using var res = await _http.GetAsync(url, ct);
            res.EnsureSuccessStatusCode();
        
            _logger.Log(nameof(HttpDataProvider), $"Pull Equipment page - take {take} - {res.Content}" );

            var payload = await res.Content.ReadFromJsonAsync<SyncPage<EquipmentSyncDTO>>(cancellationToken: ct);
        
            _logger.Log(nameof(HttpDataProvider), $"Pull Equipment page - take {take} - payload items count: {payload?.Items.Count ?? 0} payload items[0] {(payload?.Items.Count>0 ?payload?.Items[0] : "none" )} " );
            
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
                $"Remote pull for Equipment failed while contacting {DescribeBaseAddress()}: {e.GetBaseException().Message}";
            _logger.LogError(nameof(HttpDataProvider), message);
            return Result<ISyncPage<ISyncDto>>.Failure(message);
        }
    }

    public async Task<Result<ISyncPage<ISyncDto>>> PullMusclePageAsync(
        ISyncCursor cursor, int take = 200, CancellationToken ct = default)
    {
        try
        {
            var url = $"/sync/muscle?ts={Uri.EscapeDataString(cursor.ITs.ToString("o"))}&seq={cursor.ISeq}&take={take}";
            using var res = await _http.GetAsync(url, ct);
            res.EnsureSuccessStatusCode();
            var payload = await res.Content.ReadFromJsonAsync<SyncPage<MuscleSyncDTO>>(cancellationToken: ct);

            return payload == null
                ? Result<ISyncPage<ISyncDto>>.Failure("Remote pull for Muscle failed: server returned an empty payload.")
                : Result<ISyncPage<ISyncDto>>.SuccessResult(payload);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception e)
        {
            var message =
                $"Remote pull for Muscle failed while contacting {DescribeBaseAddress()}: {e.GetBaseException().Message}";
            _logger.LogError(nameof(HttpDataProvider), message);
            return Result<ISyncPage<ISyncDto>>.Failure(message);
        }
    }

    private string DescribeBaseAddress() => _http.BaseAddress?.ToString().TrimEnd('/') ?? "an unknown base address";
    
}
