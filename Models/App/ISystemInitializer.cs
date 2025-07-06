using System;
using System.Threading.Tasks;

namespace GainsLab.Models.App;

/// <summary>
/// Initializes the system by setting up essential services and loading data.
/// </summary>
public interface ISystemInitializer
{
    
    
    /// <summary>
    /// Asynchronously initializes the system by initializing services and loading cached data.
    /// </summary>
    /// <returns>A task that represents the asynchronous initialization operation.</returns>
    public Task InitializeAsync(IServiceProvider serviceProvider);
}