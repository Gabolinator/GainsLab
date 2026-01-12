namespace GainsLab.WebLayer.Model.Notification.Confirmation;

public sealed record ConfirmRequest(
    string Title, 
    string Message, 
    string ConfirmText = "Confirm", 
    string CancelText = "Cancel",
    bool IsDanger = false);
