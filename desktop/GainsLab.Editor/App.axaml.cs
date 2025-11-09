using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using GainsLab.Core.Models.Core.Interfaces.DataManagement;
using GainsLab.Core.Models.Core.Results;
using GainsLab.Models.App;
using GainsLab.Models.Core.LifeCycle;
using GainsLab.Models.DataManagement;
using Microsoft.Extensions.DependencyInjection;
using Npgsql.Replication.TestDecoding;

namespace GainsLab;

/// <summary>
/// Avalonia application bootstrapper responsible for initializing the editor and wiring startup logic.
/// </summary>
public partial class App : Application
{
    
    //boot strapper service
    private AppHost _appHost;
    private IAppLifeCycle _lifecycle;
    private Window? mainWindow;

   
    
    /// <summary>
    /// Loads the XAML resources for the application.
    /// </summary>
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    /// <summary>
    /// Runs after Avalonia finishes initializing and triggers application startup logic.
    /// </summary>
    public override async void OnFrameworkInitializationCompleted()
    {
        
    
        await OnAppStart();
        
        
        base.OnFrameworkInitializationCompleted();
    }

   
    /// <summary>
    /// Configures dependency injection, resolves the main window, and invokes lifecycle hooks.
    /// </summary>
    private  async Task OnAppStart()
    {
        // Initialize AppHost and its dependencies
        _appHost = new AppHost();
        await _appHost.RunAsync();
        
        _lifecycle = _appHost.ServiceProvider.GetRequiredService<IAppLifeCycle>();
       
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            mainWindow = desktop.MainWindow = _appHost.ServiceProvider.GetRequiredService<MainWindow>();
            if (mainWindow is MainWindow main)
            {

                var dataManager = _appHost.ServiceProvider.GetRequiredService<IDataManager>();
                main.SetOnClick(dataManager.CreateLocalDataAsync);
            }
        }

        await _lifecycle.InitializeAsync(_appHost.ServiceProvider, ApplicationLifetime);

        
        await _lifecycle.OnStartAppAsync();
    }

   
}
