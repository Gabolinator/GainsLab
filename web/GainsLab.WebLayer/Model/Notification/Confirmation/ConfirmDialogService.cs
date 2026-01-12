namespace GainsLab.WebLayer.Model.Notification.Confirmation;

public class ConfirmDialogService : IConfirmDialog
{
    public event Func<ConfirmRequest, Task<bool>>? OnShow;
    
    public Task<bool> ShowAsync(ConfirmRequest request)
        => OnShow is null
            ? Task.FromResult(false)
            : OnShow.Invoke(request);
    
    
}