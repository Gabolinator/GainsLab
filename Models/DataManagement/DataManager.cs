using System.Collections.Generic;
using System.Threading.Tasks;
using GainsLab.Models.Core;
using GainsLab.Models.DataManagement.Caching.Interface;
using GainsLab.Models.DataManagement.FileAccess;
using GainsLab.Models.Logging;

namespace GainsLab.Models.DataManagement;

public class DataManager :IDataManager
{

    private readonly IWorkoutLogger _logger;
    private readonly IDataProvider _dataProvider;
    private readonly IComponentCacheRegistry _cache;
    private readonly IFileDataService _fileDataService;

    public DataManager(IWorkoutLogger logger, IDataProvider dataProvider, IComponentCacheRegistry cache, IFileDataService fileDataService)
    {
        _logger = logger;
        _dataProvider = dataProvider;
        _cache = cache;
        _fileDataService = fileDataService;

    }

    public async Task InitializeAsync()
    {
       // await LoadAnCacheAllDataAsync();
    }

    public async Task LoadAndCacheDataAsync()
    {
       //load all files from paths 
       //get all the current object in the database
       //add all the objects loaded + the current object in database to cache
       
       //or 
       //load all files from paths 
       //add all the loaded objects to the database
       //get all the object in the database and add it to the cache
    }

    public  async Task<IEnumerable<TComponent>> ResolveComponentsAsync<TComponent>(List<IIdentifier> unresolved) where TComponent : IWorkoutComponent
    {
        throw new System.NotImplementedException();
    }
    
    public  async Task<TComponent> ResolveComponentAsync<TComponent>(IIdentifier unresolved) where TComponent : IWorkoutComponent
    {
        throw new System.NotImplementedException();
    }

    public Task<T?> GetComponentAsync<T>(IIdentifier id) where T : IWorkoutComponent
    {
        throw new System.NotImplementedException();
    }

    public Task<List<T>> GetComponentsAsync<T>(List<IIdentifier> ids) where T : IWorkoutComponent
    {
        throw new System.NotImplementedException();
    }

    public Task SaveComponentAsync<T>(T component) where T : IWorkoutComponent
    {
        throw new System.NotImplementedException();
    }

    public Task SaveComponentsAsync<T>(IEnumerable<T> components) where T : IWorkoutComponent
    {
        throw new System.NotImplementedException();
    }

    public Task DeleteComponentAsync<T>(IIdentifier id) where T : IWorkoutComponent
    {
        throw new System.NotImplementedException();
    }
}