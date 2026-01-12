namespace GainsLab.WebLayer.Model.Notification;

public enum ToastLevel { Info, Success, Warning, Error }

public enum ToastDisplayMode
{
    ShowAll,      // stack all immediately
    OneAtATime    // queue, show next only after previous closes
}

public sealed record ToastMessage(
    Guid Id,
    ToastLevel Level,
    string Title,
    string Message,
    int AutoHideMs = 4000)
{
    public static ToastMessage Create(ToastLevel level, string title, string message, int autoHideMs = 4000)
        => new(Guid.NewGuid(), level, title, message, autoHideMs);
}