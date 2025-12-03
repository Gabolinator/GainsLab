using GainsLab.Models.App;
using GainsLab.Models.Core.LifeCycle;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Graphics;

namespace GainsLab.Maui;

public partial class App : Application
{
    private AppHost? _appHost;
    private IAppLifeCycle? _lifeCycle;

    public App()
    {
        InitializeComponent();
        MainPage = BuildLoadingPage();
        _ = InitializeAsync();
    }

    private Page BuildLoadingPage() =>
        new ContentPage
        {
            Padding = new Thickness(24),
            Content = new VerticalStackLayout
            {
                Spacing = 8,
                Children =
                {
                    new ActivityIndicator { IsRunning = true, HorizontalOptions = LayoutOptions.Center },
                    new Label
                    {
                        Text = "Starting GainsLab...",
                        HorizontalTextAlignment = TextAlignment.Center
                    }
                }
            }
        };

    private async Task InitializeAsync()
    {
        try
        {
            _appHost = new AppHost();
            await _appHost.RunAsync();

            _lifeCycle = _appHost.ServiceProvider.GetRequiredService<IAppLifeCycle>();
            await _lifeCycle.InitializeAsync(_appHost.ServiceProvider, this);

            var mainPage = _appHost.ServiceProvider.GetRequiredService<MainPage>();
            MainPage = mainPage;

            await _lifeCycle.OnStartAppAsync();
        }
        catch (Exception ex)
        {
            MainPage = new ContentPage
            {
                Padding = new Thickness(24),
                Content = new Label
                {
                    Text = $"Failed to start GainsLab: {ex.Message}",
                    TextColor = Colors.Red,
                    HorizontalTextAlignment = TextAlignment.Center
                }
            };
        }
    }
}
