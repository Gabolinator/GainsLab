using System;
using Microsoft.Extensions.DependencyInjection;

namespace GainsLab.Models.App;

public static class ServiceLocator
{
    private static IServiceProvider? _provider;
    public static void Configure(IServiceProvider provider) => _provider = provider;

    public static T Get<T>() => _provider!.GetRequiredService<T>();
}