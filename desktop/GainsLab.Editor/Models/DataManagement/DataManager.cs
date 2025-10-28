using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GainsLab.Contracts.SyncDto;
using GainsLab.Core.Models.Core;
using GainsLab.Core.Models.Core.Interfaces.Caching;
using GainsLab.Core.Models.Core.Interfaces.DataManagement;
using GainsLab.Core.Models.Core.Interfaces.Entity;
using GainsLab.Core.Models.Core.Results;
using GainsLab.Core.Models.Core.Utilities.Logging;
using GainsLab.Infrastructure;
using GainsLab.Models.Core.LifeCycle;
using GainsLab.Models.DataManagement.Sync;
using GainsLab.Models.Utilities;

namespace GainsLab.Models.DataManagement;

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
    public DataManager(
        IAppLifeCycle lifeCycle,
        ILogger logger,
        IRemoteProvider remoteProvider,
        ILocalRepository localProvider,
        IComponentCacheRegistry cache,
        IFileDataService fileDataService,
        ISyncOrchestrator syncOrchestrator)
    {
        _logger = logger;
        _remote = remoteProvider;
        _local = localProvider;
        _cache = cache;
        _fileDataService = fileDataService;
        _lifeCycle = lifeCycle;
        _syncOrchestrator = syncOrchestrator;

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
            CheckForUpdatesFromUpstream();
        }

        
    }

    private Task<bool> HasInternetConnection() => NetworkChecker.HasInternetAsync(_logger);
  

    /// <summary>
    /// Checks the remote source for new updates and schedules a delta sync when required.
    /// </summary>
    private void CheckForUpdatesFromUpstream()
    {
        //todo check if any updates to pull
        _logger.Log(nameof(DataManager), "Check for upstream updates...NOT IMPLEMENTED");
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
    /// Loads component data from disk, persists it locally, waits for any pending seed, and primes in-memory caches.
    /// </summary>
    /// <returns>A <see cref="Result"/> conveying success or failure of the overall operation.</returns>
    public async Task<Result> LoadAndCacheDataAsync()
    {

        _logger.Log(nameof(DataManager), "Loading and caching data...");
        
        //Load from files
        //not implemented
        Dictionary<EntityType, IReadOnlyList<IEntity>> fileData = await _fileDataService.LoadAllComponentsAsync();
        
        
        //batch insert all new loaded data in database
        //not implemented
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
        //not implemented
        var dataFromDB = await _local.GetAllComponentsAsync();
        
        var fromDBSuccess = dataFromDB.Success;
        if (!fromDBSuccess || dataFromDB.Value == null)
        {
            _logger.LogWarning(nameof(DataManager), $"Could not Load all component data from DB. { dataFromDB.GetErrorMessage()}");
            
        }

        else CacheAllData(dataFromDB.Value);


        bool allFailed = !fromDBSuccess && !batchSaveSuccess;
        _logger.Log(nameof(DataManager), "Finished loading and caching data.");
        return !allFailed ? Result.SuccessResult() : Result.Failure("Loading and Retreiving datafrom database failed");

    }

    /// <summary>
    /// Attempts to resolve a single component by identifier using cache, local storage, and remote fallbacks.
    /// </summary>
    public Task<Result<TEntity>> TryGetEntityAsync<TId, TEntity>(TId id)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Attempts to resolve multiple components by identifier using cache, local storage, and remote fallbacks.
    /// </summary>
    public Task<ResultList<TEntity>> TryGetComponentsAsync<TId, TEntity>(IEnumerable<TId> ids)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Persists a single component to the local data store and updates caches accordingly.
    /// </summary>
    public Task<Result> SaveComponentAsync<TEntity>(TEntity component)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Persists a batch of components to the local repository and returns the outcome for each entry.
    /// </summary>
    public Task<ResultList> SaveComponentsAsync<TEntity>(IEnumerable<TEntity> components)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Attempts to resolve the supplied identifiers into fully materialized components.
    /// </summary>
    public Task<ResultList<TEntity>> TryResolveComponentsAsync<TId, TEntity>(List<TId> toResolve)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Attempts to resolve a single unresolved component identifier.
    /// </summary>
    public Task<Result<TEntity>> TryResolveComponentAsync<TId, TEntity>(TId unresolved)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Removes a component from the local store and cache surfaces.
    /// </summary>
    public Task<Result> DeleteComponentAsync<TEntity>(TEntity entity)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Writes the current component set out to files on disk (e.g., for offline usage or backups).
    /// </summary>
    public Task<Result> SaveAllDataToFilesAsync()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Stores every successful component payload in the cache registry under its entity type.
    /// </summary>
    private void CacheAllData(Dictionary<EntityType, IReadOnlyList<IEntity>> data)
    {
        if (data.Count == 0)
        {
            _logger.LogWarning(nameof(DataManager),"No data to cache");
            return;
        }
        
      

        foreach (var kvp in data)
        {
            //filter the not successfull result out 
            var  values = kvp.Value;
            // if (!results.Success || !results.TryGetSuccessValues(_logger, out var values))
            // {
            //     _logger.LogWarning(nameof(DataManager),$"No valid components to cache found for {kvp.Key}");
            //     continue;
            // }

            CacheComponents(kvp.Key,  values.ToList());
        }
    }

    /// <summary>
    /// Stores a typed component collection in the cache registry.
    /// </summary>
    private void CacheComponents(EntityType componentType, List<IEntity> components)
    {
        if(components == null || components.Count == 0) return;
        
        _cache.StoreAll(componentType, components);
    }
    
}
