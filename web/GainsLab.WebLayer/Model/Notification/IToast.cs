using GainsLab.Application.Results;

namespace GainsLab.WebLayer.Model.Notification;



public partial interface IToast
{
    void Success(string message, string? title = null);
    void Info(string message, string? title = null);
    void Warning(string message, string? title = null);
    void Error(string message, string? title = null);
    void Errors(IEnumerable<string> messages, string? title = null);
    void Warnings(IEnumerable<string> messages, string? title = null);
    void Infos(IEnumerable<string> messages, string? title = null);
    void Successes(IEnumerable<string> messages, string? title = null);
}