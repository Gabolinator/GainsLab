using System;
using System.Threading.Tasks;
namespace GainsLab.Models.Core.LifeCycle;

/// <summary>
/// Exposes lifecycle events and hooks for the desktop application host.
/// </summary>
public interface IAppLifeCycle
{
    /// <summary>
    /// Raised synchronously when the application starts.
    /// </summary>
    public event Action onAppStart;

    /// <summary>
    /// Raised asynchronously when the application starts.
    /// </summary>
    public event Func<Task>? onAppStartAsync;

    /// <summary>
    /// Raised synchronously when the application is about to exit.
    /// </summary>
    public event Action onAppExit;

    /// <summary>
    /// Raised asynchronously when the application is about to exit.
    /// </summary>
    public event Func<Task>? onAppExitAsync;

    /// <summary>
    /// Wires the lifecycle to the platform-specific host and prepares event subscriptions.
    /// </summary>
    /// <param name="serviceProvider">Configured dependency injection container.</param>
    /// <param name="lifetimeContext">Platform-specific lifetime object (Avalonia lifetime, MAUI app, etc.).</param>
    Task InitializeAsync(IServiceProvider serviceProvider, object? lifetimeContext);
    
    /// <summary>
    /// Explicitly triggers the start pipeline (useful for tests and CLI scenarios).
    /// </summary>
    Task OnStartAppAsync();

    /// <summary>
    /// Explicitly triggers the shutdown pipeline.
    /// </summary>
    Task OnExitAppAsync();
}
