using System;
using System.Threading.Tasks;
using GainsLab.Models.Core.LifeCycle;
using ILogger = GainsLab.Core.Models.Core.Utilities.Logging.ILogger;

namespace GainsLab.Maui.Lifecycle;

/// <summary>
/// Wires shared lifecycle hooks into the MAUI application host.
/// </summary>
public class MauiAppLifecycleService : IAppLifeCycle
{
    private readonly ILogger _logger;
    private bool _shutdownRequested;

    public event Action onAppStart = delegate { };
    public event Func<Task>? onAppStartAsync;
    public event Action onAppExit = delegate { };
    public event Func<Task>? onAppExitAsync;

    public MauiAppLifecycleService(ILogger logger)
    {
        _logger = logger;
    }

    public Task InitializeAsync(IServiceProvider serviceProvider, object? lifetimeContext)
    {
        AppDomain.CurrentDomain.ProcessExit += OnProcessExit;

        return Task.CompletedTask;
    }

    public async Task OnStartAppAsync()
    {
        _logger.Log(nameof(MauiAppLifecycleService), "Application has started.");
        onAppStart?.Invoke();
        if (onAppStartAsync is not null)
        {
            await onAppStartAsync.Invoke();
        }
    }

    public async Task OnExitAppAsync()
    {
        if (_shutdownRequested) return;
        _shutdownRequested = true;

        _logger.Log(nameof(MauiAppLifecycleService), "Application is shutting down...");
        onAppExit?.Invoke();
        if (onAppExitAsync is not null)
        {
            await onAppExitAsync.Invoke();
        }
        _logger.Log(nameof(MauiAppLifecycleService), "Shutdown tasks completed.");
    }

    private void OnProcessExit(object? sender, EventArgs e)
    {
        try
        {
            OnExitAppAsync().GetAwaiter().GetResult();
        }
        catch
        {
            // ignored – best effort on process exit
        }
    }
}
