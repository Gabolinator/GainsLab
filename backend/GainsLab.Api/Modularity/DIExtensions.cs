using GainsLab.Application.Interfaces.DataManagement.Repository;
using GainsLab.Contracts.Dtos.SyncDto;
using GainsLab.Domain.Interfaces;
using GainsLab.Infrastructure.DB;
using GainsLab.Infrastructure.DB.Repository;
using GainsLab.Infrastructure.SyncService;
using ILogger = GainsLab.Domain.Interfaces.ILogger;

namespace GainsLab.Api.Modularity;

public static class DiExtensions
{
    public static void ConfigureServicesPreDBContext(this IServiceCollection services, ILogger logger, IClock clock)
    {
        services.AddSingleton<ILogger>(logger);
        services.AddSingleton<IClock>(clock);
        AddSyncServices(services);
        services.AddControllers();
        services.AddSwagger();
    }
    
    public static void ConfigureServicesPostDBContext(this IServiceCollection services, ILogger logger, IClock clock)
    {
       
        AddEntitiesRepositories(services);
        
        services.AddSingleton<IEntitySeedResolver, EntitySeedResolver>();
    }

    private static void AddEntitiesRepositories(IServiceCollection services)
    {
        services.AddScoped<IDescriptorRepository, DescriptorRepository>();
        services.AddScoped<IEquipmentRepository, EquipmentRepository>();
        services.AddScoped<IMuscleRepository, MuscleRepository>();
        services.AddScoped<IMovementCategoryRepository, MovementCategoryRepository>();
        services.AddScoped<IMovementRepository, MovementRepository>();
    }


    public static void AddSyncServices(IServiceCollection services)
    {
  
        services.AddScoped<ISyncService<EquipmentSyncDTO>, EquipmentSyncService>();
        services.AddScoped<ISyncService<DescriptorSyncDTO>, DescriptorSyncService>();
        services.AddScoped<ISyncService<MovementCategorySyncDTO>, MovementCategorySyncService>();
        services.AddScoped<ISyncService<MuscleSyncDTO>, MuscleSyncService>(); 
        services.AddScoped<ISyncService<MovementSyncDTO>, MovementSyncService>();

    
        // Also expose as non-generic so the controller can enumerate:
        services.AddScoped<ISyncService>(sp => sp.GetRequiredService<ISyncService<EquipmentSyncDTO>>());
        services.AddScoped<ISyncService>(sp => sp.GetRequiredService<ISyncService<DescriptorSyncDTO>>());
        services.AddScoped<ISyncService>(sp => sp.GetRequiredService<ISyncService<MuscleSyncDTO>>());
        services.AddScoped<ISyncService>(sp => sp.GetRequiredService<ISyncService<MovementCategorySyncDTO>>());
        services.AddScoped<ISyncService>(sp => sp.GetRequiredService<ISyncService<MovementSyncDTO>>());

            
    }

    public static void AddSwagger(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();

        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new()
            {
                Title = "GainsLab API",
                Version = "v1"
            });
        });
    }
}