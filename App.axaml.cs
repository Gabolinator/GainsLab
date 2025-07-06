using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using GainsLab.Models.App;
using GainsLab.Models.Core.LifeCycle;
using Microsoft.Extensions.DependencyInjection;

namespace GainsLab;

public partial class App : Application
{
    
    //boot strapper service
    private AppHost _appHost;
    private IAppLifeCycle _lifecycle;
    private Window? mainWindow;

   
    
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override async void OnFrameworkInitializationCompleted()
    {
        
    
        await OnAppStart();
        
        
        base.OnFrameworkInitializationCompleted();
    }

   

    private  async Task OnAppStart()
    {
        // Initialize AppHost and its dependencies
        _appHost = new AppHost();
        await _appHost.RunAsync();
        
        _lifecycle = _appHost.ServiceProvider.GetRequiredService<IAppLifeCycle>();
       
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            mainWindow = desktop.MainWindow = _appHost.ServiceProvider.GetRequiredService<MainWindow>();
        }

        await _lifecycle.InitializeAsync(_appHost.ServiceProvider, ApplicationLifetime);

        
        await _lifecycle.OnStartAppAsync();
    }

   
}