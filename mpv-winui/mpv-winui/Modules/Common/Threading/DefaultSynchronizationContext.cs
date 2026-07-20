using Microsoft.UI.Dispatching;
using System;
using System.Threading;

namespace mpv_winui.Modules.Common.Threading
{
    public class DefaultSynchronizationContext : SynchronizationContext
    {
        public Func<Exception, bool>? OnException
        {
            get; set;
        }

        private readonly DispatcherQueue m_dispatcherQueue;

        public DefaultSynchronizationContext(DispatcherQueue dispatcherQueue)
        {
            ArgumentNullException.ThrowIfNull(dispatcherQueue);

            m_dispatcherQueue = dispatcherQueue;
        }

        public override void Post(SendOrPostCallback d, object? state)
        {
            if (d == null)
            {
                throw new ArgumentNullException("d");
            }

            m_dispatcherQueue.TryEnqueue(delegate
            {
                try
                {
                    d(state);
                }
                catch (Exception ex)
                {
                    if (OnException?.Invoke(ex) != true)
                    {
                        WinRT.ExceptionHelpers.ReportUnhandledError(ex);
                    }
                }
            });
        }

        public override void Send(SendOrPostCallback d, object? state)
        {
            throw new NotSupportedException("The send method is not supported, use Post instead.");
        }

        public override SynchronizationContext CreateCopy()
        {
            return new DispatcherQueueSynchronizationContext(m_dispatcherQueue);
        }
    }
}