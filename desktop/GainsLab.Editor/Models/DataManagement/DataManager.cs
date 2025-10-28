using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GainsLab.Core.Models.Core;
using GainsLab.Core.Models.Core.Interfaces.Caching;
using GainsLab.Core.Models.Core.Interfaces.DataManagement;
using GainsLab.Core.Models.Core.Interfaces.Entity;
using GainsLab.Core.Models.Core.Results;
using GainsLab.Core.Models.Core.Utilities.Logging;
using GainsLab.Models.Core.LifeCycle;
using GainsLab.Models.DataManagement.Sync;

namespace GainsLab.Models.DataManagement;

/// <summary>
/// Coordinates pulling data from remote/local sources and caching it for the desktop app.
/// </summary>
public class DataManager :IDataManager
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
       
        //kick off a full seed the first time we spin up so caches stay in sync
        _logger.Log(nameof(DataManager), "Seed Initial data...");
        var seedResult = await _syncOrchestrator.SeedAsync();
        if (!seedResult.Success)
        {
            _logger.LogError(nameof(DataManager),
                $"Initial sync failed: {seedResult.GetErrorMessage()}");
        }
        
        //we check if there is any data out of sync
        //if there is changes not pulled => we update in local db 
        //if there is changes not pushed => we update in remote db 
        
    }

        

    public async Task<Result> LoadAndCacheDataAsync()
    {
      
       
        
        _logger.Log(nameof(DataManager), "Loading and caching data...");

        //Load from files
        //not implemented
        Dictionary<EntityType,  ResultList<IEntity>> fileData = await _fileDataService.LoadAllComponentsAsync();
        
        
        //batch insert all new loaded data in database
        //not implemented
        var result =  await _local.BatchSaveComponentsAsync(fileData);

        var batchSaveSuccess = result.Success;
        if (!batchSaveSuccess)
        {
            _logger.LogWarning(nameof(DataManager), $"Could not Save loaded data to DB.{result.GetErrorMessage()}");
            
        }



        //Load from DB to cache
        //not implemented
        var dataFromDB = await _local.GetAllComponentsAsync<IEntity>();
        
        var fromDBSuccess = dataFromDB.Success;
        if (!fromDBSuccess || dataFromDB.Value == null)
        {
            _logger.LogWarning(nameof(DataManager), $"Could Load all component data from DB.{(result.GetErrorMessage())}");
            
        }

        else CacheAllData(dataFromDB.Value);


        bool allFailed = !fromDBSuccess && !batchSaveSuccess;
        _logger.Log(nameof(DataManager), "Finished loading and caching data.");
        return !allFailed ? Result.SuccessResult() : Result.Failure("Loading and Retreiving datafrom database failed");

    }

    public Task<Result<TEntity>> TryGetEntityAsync<TId, TEntity>(TId id)
    {
        throw new NotImplementedException();
    }

    public Task<ResultList<TEntity>> TryGetComponentsAsync<TId, TEntity>(IEnumerable<TId> ids)
    {
        throw new NotImplementedException();
    }

    public Task<Result> SaveComponentAsync<TEntity>(TEntity component)
    {
        throw new NotImplementedException();
    }

    public Task<ResultList> SaveComponentsAsync<TEntity>(IEnumerable<TEntity> components)
    {
        throw new NotImplementedException();
    }

    public Task<ResultList<TEntity>> TryResolveComponentsAsync<TId, TEntity>(List<TId> toResolve)
    {
        throw new NotImplementedException();
    }

    public Task<Result<TEntity>> TryResolveComponentAsync<TId, TEntity>(TId unresolved)
    {
        throw new NotImplementedException();
    }

    public Task<Result> DeleteComponentAsync<TEntity>(TEntity entity)
    {
        throw new NotImplementedException();
    }

    public Task<Result> SaveAllDataToFilesAsync()
    {
        throw new NotImplementedException();
    }

    private void CacheAllData( Dictionary<EntityType, ResultList<IEntity>> data)
    {
        if (data.Count == 0)
        {
            _logger.LogWarning(nameof(DataManager),"No data to cache");
            return;
        }

        foreach (var kvp in data)
        {
            //filter the not successfull result out 
            var results = kvp.Value;
            if (!results.Success || !results.TryGetSuccessValues(_logger, out var values))
            {
                _logger.LogWarning(nameof(DataManager),$"No valid components to cache found for {kvp.Key}");
                continue;
            }

            CacheComponents(kvp.Key,  values.ToList());
        }
    }

    private void CacheComponents(EntityType componentType, List<IEntity> components)
    {
        if(components == null || components.Count == 0) return;
        
        _cache.StoreAll(componentType, components);
    }
    
}
