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

public static class ServiceConfig
{
    public static void ConfigureServices(IServiceCollection services)
    {

        services.AddSingleton<IWorkoutLogger, WorkoutLogger>(); 
        
        services.AddDbContext<GainLabDBContext>(options =>
        {
            options.UseSqlite("Data Source=gainlab.db");
        }, ServiceLifetime.Singleton); // Singleton to match other services

        
       
        services.AddSingleton<IDataProvider, DataRepository>();
        services.AddSingleton<IComponentCacheRegistry, ComponentCacheRegistry>();
        services.AddSingleton<IFileDataService, JsonFilesDataService>();
        services.AddSingleton<IDataManager, DataManager>();
        
        services.AddSingleton<ComponentFactory>();
        services.AddSingleton<MainWindow>();
        services.AddSingleton<SystemInitializer>();
        
        
        
    }
}