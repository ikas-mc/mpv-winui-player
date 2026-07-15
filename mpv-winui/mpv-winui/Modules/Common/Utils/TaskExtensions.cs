using System;
using System.Threading.Tasks;

namespace mpv_winui.Modules.Common.Utils
{
    public static class TaskExtensions
    {
        extension(Task task)
        {
            public async void FireAndForget(Action<Exception>? onError = null)
            {
                try
                {
                    await task;
                }
                catch (Exception ex)
                {
                    onError?.Invoke(ex);
                }
            }
        }

        extension(ValueTask task)
        {
            public async void FireAndForget(Action<Exception>? onError = null)
            {
                try
                {
                    await task;
                }
                catch (Exception ex)
                {
                    onError?.Invoke(ex);
                }
            }
        }
    }
}