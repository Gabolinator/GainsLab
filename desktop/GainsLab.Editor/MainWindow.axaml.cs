using Avalonia.Controls;
using Avalonia.Interactivity;
using GainsLab.Core.Models.Core.Factory;
using GainsLab.Models.Factory;
using GainsLab.Models.Logging;
using Microsoft.Extensions.Logging;
using ILogger = GainsLab.Models.Logging.ILogger;


namespace GainsLab;

public partial class MainWindow : Window
{
    private readonly ILogger _logger;
   

    public MainWindow(ILogger logger)
    {
        _logger = logger;
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
    }
}