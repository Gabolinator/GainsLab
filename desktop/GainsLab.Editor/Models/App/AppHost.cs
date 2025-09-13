using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;

namespace GainsLab.Models.App;


/// <summary>
/// Hosts and initializes the application, setting up dependency injection and running startup tasks.
/// </summary>
public class AppHost
{
    
    private IServiceProvider _serviceProvider;
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
        
        //initialize
        var initializer = _serviceProvider.GetRequiredService<SystemInitializer>();
        await initializer.InitializeAsync(_serviceProvider);
        
    }
}