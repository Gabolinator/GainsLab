using System;
using System.Threading.Tasks;
using GainsLab.Application.Interfaces;
using GainsLab.Core.Models.Core.Interfaces.DataManagement;
using GainsLab.Domain.Interfaces;
using GainsLab.Domain.Interfaces.Caching;
using GainsLab.Models.App.LifeCycle;
using GainsLab.Models.DataManagement;
using GainsLab.Models.DataManagement.DB;
using Microsoft.Extensions.DependencyInjection;

namespace GainsLab.Models.App;


/// <summary>
/// Initializes the system by setting up essential services and loading data.
/// </summary>
public class SystemInitializer : ISystemInitializer
{
    private readonly ILogger _workoutLogger;
   
    
    
    private readonly IComponentCacheRegistry _cacheRegistry;
    private readonly IDataManager _dataManager;
    private readonly IAppLifeCycle _lifeCycle;


    /// <summary>
    /// Initializes a new instance of the <see cref="SystemInitializer"/> class.
    /// </summary>
    /// <param name="workoutLogger">The workout logger for logging activities.</param>
    /// <param name="cacheRegistry">The component cache registry for caching components.</param>
    /// <param name="dataManager">The data manager for managing application data.</param>
    public SystemInitializer(
        ILogger workoutLogger, 
        IComponentCacheRegistry cacheRegistry, 
        IDataManager dataManager,
        IAppLifeCycle lifeCycle)
    {
        _workoutLogger = workoutLogger;
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
        
        //
        
        await _cacheRegistry.InitializeAsync();
        await _dataManager.InitializeAsync();
        await _dataManager.LoadAndCacheDataAsync();
     
    }
}