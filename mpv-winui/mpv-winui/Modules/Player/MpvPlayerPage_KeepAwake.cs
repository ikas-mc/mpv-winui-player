using mpv_winrt;
using mpv_winui.Modules.Common.Utils;
using System;

namespace mpv_winui.Modules.Player
{
    public sealed partial class MpvPlayerPage
    {
        private SystemDisplayRequest? _displayRequest;

        private void SetupKeepAwake()
        {
            if (!AppContext.AppSetting.KeepDisplayAwake)
            {
                return;
            }

            _displayRequest = new SystemDisplayRequest();

            _mediaPlayer.FileLoaded += MpvPlayerPage_FileLoaded;
            _mediaPlayer.FileFailed += MpvPlayerPage_FileFailed;
            _mediaPlayer.FileEnded += MpvPlayerPage_FileEnded;
            _mediaPlayer.FileStopped += MpvPlayerPage_FileStopped;
            _mediaPlayer.PlaybackStateChanged += MpvPlayerPage_PlaybackStateChanged;
        }

        private void CleanupKeepAwake()
        {
            _mediaPlayer.FileLoaded -= MpvPlayerPage_FileLoaded;
            _mediaPlayer.FileFailed -= MpvPlayerPage_FileFailed;
            _mediaPlayer.FileEnded -= MpvPlayerPage_FileEnded;
            _mediaPlayer.FileStopped -= MpvPlayerPage_FileStopped;
            _mediaPlayer.PlaybackStateChanged -= MpvPlayerPage_PlaybackStateChanged;
            StopKeepDisplayAwake();
            _displayRequest = null;
        }

        private void MpvPlayerPage_FileLoaded()
        {
            if (_mediaPlayer.IsPaused())
            {
                StopKeepDisplayAwake();
            }
            else
            {
                StartKeepDisplayAwake();
            }
        }

        private void MpvPlayerPage_FileEnded()
        {
            StopKeepDisplayAwake();
        }

        private void MpvPlayerPage_FileFailed(FileFailedEventArgs args)
        {
            StopKeepDisplayAwake();
        }

        private void MpvPlayerPage_FileStopped()
        {
            StopKeepDisplayAwake();
        }

        private void MpvPlayerPage_PlaybackStateChanged(PlaybackStateChangedEventArgs args)
        {
            if (args.IsPaused)
            {
                StopKeepDisplayAwake();
            }
            else
            {
                StartKeepDisplayAwake();
            }
        }

        private void StartKeepDisplayAwake()
        {
            if (!AppContext.AppSetting.KeepDisplayAwake)
            {
                return;
            }

            DispatcherQueue.RunAsync(() =>
            {
                try
                {
                    _displayRequest?.Enable();
                }
                catch (Exception ex)
                {
                    OnException(ex);
                }
            });
        }

        private void StopKeepDisplayAwake()
        {
            DispatcherQueue.RunAsync(() =>
            {
                try
                {
                    _displayRequest?.Disable();
                }
                catch (Exception ex)
                {
                    OnException(ex);
                }
            });
        }
    }
}