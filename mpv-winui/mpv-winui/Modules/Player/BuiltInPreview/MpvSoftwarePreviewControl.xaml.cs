using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using mpv_winrt;
using mpv_winui.Modules.Common.Utils;
using NLog;
using System;
using System.Threading.Tasks;

namespace mpv_winui.Modules.Player.BuiltInPreview
{
    public sealed partial class MpvSoftwarePreviewControl : UserControl
    {
        private static readonly Logger _logger = LogManager.GetLogger("BuiltInPreview");
        private MpvPreviewer? _previewer;
        private Task<MpvPreviewer?>? _previewTask;
        private bool _keepAlive;
        private int _keepAliveTimeout;
        private string _previewLoadedPath = string.Empty;
        private double _lastPreviewSec = -1;
        private (string Path, double Sec)? _pendingPreview;
        private readonly DispatcherTimer _previewDestroyTimer;

        public MpvSoftwarePreviewControl()
        {
            InitializeComponent();
            _previewDestroyTimer = new DispatcherTimer();
            _previewDestroyTimer.Tick += PreviewDestroyTimer_Tick;
        }

        public bool KeepAlive
        {
            get => _keepAlive;
            set => _keepAlive = value;
        }

        public int KeepAliveTimeout
        {
            get => _keepAliveTimeout;
            set => _keepAliveTimeout = value;
        }

        public async void Show(double hoverSec, string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                Hide();
                return;
            }

            if (_previewer != null)
            {
                ShowAt(path, hoverSec);
                return;
            }

            _pendingPreview = (path, hoverSec);

            if (_previewTask == null)
            {
                _previewTask = CreatePreviewerAsync();
                _previewer = await _previewTask;
                _previewTask = null;

                if (_previewer != null && _pendingPreview is { } pending)
                {
                    ShowAt(pending.Path, pending.Sec);
                }
            }
        }

        public void Hide()
        {
            PreviewCard.Visibility = Visibility.Collapsed;

            if (!_keepAlive)
            {
                if (_keepAliveTimeout > 0)
                {
                    _previewDestroyTimer.Interval = TimeSpan.FromSeconds(_keepAliveTimeout);
                    _previewDestroyTimer.Stop();
                    _previewDestroyTimer.Start();
                }
                else
                {
                    DestroyPreviewer();
                }
            }
        }

        public void Close()
        {
            DestroyPreviewer();
        }

        private void PreviewDestroyTimer_Tick(object? sender, object e)
        {
            _previewDestroyTimer.Stop();
            DestroyPreviewer();
        }

        private (int Width, int Height) GetRenderSize()
        {
            var scale = XamlRoot?.RasterizationScale ?? 1.0;
            if (scale < 1.0)
            {
                scale = 1.0;
            }
            var width = ActualWidth > 0 ? ActualWidth : Width;
            var height = ActualHeight > 0 ? ActualHeight : Height;
            if (width <= 0)
            {
                width = 320;
            }
            if (height <= 0)
            {
                height = 180;
            }
            return ((int)Math.Ceiling(width * scale), (int)Math.Ceiling(height * scale));
        }

        private async Task<MpvPreviewer?> CreatePreviewerAsync()
        {
            MpvPreviewer? previewer = null;
            try
            {
                previewer = new MpvPreviewer();
                var (renderWidth, renderHeight) = GetRenderSize();
                await previewer.Initialize(ThumbnailImage, (uint)renderWidth, (uint)renderHeight);
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                if (null != previewer)
                {
                    try
                    {
                        await Task.Run(() => previewer.Destroy());
                    }
                    catch (Exception ex2)
                    {
                        _logger.Error(ex2);
                    }
                }
                previewer = null;
            }

            return previewer;
        }

        private void ShowAt(string path, double sec)
        {
            _previewDestroyTimer.Stop();

            if (!string.Equals(_previewLoadedPath, path, StringComparison.Ordinal))
            {
                _previewLoadedPath = path;
                _lastPreviewSec = -1;
                Task.Run(() =>
                {
                    _previewer?.LoadFile(path);
                    _previewer?.SetPosition(sec);
                }).FireAndForget(OnException);

                _lastPreviewSec = sec;
            }
            else if (Math.Abs(sec - _lastPreviewSec) > 0.05)
            {
                _previewer?.SetPosition(sec);
                _lastPreviewSec = sec;
            }

            PreviewCard.Visibility = Visibility.Visible;
        }

        private void DestroyPreviewer()
        {
            _previewDestroyTimer.Stop();
            _pendingPreview = null;

            _previewLoadedPath = string.Empty;
            _lastPreviewSec = -1;
            ThumbnailImage.Source = null;
            PreviewCard.Visibility = Visibility.Collapsed;

            if (_previewer is { } previewer)
            {
                _previewer = null;
                Task.Run(() => previewer.Destroy()).FireAndForget(OnException);
            }
        }

        private void OnException(Exception ex)
        {
            _logger.Error(ex);
        }
    }
}