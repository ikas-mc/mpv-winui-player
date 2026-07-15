using System;
using Windows.System.Threading;

namespace mpv_winui.Modules.Common.Utils
{
    public static class DebounceUtil
    {
        public static Action Debounce(Action action, TimeSpan delay)
        {
            ThreadPoolTimer? timer = null;
            return () =>
            {
                timer?.Cancel();
                timer = ThreadPoolTimer.CreateTimer((sender) =>
                {
                    if (Object.ReferenceEquals(sender, timer))
                    {
                        action();
                    }
                }, delay);
            };
        }

        public static Action<T> Debounce<T>(Action<T> action, TimeSpan delay)
        {
            ThreadPoolTimer? timer = null;
            return (arg) =>
            {
                timer?.Cancel();
                timer = ThreadPoolTimer.CreateTimer((sender) =>
                {
                    if (Object.ReferenceEquals(sender, timer))
                    {
                        action(arg);
                    }
                }, delay);
            };
        }
    }
}
