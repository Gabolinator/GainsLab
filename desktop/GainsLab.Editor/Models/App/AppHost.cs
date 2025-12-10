using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using GainsLab.Domain.Interfaces;
using GainsLab.Infrastructure.DB.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GainsLab.Models.App;


/// <summary>
/// Hosts and initializes the application, setting up dependency injection and running startup tasks.
/// </summary>
public class AppHost
{
    
    private IServiceProvider _serviceProvider;

    /// <summary>
    /// Gets the configured service provider after <see cref="RunAsync"/> completes.
    /// </summary>
    public IServiceProvider ServiceProvider => _serviceProvider;
  
    /// <summary>
    /// Asynchronously runs the application by configuring services, initializing components, and preparing the application for use.
    /// </summary>
    /// <returns>A task that represents the asynchronous run operation.</returns>
    public async Task RunAsync()
    {
        
        //configure Dependency injection
        var services = new ServiceCollection();
        ServiceConfig.ConfigureServices(services);
        _serviceProvider = services.BuildServiceProvider();
        
        
        
        //configure the service locator pattern (just in case)
        ServiceLocator.Configure(_serviceProvider);
        
        
        using (var scope = services.BuildServiceProvider().CreateScope())
        {
            var logger = scope.ServiceProvider.GetRequiredService<ILogger>();
            var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<GainLabSQLDBContext>>();

            await using var db = await factory.CreateDbContextAsync();
            logger.Log("DbInit", $"SQLite path: {db.Database.GetDbConnection().DataSource}");
            await db.Database.MigrateAsync();
        }
        
        //initialize
        var initializer = _serviceProvider.GetRequiredService<SystemInitializer>();
        await initializer.InitializeAsync(_serviceProvider);
        
    }
}
