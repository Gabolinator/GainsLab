
using GainsLab.Contracts.SyncDto;
using GainsLab.Contracts.SyncService;
using GainsLab.Core.Models.Core;
using GainsLab.Core.Models.Core.Interfaces.DataManagement;
using GainsLab.Core.Models.Core.Interfaces.DB;
using GainsLab.Core.Models.Core.Results;

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
        
        switch(type) 
        {
            case EntityType.Descriptor:
                var pageDescp = await PullDescriptorPageAsync(cursor, take, ct);
                return Result<ISyncPage<ISyncDto>>.SuccessResult(pageDescp);
            case EntityType.Equipment:
                var pageEquip = await PullEquipmentPageAsync(cursor, take, ct);
                return Result<ISyncPage<ISyncDto>>.SuccessResult(pageEquip);
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
    private async Task<ISyncPage<ISyncDto>> PullDescriptorPageAsync(ISyncCursor cursor, int take, CancellationToken ct)
    {
        var url = $"/sync/descriptor?ts={Uri.EscapeDataString(cursor.ITs.ToString("o"))}&seq={cursor.ISeq}&take={take}";
        using var res = await _http.GetAsync(url, ct);
        res.EnsureSuccessStatusCode();

        _logger.Log(nameof(HttpDataProvider), $"Pull Descriptor page - take {take} - {res.Content}" );
        
        var payload = await res.Content.ReadFromJsonAsync<SyncPage<DescriptorSyncDto>>(cancellationToken: ct);
        
        _logger.Log(nameof(HttpDataProvider), $"Pull Descriptor page - take {take} - payload items count: {payload?.Items.Count ?? 0} payload items[0] {(payload?.Items.Count>0 ?payload?.Items[0] : "none" )} " );

        
        
        return payload ?? new SyncPage<DescriptorSyncDto>(DateTimeOffset.UtcNow, null, Array.Empty<DescriptorSyncDto>());

    }


    /// <summary>
    /// Invokes the equipment sync endpoint and materializes a page of DTOs.
    /// </summary>
    /// <param name="cursor">Cursor describing where to resume the equipment stream.</param>
    /// <param name="take">Maximum number of records to request.</param>
    /// <param name="ct">Cancellation token propagated from the caller.</param>
    public async Task<SyncPage<EquipmentSyncDto>> PullEquipmentPageAsync(
        ISyncCursor cursor, int take = 200, CancellationToken ct = default)
    {
        var url = $"/sync/equipment?ts={Uri.EscapeDataString(cursor.ITs.ToString("o"))}&seq={cursor.ISeq}&take={take}";
        using var res = await _http.GetAsync(url, ct);
        res.EnsureSuccessStatusCode();
        
        _logger.Log(nameof(HttpDataProvider), $"Pull Equipment page - take {take} - {res.Content}" );

        var payload = await res.Content.ReadFromJsonAsync<SyncPage<EquipmentSyncDto>>(cancellationToken: ct);
        
        _logger.Log(nameof(HttpDataProvider), $"Pull Equipment page - take {take} - payload items count: {payload?.Items.Count ?? 0} payload items[0] {(payload?.Items.Count>0 ?payload?.Items[0] : "none" )} " );

        return payload ?? new SyncPage<EquipmentSyncDto>(DateTimeOffset.UtcNow, null, Array.Empty<EquipmentSyncDto>());
    }
    
}
