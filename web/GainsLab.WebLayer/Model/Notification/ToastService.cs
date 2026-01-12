namespace GainsLab.WebLayer.Model.Notification;


public sealed class ToastService
{
private readonly object _lock = new();

    private readonly Queue<ToastMessage> _queue = new();
    private bool _isShowingOne; // only relevant for OneAtATime mode

    public ToastDisplayMode DisplayMode { get; set; } = ToastDisplayMode.ShowAll;

    // Host subscribes to this: "show this toast now"
    public event Action<ToastMessage>? OnShow;

    //Host can clear UI when service clears
    public event Action? OnClear;
    
    public void Show(ToastMessage toast)
    {
        lock (_lock)
        {
            if (DisplayMode == ToastDisplayMode.ShowAll)
            {
                OnShow?.Invoke(toast);
                return;
            }

            // OneAtATime
            _queue.Enqueue(toast);

            // If nothing currently displayed, show immediately
            if (!_isShowingOne)
            {
                _isShowingOne = true;
                OnShow?.Invoke(_queue.Dequeue());
            }
        }
    }

    // Called by the Host when the currently displayed toast is dismissed/auto-hidden
    public void NotifyDismissed(Guid toastId)
    {
        lock (_lock)
        {
            if (DisplayMode != ToastDisplayMode.OneAtATime)
                return;

            if (_queue.Count > 0)
            {
                OnShow?.Invoke(_queue.Dequeue());
            }
            else
            {
                _isShowingOne = false;
            }
        }
    }

    public void Clear()
    {
        lock (_lock)
        {
            _queue.Clear();
            _isShowingOne = false;
        }
        OnClear?.Invoke();
    }

  
    public void Errors(IEnumerable<string> messages, string? title = null, int autoHideMs = 6000)
        => Batch(ToastLevel.Error, messages, title ?? "Error", autoHideMs);

    public void Warnings(IEnumerable<string> messages, string? title = null, int autoHideMs = 5000)
        => Batch(ToastLevel.Warning, messages, title ?? "Warning", autoHideMs);

    public void Infos(IEnumerable<string> messages, string? title = null, int autoHideMs = 4000)
        => Batch(ToastLevel.Info, messages, title ?? "Info", autoHideMs);

    public void Successes(IEnumerable<string> messages, string? title = null, int autoHideMs = 3500)
        => Batch(ToastLevel.Success, messages, title ?? "Success", autoHideMs);

    private void Batch(ToastLevel level, IEnumerable<string> messages, string title, int autoHideMs)
    {
        if (messages is null || !messages.Any()) return;

        foreach (var msg in messages)
        {
            if (string.IsNullOrWhiteSpace(msg)) continue;
            Show(ToastMessage.Create(level, title, msg.Trim(), autoHideMs));
        }
    }
}