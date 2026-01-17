using System.Net.Http.Json;
using GainsLab.Application.Interfaces;
using GainsLab.Application.Interfaces.DataManagement.Provider;
using GainsLab.Application.Interfaces.Sync;
using GainsLab.Application.Results;
using GainsLab.Contracts.Dtos.Delete.Outcome;
using GainsLab.Contracts.Dtos.GetDto;
using GainsLab.Contracts.Dtos.ID;
using GainsLab.Contracts.Dtos.PostDto;
using GainsLab.Contracts.Dtos.PostDto.Outcome;
using GainsLab.Contracts.Dtos.SyncDto;
using GainsLab.Contracts.Dtos.UpdateDto;
using GainsLab.Contracts.Dtos.UpdateDto.Outcome;
using GainsLab.Contracts.Dtos.UpdateDto.Request;
using GainsLab.Contracts.Interface;
using GainsLab.Contracts.SyncService;
using GainsLab.Domain;
using GainsLab.Domain.Interfaces;
using GainsLab.Infrastructure.Api;
using GainsLab.Infrastructure.Api.Interface;
using GainsLab.Infrastructure.SyncService;
using GainsLab.Infrastructure.Utilities;
using IDescriptorApi = GainsLab.Infrastructure.Api.Interface.IDescriptorApi;

namespace GainsLab.Infrastructure;

/// <summary>
/// Simple HTTP-based <see cref="IRemoteProvider"/> that hits the sync API.
/// </summary>
public class HttpDataProvider: 
    IRemoteProvider, 
    IEquipmentProvider, 
    IDescriptorProvider,
    IMuscleProvider,
    IMovementCategoryProvider
{
    
    private readonly HttpClient _http;
    private readonly ILogger _logger;
    private readonly INetworkChecker _networkChecker;
    private readonly IApiClientRegistry _apiClient;
    private IEquipmentApi Equipments => _apiClient.EquipmentApi;
    private IDescriptorApi Descriptors =>  _apiClient.DescriptorApi;

    private IMuscleApi Muscles => _apiClient.MuscleApi;
    private IMovementCategoryApi MovementCategories => _apiClient.MovementCategoryApi;


    /// <summary>
    /// Creates a provider that uses the supplied <see cref="HttpClient"/> (preconfigured via DI).
    /// </summary>
    /// <param name="http">The HTTP client configured with the sync API base address.</param>
    /// <param name="apiClientRegistry"></param>
    /// <param name="logger">Logger used to capture diagnostic information.</param>
    /// <param name="networkChecker"></param>
    public HttpDataProvider(HttpClient http, IApiClientRegistry apiClientRegistry ,ILogger logger, INetworkChecker networkChecker)
    {
        _http = http;
        _logger = logger;
        _networkChecker = networkChecker;
        _apiClient = apiClientRegistry;
    }

    
    /// <inheritdoc />
    public Task<Result> InitializeAsync() => Task.FromResult(Result.SuccessResult());

    /// <inheritdoc />
    public async Task<Result<ISyncPage<ISyncDto>>> PullAsync(EntityType type, ISyncCursor cursor, int take = 200, CancellationToken ct = default)
    {
        _logger.Log(nameof(HttpDataProvider), $"Pull Entity Of Type : {type} from {_http.BaseAddress}");


        if (!await _networkChecker.HasInternetAsync(_logger))
        {
            var message = $"Unable to reach sync server at {_http.DescribeBaseAddress()} - no internet connection detected.";
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
            
            case EntityType.MovementCategory:
                return await PullMovementCategoryPageAsync(cursor, take, ct);
            default:
                return Result<ISyncPage<ISyncDto>>.Failure($"Remote pull for {type} is not implemented.");
        }

    }

  

    #region MovementCategory

    public async Task<Result<ISyncPage<ISyncDto>>> PullMovementCategoryPageAsync(ISyncCursor cursor, int take, CancellationToken ct)
    {
        try
        {
            var type = EntityType.MovementCategory;
            var syncType = type.ToString().ToLowerInvariant();
            
            var url = $"/sync/{syncType}?ts={Uri.EscapeDataString(cursor.ITs.ToString("o"))}&seq={cursor.ISeq}&take={take}";
            _logger.Log(nameof(HttpDataProvider), $"Trying to access {url}");
            
            using var res = await _http.GetAsync(url, ct);
            res.EnsureSuccessStatusCode();

            _logger.Log(nameof(HttpDataProvider), $"Pull MovementCategory page - take {take} - {res.Content}" );
        
            var payload = await res.Content.ReadFromJsonAsync<SyncPage<MovementCategorySyncDTO>>(cancellationToken: ct);
        
            _logger.Log(nameof(HttpDataProvider), $"Pull MovementCategory page - take {take} - payload items count: {payload?.Items.Count ?? 0} payload items[0] {(payload?.Items.Count>0 ?payload?.Items[0] : "none" )} " );

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
            _logger.LogError(nameof(HttpDataProvider), message);
            return Result<ISyncPage<ISyncDto>>.Failure(message);
        }
    }
    
    public Task<Result<MovementCategoryGetDTO>> GetMovementCategoryAsync(MovementCategoryEntityId entity,
        CancellationToken ct)
        => MovementCategories.GetMovementCategoryAsync(entity, ct);

    public Task<Result<MovementCategoryCreateOutcome>> CreateMovementCategoryAsync(MovementCategoryPostDTO entity,
        CancellationToken ct)
        => MovementCategories.CreateMovementCategoryAsync(entity, ct);

    public Task<Result<MovementCategoryUpdateOutcome>> UpdateMovementCategoryAsync(
        MovementCategoryUpdateRequest request, CancellationToken ct)
        => MovementCategories.UpdateMovementCategoryAsync(request, ct);

    public Task<Result<MovementCategoryDeleteOutcome>> DeleteMovementCategoryAsync(MovementCategoryEntityId entity,
        CancellationToken ct)
        => MovementCategories.DeleteMovementCategoryAsync(entity, ct);


    #endregion
    

    
    #region Muscle
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
                $"Remote pull for Muscle failed while contacting {_http.DescribeBaseAddress()}: {e.GetBaseException().Message}";
            _logger.LogError(nameof(HttpDataProvider), message);
            return Result<ISyncPage<ISyncDto>>.Failure(message);
        }
    }
    
    public Task<Result<MuscleGetDTO>> GetMuscleAsync(MuscleEntityId entity, CancellationToken ct)
    => Muscles.GetMuscleAsync(entity, ct);

    public Task<Result<MuscleCreateOutcome>> CreateMuscleAsync(MusclePostDTO entity, CancellationToken ct)
        => Muscles.CreateMuscleAsync(entity, ct);

    public Task<Result<MuscleUpdateOutcome>> UpdateMuscleAsync(MuscleUpdateRequest request, CancellationToken ct)
   => Muscles.UpdateMuscleAsync(request, ct);

    public Task<Result<MuscleDeleteOutcome>> DeleteMuscleAsync(MuscleEntityId entity, CancellationToken ct)
    => Muscles.DeleteMuscleAsync(entity, ct);
    #endregion

    
    #region Descriptor

    public Task<Result<ISyncPage<ISyncDto>>> PullDescriptorPageAsync(ISyncCursor cursor, int take, CancellationToken ct)
        => Descriptors.PullDescriptorPageAsync(cursor, take, ct);

    public Task<Result<DescriptorGetDTO>> GetDescriptorAsync(DescriptorGetDTO entity, CancellationToken ct)
        => Descriptors.GetDescriptorAsync(entity, ct);

    public Task<Result<DescriptorCreateOutcome>> CreateDescriptorAsync(DescriptorPostDTO entity, CancellationToken ct)
        => Descriptors.CreateDescriptorAsync(entity, ct);

    public Task<Result<DescriptorUpdateOutcome>> UpdateDescriptorAsync(DescriptorUpdateRequest entity, CancellationToken ct)
        => Descriptors.UpdateDescriptorAsync(entity, ct);


    #endregion
    
    #region Equipments

    public Task<Result<ISyncPage<ISyncDto>>> PullEquipmentPageAsync(ISyncCursor cursor, int take, CancellationToken ct)
        => Equipments.PullEquipmentPageAsync(cursor, take, ct);

    public Task<Result<EquipmentGetDTO>> GetEquipmentAsync(EquipmentEntityId entity, CancellationToken ct)
        => Equipments.GetEquipmentAsync(entity, ct);
    
    public Task<Result<EquipmentCreateOutcome>> CreateEquipmentAsync(EquipmentPostDTO entity, CancellationToken ct)
        => Equipments.CreateEquipmentAsync(entity, ct);

    public Task<Result<EquipmentUpdateOutcome>> UpdateEquipmentAsync(EquipmentUpdateRequest updateRequest, CancellationToken ct)
        => Equipments.UpdateEquipmentAsync(updateRequest, ct);

    public Task<Result<EquipmentDeleteOutcome>> DeleteEquipmentAsync(EquipmentEntityId entity, CancellationToken ct)
        => Equipments.DeleteEquipmentAsync(entity, ct);
    
    #endregion
    
}
