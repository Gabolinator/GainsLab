using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using GainsLab.Models.App;

namespace GainsLab;

public partial class App : Application
{
    
    //boot strapper service
    private AppHost _appHost;
    
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override async void OnFrameworkInitializationCompleted()
    {
        
        // Initialize AppHost and its dependencies
        _appHost = new AppHost();
        await _appHost.RunAsync();
        
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = _appHost.MainWindow;
        }

        base.OnFrameworkInitializationCompleted();
    }
}