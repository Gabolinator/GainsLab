using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;

namespace GainsLab.Models.App;

public class AppHost
{
    
    private IServiceProvider _serviceProvider;
    public Window? MainWindow => _serviceProvider.GetRequiredService<MainWindow>();


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
        await initializer.InitializeAsync();
        
    }
}