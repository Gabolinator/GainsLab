using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Avalonia.Controls.ApplicationLifetimes;
using GainsLab.Models.DataManagement.DB;
using GainsLab.Models.Logging;

namespace GainsLab.Models.Core.LifeCycle;

public class AppLifecycleService  : IAppLifeCycle
{
    private readonly IWorkoutLogger _logger;
    private IDataProvider _dataProvider;

    public event Action onAppStart;
    public event Func<Task>? onAppStartAsync;
    public event Action onAppExit;
    public event Func<Task>? onAppExitAsync;

    public AppLifecycleService(IWorkoutLogger logger)
    {
        _logger = logger;
      
    }

    public async Task InitializeAsync( IServiceProvider serviceProvider, IApplicationLifetime? lifetime)
    {
        if (lifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
           // desktop.MainWindow.Closed += OnMainWindowClosed;
            desktop.ShutdownRequested += OnShutdownResquested;
        }
        
        
    }

    private void OnShutdownResquested(object? sender, ShutdownRequestedEventArgs e)
    {
       
       OnExitAppAsync().GetAwaiter().GetResult();
    }

    private void OnMainWindowClosed(object? sender, EventArgs e)
    {
      //  OnExitAppAsync().GetAwaiter().GetResult();
    }


    public async Task OnStartAppAsync()
    {
        _logger.Log("AppLifecycle", "Application has started.");
        onAppStart?.Invoke();
        onAppStartAsync?.Invoke();
        await Task.CompletedTask;
    }

    public async Task OnExitAppAsync()
    {
        _logger.Log("AppLifecycle", "Application is shutting down...");
        
        onAppExit?.Invoke();
        onAppExitAsync?.Invoke();
        _logger.Log("AppLifecycle", "Shutdown tasks completed.");
    }
}