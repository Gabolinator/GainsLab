using GainsLab.Core.Models.Core.Interfaces.DataManagement;
using GainsLab.Core.Models.Core.Results;
using ILogger = GainsLab.Core.Models.Core.Utilities.Logging.ILogger;

namespace GainsLab.Maui;

public partial class MainPage : ContentPage
{
    private readonly ILogger _logger;
    private readonly IDataManager _dataManager;

    public MainPage(ILogger logger, IDataManager dataManager)
    {
        _logger = logger;
        _dataManager = dataManager;
        InitializeComponent();
    }

    private async void OnButtonClick(object? sender, EventArgs e)
    {
        MessageLabel.Text = "Processing...";
        _logger.Log(nameof(MainPage), "Clicked button - initiating local data creation.");

        Result? result = null;
        try
        {
            result = await _dataManager.CreateLocalDataAsync();
        }
        catch (Exception ex)
        {
            MessageLabel.Text = "Unexpected error.";
            _logger.LogError(nameof(MainPage), $"CreateLocalDataAsync threw: {ex}");
            return;
        }

        if (result.Success)
        {
            MessageLabel.Text = "You clicked the button!";
            _logger.Log(nameof(MainPage), "Clicked button - success");
        }
        else
        {
            var message = result.GetErrorMessage() ?? "Unknown failure.";
            MessageLabel.Text = "Click failed.";
            _logger.LogWarning(nameof(MainPage), $"Clicked button - failed: {message}");
        }
    }
}
