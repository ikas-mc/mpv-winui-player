using Microsoft.Graphics.Display;
using Microsoft.UI.Windowing;
using mpv_winui.Modules.Common.Utils;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;

namespace mpv_winui.Modules.Player
{
    public sealed partial class MpvPlayerPage
    {
        private DisplayInformation? _displayInfo;
        private mpv_winrt.DisplayColorKind _lastColorKind = mpv_winrt.DisplayColorKind.SDR;
        private const uint _defaultRefreshRate = 60;
        private uint _lastRefreshRate = _defaultRefreshRate;
        private HMONITOR? _lastMonitor;
        private DispatcherTimerDebouncer<int>? _displayInfoDebouncer;

        public void InitDisplayInfo()
        {
            //TODO use player view rect
            _displayInfo = DisplayInformation.CreateForWindowId(_appWindow.Id);
            _lastColorKind = ReadColorKind();
            _displayInfo.AdvancedColorInfoChanged += OnAdvancedColorInfoChanged;

            _displayInfoDebouncer = new(DispatcherQueue, TimeSpan.FromSeconds(1), CheckAndUpdateDisplayInfo);
            _lastMonitor = Win32WindowHelper.GetMonitor(App.Window!);
            _lastRefreshRate = ReadRefreshRate();
            //_appWindow.Changed += OnDisplayAppWindowChanged;

            unsafe
            {
                //TODO move to Window
                var hwnd = Win32WindowHelper.GetHwnd(App.Window!);
                PInvoke.SetWindowSubclass(new HWND(hwnd), &SubclassWindowProc, 52120, 0);
            }
        }

        public void CleanupDisplayInfo()
        {
            //_appWindow?.Changed -= OnDisplayAppWindowChanged;

            if (_displayInfo is { } displayInfo)
            {
                try
                {
                    _displayInfo = null;
                    displayInfo?.AdvancedColorInfoChanged -= OnAdvancedColorInfoChanged;
                    displayInfo?.Dispose();
                }
                catch (Exception)
                {

                }
            }

            if (_displayInfoDebouncer is { } debouncer)
            {
                try
                {
                    _displayInfoDebouncer = null;
                    debouncer?.Dispose();
                }
                catch (Exception)
                {

                }
            }

            unsafe
            {
                var hwnd = Win32WindowHelper.GetHwnd(App.Window!);
                PInvoke.RemoveWindowSubclass(new HWND(hwnd), &SubclassWindowProc, 52120);
            }
        }

        private mpv_winrt.DisplayColorKind ReadColorKind()
        {
            try
            {
                var colorInfo = _displayInfo?.GetAdvancedColorInfo();
                if (colorInfo != null)
                {
                    return colorInfo.CurrentAdvancedColorKind switch
                    {
                        DisplayAdvancedColorKind.HighDynamicRange => mpv_winrt.DisplayColorKind.HDR,
                        DisplayAdvancedColorKind.WideColorGamut => mpv_winrt.DisplayColorKind.WCG,
                        _ => mpv_winrt.DisplayColorKind.SDR
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
            return mpv_winrt.DisplayColorKind.SDR;
        }

        private uint ReadRefreshRate()
        {
            if (_lastMonitor is { IsNull: false } monitor)
            {
                try
                {
                    return Win32WindowHelper.GetDisplayFrequency(monitor, _defaultRefreshRate);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex);
                }
            }
            return _defaultRefreshRate;
        }

        private void OnAdvancedColorInfoChanged(DisplayInformation sender, object args)
        {
            var newKind = ReadColorKind();
            if (newKind != _lastColorKind)
            {
                _lastColorKind = newKind;
                _mediaPlayer?.UpdateDisplayColorInfo(newKind);
            }
        }

        private void OnDisplayAppWindowChanged(AppWindow sender, AppWindowChangedEventArgs args)
        {
            if (!args.DidPositionChange)
            {
                return;
            }

            _displayInfoDebouncer?.OnEvent(0);
        }

        private void CheckAndUpdateDisplayInfo(int type)
        {
            var monitor = Win32WindowHelper.GetMonitor(App.Window!);
            if (_logger.IsTraceEnabled)
            {
                _logger.Trace("display check, monitor={}", monitor.ToString());
            }

            if (type > 1 || _lastMonitor != monitor)
            {
                _lastMonitor = monitor;
                var rate = ReadRefreshRate();
                if (_logger.IsDebugEnabled)
                {
                    _logger.Trace("display update, last monitor={},,lastRefreshRate={}, new monitor={}, newRefreshRates={}", monitor.ToString(), _lastRefreshRate, monitor.ToString(), rate);
                }
                if (rate != _lastRefreshRate)
                {
                    _lastRefreshRate = rate;
                    _mediaPlayer?.UpdateDisplayRefreshRate(rate);
                }
            }
        }

        [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
        private unsafe static LRESULT SubclassWindowProc(HWND hWnd, uint uMsg, WPARAM wParam, LPARAM lParam, nuint uIdSubclass, nuint dwRefData)
        {
            switch (uMsg)
            {
                case PInvoke.WM_DISPLAYCHANGE:
                {
                    if (_selfWeakReference?.TryGetTarget(out var self) == true)
                    {
                        self?._displayInfoDebouncer?.OnEvent(2);
                    }
                    break;
                }

                case PInvoke.WM_EXITSIZEMOVE:
                {
                    if (_selfWeakReference?.TryGetTarget(out var self) == true)
                    {
                        self?._displayInfoDebouncer?.OnEvent(1);
                    }
                    break;
                }
            }
            return PInvoke.DefSubclassProc(hWnd, uMsg, wParam, lParam);
        }
    }
}
