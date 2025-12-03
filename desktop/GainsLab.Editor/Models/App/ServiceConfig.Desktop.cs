using GainsLab.Models.App.LifeCycle;
using GainsLab.Models.Core.LifeCycle;
using Microsoft.Extensions.DependencyInjection;

namespace GainsLab.Models.App;

/// <summary>
/// Desktop-specific service registrations for the editor host.
/// </summary>
public static partial class ServiceConfig
{
    static partial void ConfigurePlatformServices(IServiceCollection services)
    {
        services.AddSingleton<IAppLifeCycle, AppLifecycleService>();
        services.AddSingleton<MainWindow>();
    }
}
