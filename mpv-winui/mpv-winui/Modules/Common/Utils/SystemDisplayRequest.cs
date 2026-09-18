using System;
using Windows.Win32;
using Windows.Win32.System.Power;

namespace mpv_winui.Modules.Common.Utils
{
    public class SystemDisplayRequest
    {
        private bool _enabled;

        public void Enable()
        {
            if (_enabled)
            {
                return;
            }

            try
            {
                var result = PInvoke.SetThreadExecutionState(EXECUTION_STATE.ES_CONTINUOUS | EXECUTION_STATE.ES_SYSTEM_REQUIRED | EXECUTION_STATE.ES_DISPLAY_REQUIRED);
                if (result != 0)
                {
                    _enabled = true;
                }
            }
            catch (Exception)
            {
                //
            }
        }

        public void Disable()
        {
            if (!_enabled)
            {
                return;
            }

            try
            {
                var result = PInvoke.SetThreadExecutionState(EXECUTION_STATE.ES_CONTINUOUS);
                if (result != 0)
                {
                    _enabled = false;
                }
            }
            catch (Exception)
            {
                //
            }
        }
    }
}