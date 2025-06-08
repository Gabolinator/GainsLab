using System.Threading.Tasks;
using GainsLab.Models.DataManagement;
using GainsLab.Models.DataManagement.Caching.Interface;
using GainsLab.Models.Logging;

namespace GainsLab.Models.App;

public class SystemInitializer : ISystemInitializer
{
    private readonly IWorkoutLogger _workoutLogger;
    private readonly IDataProvider _dataProvider;
    private readonly IComponentCacheRegistry _cacheRegistry;
    private readonly IDataManager _dataManager;

    public SystemInitializer(IWorkoutLogger workoutLogger, IDataProvider dataProvider, IComponentCacheRegistry cacheRegistry, IDataManager dataManager)
    {
        _workoutLogger = workoutLogger;
        _dataProvider = dataProvider;
        _cacheRegistry = cacheRegistry;
        _dataManager = dataManager;
    }
    
    public async Task InitializeAsync()
    {
        _workoutLogger.Log(nameof(SystemInitializer),"Initializing system...");
       await _dataProvider.InitializeAsync();
       await _cacheRegistry.InitializeAsync();
       await _dataManager.InitializeAsync();
       await _dataManager.LoadAndCacheDataAsync();







    }
}

