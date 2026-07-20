using Microsoft.UI.Dispatching;
using System;

namespace mpv_winui.Modules.Common.Utils
{
    public partial class DispatcherTimerDebouncer<T> : IDisposable
    {
        private DispatcherQueueTimer? _timer;
        private Action<T>? _action;
        private T? _latestData;
        private bool _isDisposed = false;

        public DispatcherTimerDebouncer(DispatcherQueue dispatcherQueue, TimeSpan delay, Action<T> action)
        {
            _action = action ?? throw new ArgumentNullException(nameof(action));
            _timer = dispatcherQueue.CreateTimer();
            _timer.Interval = delay;
            _timer.Tick += Timer_Tick;
        }

        private void Timer_Tick(DispatcherQueueTimer sender, object args)
        {
            if (_isDisposed || _timer == null)
            {
                return;
            }

            _timer?.Stop();
            _action?.Invoke(_latestData!);
        }

        public void OnEvent(T data)
        {
            if (_isDisposed || _timer == null)
            {
                return;
            }

            _latestData = data;
            _timer?.Stop();
            _timer?.Start();
        }

        public void Cancel()
        {
            if (_isDisposed)
            {
                return;
            }

            _timer?.Stop();
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }


        private void Dispose(bool disposing)
        {
            if (_isDisposed)
            {
                return;
            }

            if (disposing)
            {
                if (_timer != null)
                {
                    _timer.Stop();
                    _timer.Tick -= Timer_Tick;
                    _timer = null;
                }

                _action = null;
                _latestData = default;
            }

            _isDisposed = true;
        }

        ~DispatcherTimerDebouncer()
        {
            Dispose(disposing: false);
        }
    }
}
