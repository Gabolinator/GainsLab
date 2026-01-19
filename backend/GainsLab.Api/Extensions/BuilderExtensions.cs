
using GainsLab.Domain.Interfaces;
using GainsLab.Infrastructure.DB.Context;
using Microsoft.EntityFrameworkCore;
using ILogger = GainsLab.Domain.Interfaces.ILogger;

namespace GainsLab.Api.Extensions;

public static class BuilderExtensions
{
    public static void ConfigureEnvironment(this WebApplicationBuilder builder, ILogger logger)
    {
        // TODO: remove or make configurable before shipping to production.
        builder.Environment.EnvironmentName = Environments.Development;
        builder.Configuration.AddUserSecrets<Program>(optional: true);
       
        logger.Log($"ENV: {builder.Environment.EnvironmentName}");
     
    }
    
    
  
    
 
   public static void ConfigureServices(this WebApplicationBuilder builder, ILogger logger, IClock clock)
   {
       IServiceCollection services = builder.Services;
       IConfiguration configuration =  builder.Configuration;
        
       //controller , swagger, etc
        services.ConfigureServicesPreDBContext(logger, clock);
        

        var connectionString = configuration.GetConnectionString("GainsLabDb");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Missing ConnectionStrings:GainsLabDb. Add it to appsettings.Development.json or user-secrets.");
        }

        else logger.Log($"Conn (pre): '{connectionString}'");
        
       
        
        services.AddDbContext<GainLabPgDBContext>(options =>
            options.UseNpgsql(
                connectionString, 
                npgsql => npgsql.EnableRetryOnFailure())
                .EnableSensitiveDataLogging()
                .EnableDetailedErrors().LogTo(
                    message => logger.Log(message),
                    LogLevel.Debug)
            );

        
        services.ConfigureServicesPostDBContext(logger, clock);
  
        // Optional CORS for your client app
        // services.AddCors(o => o.AddDefaultPolicy(p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));
    }

}