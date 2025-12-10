using System;
using System.IO;
using GainsLab.Contracts;
using GainsLab.Core.Models.Core.Factory;
using GainsLab.Core.Models.Core.Interfaces.DataManagement;
using GainsLab.Infrastructure.DB;
using GainsLab.Infrastructure.DB.Context;
using GainsLab.Infrastructure.DB.Outbox;
using GainsLab.Models.App.LifeCycle;
using GainsLab.Models.DataManagement;
using GainsLab.Models.DataManagement.DB;
using GainsLab.Models.DataManagement.FileAccess;
using System.Net.Http;
using GainsLab.Application;
using GainsLab.Application.Interfaces;
using GainsLab.Application.Outbox;
using GainsLab.Domain.Interfaces;
using GainsLab.Domain.Interfaces.Caching;
using GainsLab.Infrastructure;
using GainsLab.Infrastructure.Caching;
using GainsLab.Infrastructure.Logging;
using GainsLab.Infrastructure.Outbox;
using GainsLab.Infrastructure.Sync;
using GainsLab.Infrastructure.Sync.Processor;
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
        services.AddSingleton<GainsLab.Application.Interfaces.DataManagement.ILocalRepository, DataRepository>();
        void ConfigureSyncClient(IServiceProvider sp, HttpClient client)
        {
            var logger = sp.GetService<ILogger>();
            var baseAddress = ResolveSyncBaseAddress(logger);
            client.BaseAddress = baseAddress;
            client.Timeout = TimeSpan.FromSeconds(30);
        }

        services.AddHttpClient<GainsLab.Application.Interfaces.Sync.IRemoteProvider, HttpDataProvider>(ConfigureSyncClient);
        services.AddHttpClient("SyncApi", ConfigureSyncClient);
        
        services.AddSingleton<IComponentCacheRegistry, ComponentCacheRegistry>();
        services.AddSingleton<IFileDataService, JsonFilesDataService>();

        services.AddSingleton<IEntitySeedResolver, EntitySeedResolver>();
        services.AddSingleton<GainsLab.Application.Interfaces.DataManagement.IDataManager, DataManager>();
        services.AddSingleton<ISyncCursorStore, FileSyncCursorStore>();
        services.AddSingleton<IOutboxDispatcher>(sp =>
        {
            var factory = sp.GetRequiredService<IDbContextFactory<GainLabSQLDBContext>>();
            var logger = sp.GetRequiredService<ILogger>();
            var httpFactory = sp.GetRequiredService<IHttpClientFactory>();
            var client = httpFactory.CreateClient("SyncApi");
            return new OutboxDispatcher(factory, logger, client);
        });


        services= AddSyncProcessors(services);
        
      
        
       // services.AddSingleton<EntityFactory>();
        services.AddSingleton<MainWindow>();
        services.AddSingleton<SystemInitializer>();
     
    }

    private static IServiceCollection AddSyncProcessors(IServiceCollection services)
    {
        var descriptorResolver = new DescriptorResolver("syn");
        services.AddSingleton<IDescriptorResolver>(descriptorResolver); 
        services.AddSingleton<ISyncEntityProcessor, DescriptorSyncProcessor>();
        services.AddSingleton<ISyncEntityProcessor, EquipmentSyncProcessor>();
        services.AddSingleton<ISyncEntityProcessor, MuscleSyncProcessor>();
        services.AddSingleton<ISyncEntityProcessor, MovementCategorySyncProcessor>();
        services.AddSingleton<ISyncOrchestrator, SyncOrchestrator>();

        return services;
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
