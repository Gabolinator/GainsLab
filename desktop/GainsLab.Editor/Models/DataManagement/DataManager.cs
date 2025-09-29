using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GainsLab.Core.Models.Core.Interfaces.Caching;
using GainsLab.Core.Models.Core.Interfaces.Entity;
using GainsLab.Models.Core;
using GainsLab.Models.Core.LifeCycle;
using GainsLab.Models.Core.Results;
using GainsLab.Models.DataManagement.DB;
using GainsLab.Models.DataManagement.FileAccess;
using GainsLab.Models.Logging;

namespace GainsLab.Models.DataManagement;

public class DataManager :IDataManager
{

    private readonly ILogger _logger;
    
    //bridge to database
    private readonly IDataProvider _dataProvider;
    
    //access to component caches 
    private readonly IComponentCacheRegistry _cache;
    
    //read and write data to files
    private readonly IFileDataService _fileDataService;

    private string fileDirectory;
    private readonly IAppLifeCycle _lifeCycle;

    public DataManager(IAppLifeCycle lifeCycle, ILogger logger, IDataProvider dataProvider, IComponentCacheRegistry cache, IFileDataService fileDataService)
    {
        _logger = logger;
        _dataProvider = dataProvider;
        _cache = cache;
        _fileDataService = fileDataService;
        _lifeCycle = lifeCycle;

    }

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
       
    }

        

    public async Task<Result> LoadAndCacheDataAsync()
    {
      
        
        _logger.Log(nameof(DataManager), "Loading and caching data...");

        //Load from files
        //not implemented
        Dictionary<eWorkoutComponents,  ResultList<IEntity>> fileData = await _fileDataService.LoadAllComponentsAsync();
        
        
        //batch insert all new loaded data in database
        //not implemented
        var result =  await _dataProvider.BatchSaveComponentsAsync(fileData);

        var batchSaveSuccess = result.Success;
        if (!batchSaveSuccess)
        {
            _logger.LogWarning(nameof(DataManager), $"Could not Save loaded data to DB.{result.GetErrorMessage()}");
            
        }



        //Load from DB to cache
        //not implemented
        var dataFromDB = await _dataProvider.GetAllComponentsAsync<IEntity>();
        
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

    private void CacheAllData( Dictionary<eWorkoutComponents, ResultList<IEntity>> data)
    {
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

    private void CacheComponents(eWorkoutComponents componentType, List<IEntity> components)
    {
        if(components == null || components.Count == 0) return;
        
        _cache.StoreAll(componentType, components);
    }


    // public async Task<Result<T>> TryGetEntityAsync<T>(IIdentifier id) where T : IWorkoutComponent
    // {
    //     if (id.IsEmpty()) return Result<T>.Failure("Id is empty");
    //
    //     if (_cache.TryGetComponent<T>(id, out var cached))
    //         return Result<T>.SuccessResult(cached!);
    //
    //     var dbResult = await _dataProvider.GetComponentAsync<T>(id);
    //     if (!dbResult.Success)
    //         return  Result<T>.Failure("Component not found in DB.");
    //
    //     // store in cache
    //     _cache.Store(dbResult.Value!);
    //     return Result<T>.SuccessResult(dbResult.Value!);
    // }
    //
    //
    //
    // public async Task<ResultList<T>> TryGetComponentsAsync<T>(IEnumerable<IIdentifier> ids) where T : IWorkoutComponent
    // {
    //     var foundComponents = new List<T>();
    //     var toResolve = new List<IIdentifier>();
    //
    //     foreach (var id in ids)
    //     {
    //         if (_cache.TryGetComponent<T>(id, out var cached) && cached is not null)
    //             foundComponents.Add(cached);
    //         else
    //             toResolve.Add(id);
    //     }
    //
    //     //no components to resolve 
    //     if (toResolve.Count == 0)
    //     {
    //         //todo 
    //         return ResultList<T>.FailureResult<T>("No  components to resolve ");
    //     }
    //
    //     var resolveResult = await TryResolveComponentsAsync<T>(toResolve);
    //
    //     //failed to resolve components
    //     if (!resolveResult.Success || !resolveResult.TryGetSuccessValues(out var resolved))
    //     {
    //         return ResultList<T>.FailureResult<T>("Failed to resolve components");
    //     }
    //
    //     foundComponents = resolved.ToList();
    //     
    //     return ResultList<T>.SuccessResults(foundComponents);
    // }
    //
    // public async Task<Result<T>> TryResolveComponentAsync<T>(IIdentifier unresolved) where T : IWorkoutComponent
    // {
    //     Result<T> dbResult = await _dataProvider.GetComponentAsync<T>(unresolved);
    //     if (!dbResult.Success || dbResult.Value is null) return Result<T>.Failure("Could not Resolve Component");
    //     
    //     //add to cache
    //     _cache.Store(dbResult.Value!);
    //
    //     return dbResult;
    // }
    //
    //
    // public async Task<ResultList<T>> TryResolveComponentsAsync<T>(List<IIdentifier> toResolve) where T : IWorkoutComponent
    // {
    //     
    //     var  dbResults = await _dataProvider.GetComponentsAsync<T>(toResolve);
    //     if (!dbResults.TryGetSuccessValues(_logger, out var r))
    //     {
    //         return ResultList<T>.FailureResult<T>("No components were resolved");
    //     }
    //
    //     var resolved = r.ToList();
    //     
    //     // var resolved = dbResults
    //     //     .Where(r => r.Success && r.Value is not null)
    //     //     .Select(r => r.Value!)
    //     //     .ToList();
    //
    //     
    //     var type = resolved[0].ComponentType;
    //     _cache.StoreAll(type, resolved);
    //
    //     return ResultList<T>.SuccessResults<T>(resolved);
    // }

    // public async Task<Result> SaveComponentAsync<T>(T component) where T : IWorkoutComponent
    // {
    //     if (component.Identifier.IsEmpty()) return Result.Failure("Identifier list empty");
    //
    //     _logger.Log(nameof(DataManager), $"Saving component {component.Name}");
    //     _cache.Store(component);
    //     
    //     //save to database
    //    var saveResult = await _dataProvider.SaveComponentAsync(component);
    //    return saveResult.Success ? Result.SuccessResult() : Result.Failure(((Result)saveResult).GetErrorMessage());
    // }
    //
    // public async Task<ResultList> SaveComponentsAsync<T>(IEnumerable<T> components) where T : IWorkoutComponent
    // {
    //     var list = components.ToList();
    //     if(list.Count ==0) return ResultList.FailureResult("No component to save");
    //     
    //     
    //     _cache.StoreAll<T>(list.ToList());
    //     
    //    var result = await _dataProvider.SaveComponentsAsync(list[0].ComponentType,list);
    //    
    //    //get all the success 
    //    return result.ToBoolResultList();
    // }
    //
    // public async Task<Result> DeleteComponentAsync<T>(IIdentifier id) where T : IWorkoutComponent
    // {
    //     if (id.IsEmpty()) return Result.Failure("Failed to delete: No id ");
    //
    //     _cache.Remove<T>(id);
    //     return await _dataProvider.DeleteComponentAsync<T>(id);
    //  
    // }
    //
    // public async Task<Result> SaveAllDataToFilesAsync()
    // {
    //     //get all the data in the registry 
    //     //and save it 
    //     var data = await _dataProvider.GetAllComponentsAsync();
    //     if (!data.Success) return Result.Failure(data.GetErrorMessage());
    //
    //     var dict = new Dictionary<eWorkoutComponents, List<IWorkoutComponent>>();
    //     if (!data.TryGetValue(out var values)) return Result.Failure(data.GetErrorMessage());
    //     var v = values!;
    //
    //     foreach (var kvp in v)
    //     {
    //         if (!kvp.Value.TryGetSuccessValues(out var components)) continue;
    //         var list = components.ToList();
    //
    //
    //         if (!dict.TryAdd(kvp.Key, list))
    //         {
    //             _logger.LogWarning(nameof(DataManager),$"Trying to add duplicate for {kvp.Key}");
    //         }
    //     }
    //         
    //     if(dict.Count ==0) return  Result.Failure("No data in dictionnary");
    //
    //
    //
    //
    //     return  await _fileDataService.WriteAllComponentsAsync( dict, fileDirectory, ".json");
    //     
    // }

  
}
