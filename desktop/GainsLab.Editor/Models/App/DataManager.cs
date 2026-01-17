using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using GainsLab.Application.Interfaces;
using GainsLab.Application.Interfaces.DataManagement;
using GainsLab.Application.Interfaces.Sync;
using GainsLab.Application.Results;
using GainsLab.Domain;
using GainsLab.Domain.Entities.WorkoutEntity;
using GainsLab.Domain.Interfaces;
using GainsLab.Domain.Interfaces.Caching;
using GainsLab.Domain.Interfaces.Entity;
using GainsLab.Infrastructure;
using GainsLab.Infrastructure.DB.Handlers;
using GainsLab.Infrastructure.Sync;
using GainsLab.Infrastructure.Utilities;
using GainsLab.Models.App.LifeCycle;
using ILogger = GainsLab.Domain.Interfaces.ILogger;


namespace GainsLab.Models.App;

/// <summary>
/// Coordinates pulling data from remote/local sources and caching it for the desktop app.
/// </summary>
public class DataManager : IDataManager
{

    private readonly ILogger _logger;
    
    //bridge to remote database
    private readonly IRemoteProvider _remote;
    
    //bridge to local database
    private readonly ILocalRepository _local;
    
    
    //access to component caches 
    private readonly IComponentCacheRegistry _cache;
    
    //read and write data to files
    private readonly IFileDataService _fileDataService;
    private readonly ISyncOrchestrator _syncOrchestrator;
    
    private readonly IEntitySeedResolver _resolver;
    private readonly INetworkChecker _networkChecker;

    private string fileDirectory;
    private readonly IAppLifeCycle _lifeCycle;
    private Task<bool>? _seedTask;


    /// <summary>
    /// Creates a new <see cref="DataManager"/> wired up with the required infrastructure services.
    /// </summary>
    /// <param name="lifeCycle">Lifecycle hook used to persist data when the app exits.</param>
    /// <param name="logger">Logger used for diagnostics.</param>
    /// <param name="remoteProvider">Abstraction over the remote sync API.</param>
    /// <param name="localProvider">Local repository that persists synced entities.</param>
    /// <param name="cache">Registry that exposes component caches to the desktop application.</param>
    /// <param name="fileDataService">File service responsible for serializing components to disk.</param>
    /// <param name="syncOrchestrator">Coordinator that performs the actual seed/delta synchronization.</param>
    /// <param name="resolver"></param>
    /// <param name="networkChecker"></param>
    public DataManager(
        IAppLifeCycle lifeCycle,
        ILogger logger,
        IRemoteProvider remoteProvider,
        ILocalRepository localProvider,
        IComponentCacheRegistry cache,
        IFileDataService fileDataService,
        ISyncOrchestrator syncOrchestrator,
        IEntitySeedResolver resolver,
        INetworkChecker networkChecker)
    {
        _logger = logger;
        _remote = remoteProvider;
        _local = localProvider;
        _cache = cache;
        _fileDataService = fileDataService;
        _lifeCycle = lifeCycle;
        _syncOrchestrator = syncOrchestrator;
        _resolver = resolver;
        _networkChecker = networkChecker;
    }

    /// <summary>
    /// Prepares storage folders, hooks lifecycle events, and runs an initial synchronization.
    /// </summary>
    public async Task InitializeAsync()
    {
        _logger.Log(nameof(DataManager), "Initializing...");
        //get the local direct
        var basePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "GainsLab", "Files");
            
        //ensure path exist 
        if (!Path.Exists(basePath))
        {
            _logger.LogWarning(nameof(DataManager),$" BaseFolder at path: {basePath} - Doesnt exist- creating it");
            Directory.CreateDirectory(basePath);
        }
        
        fileDirectory = basePath;

        _lifeCycle.onAppExitAsync +=SaveAllDataToFilesAsync;

        await _local.InitializeAsync();
        
       if (!await HasInternetConnection())
       {
           _logger.LogWarning(nameof(DataManager),"No internet connection");
           return;
       } 
       
       
        
       //kick off a full seed the first time we spin up so caches stay in sync
        _seedTask ??= DoInitialSeed();

       
        bool didInitialSeed =await _seedTask;
        
        if (!didInitialSeed)
        {
            await CheckForUpdatesFromUpstreamAsync();
        }

        
    }

    private Task<bool> HasInternetConnection() => _networkChecker.HasInternetAsync(_logger);
  

    /// <summary>
    /// Checks the remote source for new updates and schedules a delta sync when required.
    /// </summary>
    private async Task CheckForUpdatesFromUpstreamAsync()
    {
        _logger.Log(nameof(DataManager), "Checking for upstream updates...");

        if (!await HasInternetConnection())
        {
            _logger.LogWarning(nameof(DataManager),
                "Skipping delta sync because there is no internet connection.");
            return;
        }

        var state = await LoadOrCreateSyncStateAsync();
        if (!state.SeedCompleted)
        {
            _logger.LogWarning(nameof(DataManager),
                "Skipping delta sync because the initial seed has not completed.");
            return;
        }

        var cursors = ParseCursorMap(state.CursorsJson);

        var deltaResult = await _syncOrchestrator.PullDeltasAsync(cursors);
        if (!deltaResult.Success || deltaResult.Value is null)
        {
            var error = deltaResult.GetErrorMessage() ?? "Unknown delta sync failure.";
            _logger.LogError(nameof(DataManager), $"Delta sync failed: {error}");
            return;
        }

        var outcome = deltaResult.Value;
        var updatedCursors = outcome.Cursors ??
                             (IReadOnlyDictionary<string, string>)new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        state.LastDeltaAt = DateTimeOffset.UtcNow;
        state.UpstreamSnapshot = string.IsNullOrWhiteSpace(outcome.SnapshotVersion)
            ? state.UpstreamSnapshot
            : outcome.SnapshotVersion;
        state.CursorsJson = JsonSerializer.Serialize(updatedCursors);
        await _local.SaveSyncStateAsync(state);

        _logger.Log(nameof(DataManager),
            $"Delta sync completed. Upserted {outcome.EntitiesUpserted}, deleted {outcome.EntitiesDeleted}, hadMore={outcome.HadMore}.");

        var refreshResult = await _local.GetAllComponentsAsync();
        if (refreshResult.Success && refreshResult.Value is not null)
        {
            _cache.StoreAll(refreshResult.Value);
        }
        else
        {
            _logger.LogWarning(nameof(DataManager),
                $"Delta sync finished but cache refresh failed: {refreshResult.GetErrorMessage()}");
        }
    }

    private Dictionary<string, string> ParseCursorMap(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        try
        {
            var parsed = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
            return parsed ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(nameof(DataManager),
                $"Could not parse stored cursor payload. Resetting cursors. {ex.Message}");
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }
    }

    /// <summary>
    /// Performs the initial seed by delegating to the sync orchestrator and persisting the resulting cursors.
    /// </summary>
    /// <returns><c>true</c> when a seed ran to completion; <c>false</c> when an existing seed was reused or failed.</returns>
    private async Task<bool> DoInitialSeed()
    {
        _logger.Log(nameof(DataManager), "Seed Initial data...");

      

        var state = await LoadOrCreateSyncStateAsync();
        if (state.SeedCompleted)
        {
            _logger.Log(nameof(DataManager), "Initial sync already done");
            return false;
        }

        if (!state.SeedInProgress)
        {
            state.SeedInProgress = true;
            await _local.SaveSyncStateAsync(state);
        }

        var seed = await _syncOrchestrator.SeedAsync();
        if (!seed.Success)
        {
            _logger.LogError(nameof(DataManager), $"Initial sync failed: {seed.GetErrorMessage()}");
            state.SeedInProgress = false; // allow retry
            await _local.SaveSyncStateAsync(state);
            return false;
        }

        // persist snapshot + cursors from the payload
        state.SeedCompleted    = true;
        state.SeedInProgress   = false;
        state.LastSeedAt       = DateTimeOffset.UtcNow;
        state.UpstreamSnapshot = seed.Value?.SnapshotVersion;
        state.CursorsJson      = System.Text.Json.JsonSerializer.Serialize(seed.Value?.Cursors);
        await _local.SaveSyncStateAsync(state);
        return true;
    }

    /// <summary>
    /// Retrieves the persisted sync state for the global partition, creating one on first run.
    /// </summary>
    private async Task<SyncState> LoadOrCreateSyncStateAsync()
    {
        var state = await _local.GetSyncStateAsync("global"); 
       
        if (state is null)
        {
            state = new SyncState { Partition = "global" };
            await _local.SaveSyncStateAsync(state);
        }
        return (SyncState)state;
    }


    /// <summary>
    /// To test seeding remote db
    /// </summary>
    /// <returns></returns>
    public async Task<Result> CreateLocalDataAsync()
    {
        var clock = CoreUtilities.Clock;
        
        //create a test equipment and send it to the remote
        var descriptionService = new BaseDescriptorService(clock);
        var entityFactory = new Application.EntityFactory.EntityFactory(clock,_logger, descriptionService, _resolver);
        
        var dumbbell = entityFactory.CreateEquipment("Dumbbell", "some description for dumbbell 2 - should be added", "editor test");

        var list = new List<EquipmentEntity> {dumbbell};
        
        
        //save to local db and sync up
        return  await SaveComponentsAsync(list, true);
        
    }

    /// <summary>
    /// Loads component data from disk, persists it locally, waits for any pending seed, and primes in-memory caches.
    /// </summary>
    /// <returns>A <see cref="Result"/> conveying success or failure of the overall operation.</returns>
    public async Task<Result> LoadAndCacheDataAsync()
    {

        _logger.Log(nameof(DataManager), "Loading and caching data...");
        
        //Load from files
        //todo - not implemented
        Dictionary<EntityType, IReadOnlyList<IEntity>> fileData = await _fileDataService.LoadAllComponentsAsync();
        
        
        //batch insert all new loaded data in database
        //todo - not implemented
        var result =  await _local.BatchSaveComponentsAsync(fileData);

        var batchSaveSuccess = result.Success;
        if (!batchSaveSuccess)
        {
            _logger.LogWarning(nameof(DataManager), $"Could not Save loaded data to DB.{result.GetErrorMessage()}");
        }


        // If a seed is in-flight (or needed), wait for it here.
        if (_seedTask is not null)
        {
            try { await _seedTask; } catch { /* seed failure already logged; continue */ }
        }


        //Load from DB to cache
        //todo - not implemented
        var dataFromDB = await _local.GetAllComponentsAsync();
        
        var fromDBSuccess = dataFromDB.Success;
        if (!fromDBSuccess || dataFromDB.Value == null)
        {
            _logger.LogWarning(nameof(DataManager), $"Could not Load all component data from DB. { dataFromDB.GetErrorMessage()}");
            
        }

        //cache all
        else  _cache.StoreAll(dataFromDB.Value);


        bool allFailed = !fromDBSuccess && !batchSaveSuccess;
        _logger.Log(nameof(DataManager), "Finished loading and caching data.");
        return !allFailed ? Result.SuccessResult() : Result.Failure("Loading and Retreiving datafrom database failed");

    }
    
    /// <summary>
    /// Persists a single component locally, primes the cache, and optionally dispatches the outbox upstream.
    /// </summary>
    /// <param name="component">The component to store in the local repository.</param>
    /// <param name="syncUp">When true, triggers an outbox dispatch after the local save succeeds.</param>
    public async Task<Result> SaveComponentAsync<TEntity>(TEntity component, bool syncUp = false) where TEntity : IEntity
    {
        if (component is null) throw new ArgumentNullException(nameof(component));

        var saveResult = await _local.SaveComponentAsync(component);
        if (!saveResult.Success || saveResult.Value is null)
        {
            var message = saveResult.GetErrorMessage() ?? "Failed to save component locally.";
            _logger.LogError(nameof(DataManager), message);
            return Result.Failure(message);
        }

        _cache.Store(saveResult.Value);

        if (!syncUp)
            return Result.SuccessResult();

        var pushResult = await _syncOrchestrator.SyncUpAsync();
        if (!pushResult.Success)
        {
            var message = pushResult.GetErrorMessage() ?? "Failed to push changes upstream.";
            _logger.LogError(nameof(DataManager), message);
            return Result.Failure(message);
        }

        return Result.SuccessResult();
    }

    /// <summary>
    /// Persists a batch of components, updates caches, and optionally dispatches the outbox upstream.
    /// </summary>
    /// <param name="components">The components to persist grouped by their entity type.</param>
    /// <param name="syncUp">When true, dispatches pending outbox items after the batch completes.</param>
    public async Task<Result<Dictionary<EntityType, IReadOnlyList<IEntity>>>> SaveComponentsAsync<TEntity>(IEnumerable<TEntity> components, bool syncUp = false) where TEntity : IEntity
    {
        if (components is null) throw new ArgumentNullException(nameof(components));

        var grouped = components
            .GroupBy(e => e.Type)
            .ToDictionary(
                g => g.Key,
                g => (IReadOnlyList<IEntity>)g.Cast<IEntity>().ToList());

        if (grouped.Count == 0)
            return Result<Dictionary<EntityType, IReadOnlyList<IEntity>>>.Failure("No components to save.");

        var saveResult = await _local.BatchSaveComponentsAsync(grouped);
        if (!saveResult.Success || saveResult.Value is null)
        {
            var message = saveResult.GetErrorMessage() ?? "Failed to persist component batch.";
            _logger.LogError(nameof(DataManager), message);
            return Result<Dictionary<EntityType, IReadOnlyList<IEntity>>>.Failure(message);
        }

        _cache.StoreAll(saveResult.Value);

        if (!syncUp)
            return saveResult;

        var pushResult = await _syncOrchestrator.SyncUpAsync();
        if (!pushResult.Success)
        {
            var message = pushResult.GetErrorMessage() ?? "Failed to push batched changes upstream.";
            _logger.LogError(nameof(DataManager), message);
            return Result<Dictionary<EntityType, IReadOnlyList<IEntity>>>.Failure(message);
        }

        return saveResult;
    }
 
    /// <summary>
    /// Removes a component from the local store and cache surfaces. Currently returns failure until delete semantics are defined.
    /// </summary>
    public Task<Result> DeleteComponentAsync<TEntity>(TEntity entity)
    {
        _logger.LogWarning(nameof(DataManager),
            $"Delete requested for {typeof(TEntity).Name}, but delete semantics are not implemented.");
        return Task.FromResult(Result.Failure("Delete operations are not implemented."));
    }

    /// <summary>
    /// Writes the current component set out to files on disk (e.g., for offline usage or backups).
    /// </summary>
    public async Task<Result> SaveAllDataToFilesAsync()
    {
        if (string.IsNullOrWhiteSpace(fileDirectory))
            return Result.Failure("Data manager has not been initialized.");

        var dataResult = await _local.GetAllComponentsAsync();
        if (!dataResult.Success || dataResult.Value is null || dataResult.Value.Count == 0)
        {
            var message = dataResult.GetErrorMessage() ?? "No data available to persist.";
            _logger.LogWarning(nameof(DataManager), message);
            return Result.Failure(message);
        }

        var mutable = dataResult.Value.ToDictionary(
            kvp => kvp.Key,
            kvp => kvp.Value.ToList());

        var writeResult = await _fileDataService.WriteAllComponentsAsync(mutable, fileDirectory, ".json");
        if (!writeResult.Success)
        {
            var message = writeResult.GetErrorMessage() ?? "Failed to write data to disk.";
            _logger.LogError(nameof(DataManager), message);
            return Result.Failure(message);
        }

        return Result.SuccessResult();
    }

   
}
