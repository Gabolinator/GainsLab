namespace GainsLab.Application.Interfaces;

/// <summary>
/// Exposes lifecycle events and hooks.
/// </summary>
public interface ILifeCycle
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
    /// Explicitly triggers the start pipeline (useful for tests and CLI scenarios).
    /// </summary>
    Task OnStartAppAsync();

    /// <summary>
    /// Explicitly triggers the shutdown pipeline.
    /// </summary>
    Task OnExitAppAsync();
}