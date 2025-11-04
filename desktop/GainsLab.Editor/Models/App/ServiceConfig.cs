using System;
using System.IO;
using GainsLab.Contracts;
using GainsLab.Core.Models.Core.Factory;
using GainsLab.Core.Models.Core.Interfaces.Caching;
using GainsLab.Core.Models.Core.Interfaces.DataManagement;
using GainsLab.Core.Models.Core.Utilities.Logging;
using GainsLab.Infrastructure.DB;
using GainsLab.Infrastructure.DB.Context;
using GainsLab.Infrastructure.DB.Outbox;
using GainsLab.Models.App.LifeCycle;
using GainsLab.Models.Core.LifeCycle;
using GainsLab.Models.DataManagement;
using GainsLab.Models.DataManagement.Caching;
using GainsLab.Models.DataManagement.Caching.Interface;
using GainsLab.Models.DataManagement.DB;
using GainsLab.Models.DataManagement.FileAccess;
using GainsLab.Models.Factory;
using GainsLab.Models.DataManagement.Sync;
using System.Net.Http;
using GainsLab.Contracts.Outbox;
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

        services.AddSingleton<ILogger, GainsLabLogger>(); 
        services.AddScoped<OutboxInterceptor>();
       

        var dbDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "GainsLab");

        Directory.CreateDirectory(dbDir); 

        var dbPath = Path.Combine(dbDir, "local_gainslab.db");
        
        void ConfigureSqlite(IServiceProvider sp, DbContextOptionsBuilder options)
        {
            options
                .UseSqlite($"Data Source={dbPath}")
                .EnableDetailedErrors()
                .EnableSensitiveDataLogging();

            var interceptor = sp.GetRequiredService<OutboxInterceptor>();
            options.AddInterceptors(interceptor);
        }

        
        services.AddDbContext<GainLabSQLDBContext>(ConfigureSqlite);
        services.AddDbContextFactory<GainLabSQLDBContext>(ConfigureSqlite);

        
        services.AddSingleton<IAppLifeCycle,AppLifecycleService>();
        services.AddSingleton<ILocalRepository, DataRepository>();
        
        void ConfigureSyncClient(HttpClient client)
        {
            var baseUrl = Environment.GetEnvironmentVariable("GAINS_SYNC_BASE_URL");
            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                baseUrl = "https://localhost:5001/";
            }

            if (!baseUrl.EndsWith("/")) baseUrl += "/";
            client.BaseAddress = new Uri(baseUrl);
            client.Timeout = TimeSpan.FromSeconds(30);
        }

        services.AddHttpClient<IRemoteProvider, HttpDataProvider>(ConfigureSyncClient);
        services.AddHttpClient("SyncApi", ConfigureSyncClient);
        
        services.AddSingleton<IComponentCacheRegistry, ComponentCacheRegistry>();
        services.AddSingleton<IFileDataService, JsonFilesDataService>();
        services.AddSingleton<IDataManager, DataManager>();
        services.AddSingleton<ISyncCursorStore, FileSyncCursorStore>();
        services.AddSingleton<IOutboxDispatcher>(sp =>
        {
            var factory = sp.GetRequiredService<IDbContextFactory<GainLabSQLDBContext>>();
            var logger = sp.GetRequiredService<ILogger>();
            var httpFactory = sp.GetRequiredService<IHttpClientFactory>();
            var client = httpFactory.CreateClient("SyncApi");
            return new OutboxDispatcher(factory, logger, client);
        });
        services.AddSingleton<ISyncEntityProcessor, EquipmentSyncProcessor>();
        services.AddSingleton<ISyncEntityProcessor, DescriptorSyncProcessor>();
        services.AddSingleton<ISyncOrchestrator, SyncOrchestrator>();
        
       // services.AddSingleton<EntityFactory>();
        services.AddSingleton<MainWindow>();
        services.AddSingleton<SystemInitializer>();
     
    }
}
