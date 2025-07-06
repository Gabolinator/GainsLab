using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GainsLab.Models.Core;
using GainsLab.Models.Core.Interfaces;
using GainsLab.Models.Core.LifeCycle;
using GainsLab.Models.Core.Results;
using GainsLab.Models.DataManagement.Caching.Interface;
using GainsLab.Models.DataManagement.DB;
using GainsLab.Models.DataManagement.FileAccess;
using GainsLab.Models.Factory;
using GainsLab.Models.Logging;

namespace GainsLab.Models.DataManagement;

public class DataManager :IDataManager
{

    private readonly IWorkoutLogger _logger;
    
    //bridge to database
    private readonly IDataProvider _dataProvider;
    
    //access to component caches 
    private readonly IComponentCacheRegistry _cache;
    
    //read and write data to files
    private readonly IFileDataService _fileDataService;

    private string fileDirectory;
    
    public DataManager(IWorkoutLogger logger, IDataProvider dataProvider, IComponentCacheRegistry cache, IFileDataService fileDataService)
    {
        _logger = logger;
        _dataProvider = dataProvider;
        _cache = cache;
        _fileDataService = fileDataService;

    }

        public async Task InitializeAsync(IAppLifeCycle lifeCycle)
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

        lifeCycle.onAppExitAsync +=SaveAllDataToFilesAsync;
    }

    public async Task LoadAndCacheDataAsync()
    {
        
        _logger.Log(nameof(DataManager), "Loading and caching data...");

        //Load from files
        //not implemented
        Dictionary<eWorkoutComponents, List<IWorkoutComponent>> fileData = await _fileDataService.LoadAllComponentsAsync();
        
        //batch insert all in database
        //not implemented
        await _dataProvider.BatchSaveComponentsAsync(fileData); 
        
        //Load from DB to cache
        //not implemented
        Dictionary<eWorkoutComponents, List<IWorkoutComponent>>
            dataFromDB = await _dataProvider.GetAllComponentsAsync();
        
        foreach (var kvp in dataFromDB)
        {
            List<IWorkoutComponent> dbComponents = kvp.Value;
            if(dbComponents.Count == 0 ) continue;
            _cache.StoreAll(kvp.Key, dbComponents);
        }

        _logger.Log(nameof(DataManager), "Finished loading and caching data.");
    }

    public async Task<Result<T>> TryGetComponentAsync<T>(IIdentifier id) where T : IWorkoutComponent
    {
        if (id.IsEmpty()) return Results.FailureResult<T>("Id is empty");

        if (_cache.TryGetComponent<T>(id, out var cached))
            return Results.SuccessResult(cached!);

        var dbResult = await _dataProvider.TryGetComponentAsync<T>(id);
        if (!dbResult.Success)
            return Results.FailureResult<T>("Component not found in DB.");

        // store in cache
        _cache.Store(dbResult.Value!);
        return Results.SuccessResult(dbResult.Value!);
    }

  

    public async Task<Result<IEnumerable<T>>> TryGetComponentsAsync<T>(IEnumerable<IIdentifier> ids) where T : IWorkoutComponent
    {
        var foundComponents = new List<T>();
        var toResolve = new List<IIdentifier>();

        foreach (var id in ids)
        {
            if (_cache.TryGetComponent<T>(id, out var cached) && cached is not null)
                foundComponents.Add(cached);
            else
                toResolve.Add(id);
        }

        //no components to resolve 
        if (toResolve.Count == 0)
        {
            //if no component found result failure
            return foundComponents.Count > 0
                ? Results.SuccessResult<IEnumerable<T>>(foundComponents)
                : Results.FailureResult<IEnumerable<T>>("No matches found");
        }

        var resolveResult = await TryResolveComponentsAsync<T>(toResolve);

        //failed to resolve components
        if (!resolveResult.Success)
            return foundComponents.Count > 0
                ? Results.SuccessResult<IEnumerable<T>>(foundComponents)
                : Results.FailureResult<IEnumerable<T>>("No components could be found or resolved");

        foundComponents.AddRange(resolveResult.Value!);
        return Results.SuccessResult<IEnumerable<T>>(foundComponents);
    }

    public async Task<Result<T>> TryResolveComponentAsync<T>(IIdentifier unresolved) where T : IWorkoutComponent
    {
        Result<T> dbResult = await _dataProvider.TryGetComponentAsync<T>(unresolved);
        if (!dbResult.Success || dbResult.Value is null) return Results.FailureResult<T>("Could not Resolve Component");
        
        //add to cache
        _cache.Store(dbResult.Value!);

        return dbResult;
    }


    public async Task<Result<IEnumerable<T>>> TryResolveComponentsAsync<T>(List<IIdentifier> toResolve) where T : IWorkoutComponent
    {
        
        List<Result<T>> dbResults = await _dataProvider.TryGetComponentsAsync<T>(toResolve);
        var resolved = dbResults
            .Where(r => r.Success && r.Value is not null)
            .Select(r => r.Value!)
            .ToList();

        if (resolved.Count == 0)
            return Results.FailureResult<IEnumerable<T>>("No components were resolved");

        var type = resolved[0].ComponentType;
        _cache.StoreAll(type, resolved);

        return Results.SuccessResult<IEnumerable<T>>(resolved);
    }

    public async Task SaveComponentAsync<T>(T component) where T : IWorkoutComponent
    {
        if (component.Identifier.IsEmpty()) return;

        _logger.Log(nameof(DataManager), $"Saving component {component.Name}");
        _cache.Store(component);
        
        //save to database
        await _dataProvider.SaveComponentAsync(component);
    }

    public async Task SaveComponentsAsync<T>(IEnumerable<T> components) where T : IWorkoutComponent
    {
        var list = components.ToList();
        if(list.Count ==0) return;
        
        
        _cache.StoreAll<T>(list.ToList());
        
        //
        // foreach (var component in list)
        // {
        //     if (component.Identifier.IsEmpty()) continue;
        //     _cache.StoreAll(components);
        // }
        
        await _dataProvider.SaveComponentsAsync(list[0].ComponentType,list);
    }

    public async Task DeleteComponentAsync<T>(IIdentifier id) where T : IWorkoutComponent
    {
        if (id.IsEmpty()) return;

        _cache.Remove<T>(id);
        await _dataProvider.DeleteComponentAsync<T>(id);
     
    }

    public async Task SaveAllDataToFilesAsync()
    {
        //get all the data in the registry 
        //and save it 
        var data = await _dataProvider.GetAllComponentsAsync();
        await _fileDataService.WriteAllComponentsAsync(data, fileDirectory, ".json");

    }

  
}
