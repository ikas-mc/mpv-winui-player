using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using mpv_winui.Modules.Common.Utils;
using NLog;
using System;
using System.Threading.Tasks;

namespace mpv_winui.Modules.Player.BuiltInPreview
{
    public sealed partial class MpvPreviewControl : UserControl
    {
        private static readonly Logger _logger = LogManager.GetLogger("BuiltInPreview");
        private MpvMediaPlayer? _previewPlayer;
        private Task<MpvMediaPlayer?>? _previewTask;
        private bool _keepAlive;
        private int _keepAliveTimeout;
        private string _previewLoadedPath = string.Empty;
        private double _lastPreviewSec = -1;
        private (string Path, double Sec)? _pendingPreview;
        private readonly DispatcherTimer _previewDestroyTimer;

        public MpvPreviewControl()
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

            if (_previewPlayer != null)
            {
                ShowAt(path, hoverSec);
                return;
            }

            _pendingPreview = (path, hoverSec);

            if (_previewTask == null)
            {
                _previewTask = CreatePreviewPlayerAsync();
                _previewPlayer = await _previewTask;
                _previewTask = null;

                if (_previewPlayer != null && _pendingPreview is { } pending)
                {
                    ShowAt(pending.Path, pending.Sec);
                }
            }
        }

        public void Hide()
        {
            PreviewPanel.Visibility = Visibility.Collapsed;

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
                    DestroyPreviewPlayer();
                }
            }
        }

        public void Close()
        {
            DestroyPreviewPlayer();
        }

        private void PreviewDestroyTimer_Tick(object? sender, object e)
        {
            _previewDestroyTimer.Stop();
            DestroyPreviewPlayer();
        }

        private async Task<MpvMediaPlayer?> CreatePreviewPlayerAsync()
        {
            MpvMediaPlayer? created = null;
            try
            {
                created = new MpvMediaPlayer();
                created.SwapChainChanged += PreviewPlayer_SwapChainChanged;
                await created.InitializeForPreviewAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                if (created != null)
                {
                    try
                    {
                        created.SwapChainChanged -= PreviewPlayer_SwapChainChanged;
                        await Task.Run(() => created.Close());
                    }
                    catch (Exception ex2)
                    {
                        _logger.Error(ex2);
                    }
                }
                created = null;
            }

            return created;
        }

        private void ShowAt(string path, double sec)
        {
            _previewDestroyTimer.Stop();

            if (!string.Equals(_previewLoadedPath, path, StringComparison.Ordinal))
            {
                _previewLoadedPath = path;
                _lastPreviewSec = -1;
                _previewPlayer!.LoadFile(path, sec);
                _previewPlayer.Pause();
                _lastPreviewSec = sec;
            }
            else if (Math.Abs(sec - _lastPreviewSec) > 0.05)
            {
                _previewPlayer!.Position = sec;
                _lastPreviewSec = sec;
            }

            PreviewPanel.Visibility = Visibility.Visible;
        }

        private void PreviewPlayer_SwapChainChanged(MpvMediaPlayer player, object? arg)
        {
            DispatcherQueue.RunAsync(() =>
            {
                if (_previewPlayer is null)
                {
                    return;
                }

                var width = (uint)Math.Floor(PreviewPanel.ActualWidth * PreviewPanel.CompositionScaleX);
                var height = (uint)Math.Floor(PreviewPanel.ActualHeight * PreviewPanel.CompositionScaleY);
                _previewPlayer.UpdateSize(Math.Max(1, width), Math.Max(1, height));

                _previewPlayer.UpdatePanel(PreviewPanel);
            });
        }

        private void DestroyPreviewPlayer()
        {
            _previewDestroyTimer.Stop();
            _pendingPreview = null;

            _previewLoadedPath = string.Empty;
            _lastPreviewSec = -1;
            PreviewPanel.Visibility = Visibility.Collapsed;

            if (_previewPlayer is { } player)
            {
                _previewPlayer = null;
                player.SwapChainChanged -= PreviewPlayer_SwapChainChanged;
                player.FileLoaded = null;
                Task.Run(() => player.Close()).FireAndForget(OnCloseError);
            }
        }

        private void OnCloseError(Exception ex)
        {
            _logger.Error(ex);
        }
    }
}