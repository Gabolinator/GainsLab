using Avalonia.Controls;
using Avalonia.Interactivity;
using GainsLab.Models.Factory;
using GainsLab.Models.Logging;
using Microsoft.Extensions.Logging;
using ILogger = GainsLab.Models.Logging.ILogger;


namespace GainsLab;

public partial class MainWindow : Window
{
    private readonly ILogger _logger;
    private readonly ComponentFactory _componentFactory;

    public MainWindow(ILogger logger, ComponentFactory componentFactory)
    {
        _logger = logger;
        _componentFactory = componentFactory;
        InitializeComponent();
        this.Show();      // Forces the window to show
        this.Activate();  // Brings it to front
       _logger.Log(nameof(MainWindow), "Main Window created ");
    }

    public sealed override void Show()
    {
        base.Show();
    }

    private void OnButtonClick(object? sender, RoutedEventArgs e)
    {
        MessageText.Text = "You clicked the button!";
        _componentFactory.CreateTestData();
    }
}