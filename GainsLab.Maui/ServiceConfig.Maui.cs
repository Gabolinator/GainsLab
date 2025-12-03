using GainsLab.Maui.Lifecycle;
using GainsLab.Models.Core.LifeCycle;
using Microsoft.Extensions.DependencyInjection;

namespace GainsLab.Models.App;

/// <summary>
/// MAUI-specific service registrations that plug UI components and lifecycle hooks into the shared container.
/// </summary>
public static partial class ServiceConfig
{
    static partial void ConfigurePlatformServices(IServiceCollection services)
    {
        services.AddSingleton<IAppLifeCycle, MauiAppLifecycleService>();
        services.AddSingleton<MainPage>();
    }
}
