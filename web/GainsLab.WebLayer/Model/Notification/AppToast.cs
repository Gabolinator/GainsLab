namespace GainsLab.WebLayer.Model.Notification;

public sealed class AppToast : IToast
{
    private readonly ToastService _toasts;

   
    private const int ErrorMs   = 6000;
    private const int WarningMs = 5000;
    private const int InfoMs    = 4000;
    private const int SuccessMs = 3500;

    public AppToast(ToastService toasts) => _toasts = toasts;

    public void SetMode(ToastDisplayMode mode) => _toasts.DisplayMode = mode;

   
    public void Errors(IEnumerable<string> messages, string? title = null)
        => AddBatch(ToastLevel.Error, messages, title ?? "Error", ErrorMs);

    public void Warnings(IEnumerable<string> messages, string? title = null)
        => AddBatch(ToastLevel.Warning, messages, title ?? "Warning", WarningMs);

    public void Infos(IEnumerable<string> messages, string? title = null)
        => AddBatch(ToastLevel.Info, messages, title ?? "Info", InfoMs);

    public void Successes(IEnumerable<string> messages, string? title = null)
        => AddBatch(ToastLevel.Success, messages, title ?? "Success", SuccessMs);
    
    public void Error(string message, string? title = null)
        => AddOne(ToastLevel.Error, title ?? "Error", message, ErrorMs);

    public void Warning(string message, string? title = null)
        => AddOne(ToastLevel.Warning, title ?? "Warning", message, WarningMs);

    public void Info(string message, string? title = null)
        => AddOne(ToastLevel.Info, title ?? "Info", message, InfoMs);

    public void Success(string message, string? title = null)
        => AddOne(ToastLevel.Success, title ?? "Success", message, SuccessMs);

 
    private void AddBatch(ToastLevel level, IEnumerable<string> messages, string title, int autoHideMs)
    {
        if (messages is null) return;

        foreach (var m in messages)
        {
            if (string.IsNullOrWhiteSpace(m)) continue;
            AddOne(level, title, m.Trim(), autoHideMs);
        }
    }

    private void AddOne(ToastLevel level, string title, string message, int autoHideMs)
    {
        if (string.IsNullOrWhiteSpace(message)) return;

        _toasts.Show(ToastMessage.Create(level, title, message.Trim(), autoHideMs));
    }
}
