using Avalonia;
using System;

namespace GainsLab;

/// <summary>
/// Entry point for the GainsLab desktop editor.
/// </summary>
class Program
{
    /// <summary>
    /// Main entry point for the application. Initializes Avalonia and starts the desktop lifetime.
    /// </summary>
    [STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);

    /// <summary>
    /// Configures the Avalonia application builder.
    /// </summary>
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}
