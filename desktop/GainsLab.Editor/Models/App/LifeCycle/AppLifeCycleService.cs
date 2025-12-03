using System;
using System.Threading.Tasks;
using Avalonia.Controls.ApplicationLifetimes;
using GainsLab.Core.Models.Core.Interfaces.DataManagement;
using GainsLab.Core.Models.Core.Utilities.Logging;
using GainsLab.Models.Core.LifeCycle;
using GainsLab.Models.DataManagement.DB;

namespace GainsLab.Models.App.LifeCycle;

/// <summary>
/// Default lifecycle implementation that coordinates startup and shutdown events for the desktop host.
/// </summary>
public class AppLifecycleService  : IAppLifeCycle
{
    private readonly ILogger _logger;
    private IDataProvider _dataProvider;

    /// <inheritdoc />
    public event Action onAppStart;
    /// <inheritdoc />
    public event Func<Task>? onAppStartAsync;
    /// <inheritdoc />
    public event Action onAppExit;
    /// <inheritdoc />
    public event Func<Task>? onAppExitAsync;

    /// <summary>
    /// Initializes a new instance of the <see cref="AppLifecycleService"/> class.
    /// </summary>
    /// <param name="logger">Logger used to record lifecycle events.</param>
    public AppLifecycleService(ILogger logger)
    {
        _logger = logger;
      
    }

    /// <inheritdoc />
    public async Task InitializeAsync(IServiceProvider serviceProvider, object? lifetimeContext)
    {
        if (lifetimeContext is IClassicDesktopStyleApplicationLifetime desktop)
        {
           // desktop.MainWindow.Closed += OnMainWindowClosed;
            desktop.ShutdownRequested += OnShutdownResquested;
        }
        
        
    }

    /// <summary>
    /// Handles shutdown requests by executing the asynchronous exit pipeline synchronously.
    /// </summary>
    private void OnShutdownResquested(object? sender, ShutdownRequestedEventArgs e)
    {
       
       OnExitAppAsync().GetAwaiter().GetResult();
    }

    private void OnMainWindowClosed(object? sender, EventArgs e)
    {
      //  OnExitAppAsync().GetAwaiter().GetResult();
    }


    /// <inheritdoc />
    public async Task OnStartAppAsync()
    {
        _logger.Log("AppLifecycle", "Application has started.");
        onAppStart?.Invoke();
        onAppStartAsync?.Invoke();
        await Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task OnExitAppAsync()
    {
        _logger.Log("AppLifecycle", "Application is shutting down...");
        
        onAppExit?.Invoke();
        onAppExitAsync?.Invoke();
        _logger.Log("AppLifecycle", "Shutdown tasks completed.");
    }
}
