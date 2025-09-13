using System;
using Microsoft.Extensions.DependencyInjection;

namespace GainsLab.Models.App;

/// <summary>
/// Provides a simple service locator for accessing registered services from the dependency injection container.
/// </summary>
public static class ServiceLocator
{
    private static IServiceProvider? _provider;
    
    /// <summary>
    /// Configures the service locator with the specified <see cref="IServiceProvider"/>.
    /// </summary>
    /// <param name="provider">The service provider to use for resolving services.</param>
    public static void Configure(IServiceProvider provider) => _provider = provider;

    
    /// <summary>
    /// Retrieves a service of the specified type from the service provider.
    /// </summary>
    /// <typeparam name="T">The type of the service to retrieve.</typeparam>
    /// <returns>The requested service instance.</returns>
    public static T GetService<T>() => _provider!.GetRequiredService<T>();
}