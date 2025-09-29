using System;
using System.Threading.Tasks;
using GainsLab.Core.Models.Core.Interfaces.Caching;
using GainsLab.Models.Core.LifeCycle;
using GainsLab.Models.DataManagement;
using GainsLab.Models.DataManagement.Caching.Interface;
using GainsLab.Models.DataManagement.DB;
using GainsLab.Models.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace GainsLab.Models.App;


/// <summary>
/// Initializes the system by setting up essential services and loading data.
/// </summary>
public class SystemInitializer : ISystemInitializer
{
    private readonly ILogger _workoutLogger;
    private readonly IDataProvider _dataProvider;
    private readonly IComponentCacheRegistry _cacheRegistry;
    private readonly IDataManager _dataManager;
    private readonly IAppLifeCycle _lifeCycle;


    /// <summary>
    /// Initializes a new instance of the <see cref="SystemInitializer"/> class.
    /// </summary>
    /// <param name="workoutLogger">The workout logger for logging activities.</param>
    /// <param name="dataProvider">The data provider for initializing data.</param>
    /// <param name="cacheRegistry">The component cache registry for caching components.</param>
    /// <param name="dataManager">The data manager for managing application data.</param>
    public SystemInitializer(
        ILogger workoutLogger, 
        IDataProvider dataProvider, 
        IComponentCacheRegistry cacheRegistry, 
        IDataManager dataManager,
        IAppLifeCycle lifeCycle)
    {
        _workoutLogger = workoutLogger;
        _dataProvider = dataProvider;
        _cacheRegistry = cacheRegistry;
        _dataManager = dataManager;
        _lifeCycle = lifeCycle;
    }
    
    
    
    /// <summary>
    /// Asynchronously initializes the system by initializing services and loading cached data.
    /// </summary>
    /// <returns>A task that represents the asynchronous initialization operation.</returns>
    public async Task InitializeAsync( IServiceProvider serviceProvider)
    {
        
        _workoutLogger.Log(nameof(SystemInitializer),"Initializing system...");
       
        await _dataProvider.InitializeAsync();
        await _cacheRegistry.InitializeAsync();
        await _dataManager.InitializeAsync();
        await _dataManager.LoadAndCacheDataAsync();
     
    }
}