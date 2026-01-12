using GainsLab.Application.Results;

namespace GainsLab.WebLayer.Model.Notification;

public static partial class ToastExtension
{
   public static void FromMessages(this IToast toast,MessagesContainer messages ,string? title = null, params ToastLevel[] toastLevels)
   {
      //assume all
      var set = toastLevels.Length == 0 ? 
         new (){ToastLevel.Info,ToastLevel.Success, ToastLevel.Warning, ToastLevel.Error}
         :new HashSet<ToastLevel>(toastLevels);
      
      if(set.Contains(ToastLevel.Error)) toast.Errors(messages.Get(MessageType.Error));
      if(set.Contains(ToastLevel.Warning))toast.Warnings(messages.Get(MessageType.Warning));
      if(set.Contains(ToastLevel.Success) || set.Contains(ToastLevel.Success))toast.Successes(messages.Get(MessageType.Info));
   }
}