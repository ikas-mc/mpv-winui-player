using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using NLog;
using System;
using System.Threading.Tasks;

namespace mpv_winui.Modules.MediaInfo
{
    public sealed partial class MediaInfoUserControl : UserControl
    {
        private static readonly Logger _logger = LogManager.GetLogger("MediaInfo");

        private readonly Task<string?>? _infoTask;

        public MediaInfoUserControl(string? path)
        {
            this.InitializeComponent();

            UpdateLoading(true);

            if (!string.IsNullOrEmpty(path))
            {
                _infoTask = ReadMediaInfoAsync(path);
            }
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (_infoTask is { } task)
            {
                try
                {
                    ShowText(await task);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "MediaInfo read failed");
                    ShowText(ex.Message);
                }
            }

            UpdateLoading(false);
        }

        private static Task<string?> ReadMediaInfoAsync(string path)
        {
            return Task.Run(() =>
            {
                if (_logger.IsDebugEnabled)
                {
                    _logger.Debug("Reading media info, path={Path}", path);
                }

                using var mediaInfo = new NativeMediaInfo();
                return mediaInfo.Read(path);
            });
        }

        private void ShowText(string? text)
        {
            InfoText.Text = text;
        }

        private void UpdateLoading(bool show)
        {
            LoadingRing.IsActive = show;
            LoadingRing.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}