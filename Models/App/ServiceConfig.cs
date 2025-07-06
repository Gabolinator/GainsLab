using System;
using System.IO;
using GainsLab.Models.Core.LifeCycle;
using GainsLab.Models.DataManagement;
using GainsLab.Models.DataManagement.Caching;
using GainsLab.Models.DataManagement.Caching.Interface;
using GainsLab.Models.DataManagement.DB;
using GainsLab.Models.DataManagement.FileAccess;
using GainsLab.Models.Factory;
using GainsLab.Models.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GainsLab.Models.App;


/// <summary>
/// Provides configuration for dependency injection services used in the application.
/// </summary>
public static class ServiceConfig
{
    
    
    /// <summary>
    /// Configures and registers all required services for the application.
    /// </summary>
    /// <param name="services">The service collection to which services are added.</param>
    public static void ConfigureServices(IServiceCollection services)
    {

        services.AddSingleton<IWorkoutLogger, WorkoutLogger>(); 
        
        services.AddDbContext<GainLabDBContext>(options =>
        {
            var basePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "GainsLab");
            
            //ensure path exist 
            if (!Path.Exists(basePath))
            {
                Console.WriteLine($"[ServiceConfig.ConfigureServices] BaseFolder at path: {basePath} - Doesnt exist- creating it");
                Directory.CreateDirectory(basePath);
            }

            var dbPath = Path.Combine(basePath,"gainslab.db");
            
            
            Console.WriteLine($"[ServiceConfig.ConfigureServices] Db path: {dbPath}");
            options.UseSqlite($"Data Source={dbPath}");
           // options.UseSqlite("Data Source=gainlab.db");
        }, ServiceLifetime.Singleton); // Singleton to match other services

        
        services.AddSingleton<IAppLifeCycle,AppLifecycleService>();
        services.AddSingleton<IDataProvider, DataRepository>();
        services.AddSingleton<IComponentCacheRegistry, ComponentCacheRegistry>();
        services.AddSingleton<IFileDataService, JsonFilesDataService>();
        services.AddSingleton<IDataManager, DataManager>();
        
        services.AddSingleton<ComponentFactory>();
        services.AddSingleton<MainWindow>();
        services.AddSingleton<SystemInitializer>();
     


    }
}