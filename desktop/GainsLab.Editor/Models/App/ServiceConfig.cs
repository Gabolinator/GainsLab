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
using GainsLab.Models.DataManagement.Sync.Processor;
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
        void ConfigureSyncClient(IServiceProvider sp, HttpClient client)
        {
            var logger = sp.GetService<ILogger>();
            var baseAddress = ResolveSyncBaseAddress(logger);
            client.BaseAddress = baseAddress;
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
        services.AddSingleton<ISyncEntityProcessor, MuscleSyncProcessor>();
        services.AddSingleton<ISyncOrchestrator, SyncOrchestrator>();
        
       // services.AddSingleton<EntityFactory>();
        services.AddSingleton<MainWindow>();
        services.AddSingleton<SystemInitializer>();
     
    }
    private static Uri ResolveSyncBaseAddress(ILogger? logger)
    {
        const string defaultBase = "https://localhost:5001/";
        var configured = Environment.GetEnvironmentVariable("GAINS_SYNC_BASE_URL");
        var candidate = string.IsNullOrWhiteSpace(configured) ? defaultBase : configured.Trim();

        if (!Uri.TryCreate(candidate, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            var message =
                $"GAINS_SYNC_BASE_URL must be an absolute http/https URL. Current value: '{candidate}'.";
            logger?.LogError(nameof(ServiceConfig), message);
            throw new InvalidOperationException(message);
        }

        var normalized = uri.ToString();
        if (!normalized.EndsWith("/")) normalized += "/";

        logger?.Log(nameof(ServiceConfig), $"Using sync server base address {normalized}");
        return new Uri(normalized);
    }
}
