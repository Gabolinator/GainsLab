namespace GainsLab.WebLayer.Model.Notification.Confirmation;

public interface IConfirmDialog
{
    public Task<bool> ShowAsync(ConfirmRequest confirmationRequest);
}