using Microsoft.UI.Dispatching;

namespace mpv_winui.Modules.Common.Utils
{
    public static class DispatcherExtensions
    {
        public static void RunAsync(this DispatcherQueue? dispatcher, DispatcherQueueHandler action, DispatcherQueuePriority priority = DispatcherQueuePriority.Normal)
        {
            if (dispatcher is null)
            {
                return;
            }
            if (dispatcher.HasThreadAccess)
            {
                action();
            }
            else
            {
                dispatcher.TryEnqueue(priority, action);
            }
        }
    }
}
