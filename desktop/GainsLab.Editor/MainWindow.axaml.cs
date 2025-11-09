using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml.MarkupExtensions;
using GainsLab.Core.Models.Core.Factory;
using GainsLab.Core.Models.Core.Results;
using GainsLab.Models.Factory;
using Microsoft.Extensions.Logging;
using ILogger = GainsLab.Core.Models.Core.Utilities.Logging.ILogger;


namespace GainsLab;

/// <summary>
/// Primary desktop window for the GainsLab editor.
/// </summary>
public partial class MainWindow : Window
{
    private readonly ILogger _logger;
   
    private event Func<Task<Result>> OnClick;
      

    /// <summary>
    /// Initializes a new instance of the <see cref="MainWindow"/> class.
    /// </summary>
    /// <param name="logger">Logger used to record window lifecycle events.</param>
    public MainWindow(ILogger logger)
    {
        _logger = logger;
        InitializeComponent();
        this.Show();      // Forces the window to show
        this.Activate();  // Brings it to front
        
       _logger.Log(nameof(MainWindow), "Main Window created ");
    }

    /// <inheritdoc />
    public sealed override void Show()
    {
        base.Show();
    }

    /// <summary>
    /// Handles the sample button click event by updating the message text.
    /// </summary>
    private async void OnButtonClick(object? sender, RoutedEventArgs e)
    {
        MessageText.Text = "You clicked the button!";
        _logger.Log(nameof(MainWindow), $"Clicked button - Testing seeding remote repo");

        
        var clickTask = OnClick?.Invoke();
        if (clickTask != null)
        {
            var result = await clickTask;
            if(result.Success) _logger.Log(nameof(MainWindow), $"Clicked button - Testing seeding remote repo - success");
            else _logger.LogWarning(nameof(MainWindow), $"Clicked button - Testing seeding remote repo - failed : {result.GetErrorMessage()}");

            return;
        }

        _logger.LogWarning(nameof(MainWindow), $"Clicked button - Testing seeding remote repo - No on click delegate");

      
        
    }


    public void SetOnClick(Func<Task<Result>> onClick)
    {
        OnClick = onClick;
    }
}
