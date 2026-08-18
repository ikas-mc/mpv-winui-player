using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using mpv_winui.Modules.Common.Utils;
using mpv_winui.Modules.Common.View;
using mpv_winui.Modules.FileSystem;
using NLog;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace mpv_winui.Modules.Player
{
    public sealed partial class MpvPlayerPage : Page, IParameterRefreshSupportView, IMpvOptionApplySupport
    {
        private static readonly Logger _logger = LogManager.GetLogger("MpvPlayer");
        private const string MpvConfigFolderName = "mpv";
        private readonly MpvMediaPlayer _mediaPlayer = new();
        private bool _isPlayerInitialized;
        private static WeakReference<MpvPlayerPage>? _selfWeakReference;

        private readonly AppWindow _appWindow;

        private Task? _task;

        public MpvPlayerPage()
        {
            _selfWeakReference = new(this);
            _appWindow = App.Window?.AppWindow!;
            InitializeComponent();

            _task = CreateAsync();

            Loaded += MpvPlayerPage_Loaded;
            Unloaded += MpvPlayerPage_Unloaded;
        }

        private async void MpvPlayerPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (_task is { } task)
            {
                try
                {
                    await task;
                    _task = null;
                }
                catch (Exception ex)
                {
                    _logger.Error(ex);
                }
            }

            if (_isPlayerInitialized)
            {
                SetupPlayerView();

                PlayerControl.MediaPlayer = _mediaPlayer;

                _mediaPlayer.PlaylistChanged += MpvPlayerPage_PlaylistChanged;
                _mediaPlayer.VolumeChangedChanged += VolumeChangedChanged;
                _mediaPlayer.WindowChanged += MpvPlayerPage_WindowChanged;
                _mediaPlayer.MediaInfoChanged += MpvPlayerPage_MediaInfoChanged;
                _mediaPlayer.StartListen();

                SetupKeyboardInput();

                if (AppContext.AppSetting.EnableVideoPreview)
                {
                    SetupPreview();
                }
                if (AppContext.AppSetting.EnableVideoBuiltInPreview)
                {
                    SetupBuiltInPreview();
                }

                SetupCustomMenuBarItems();

                OpenPendingPath().FireAndForget(OnException);
            }
            else
            {
                //TODO
            }
        }

        private void MpvPlayerPage_Unloaded(object sender, RoutedEventArgs e)
        {
            CleanupDisplayInfo();

            _mediaPlayer.PlaylistChanged -= MpvPlayerPage_PlaylistChanged;
            _mediaPlayer.VolumeChangedChanged -= VolumeChangedChanged;
            _mediaPlayer.WindowChanged -= MpvPlayerPage_WindowChanged;
            _mediaPlayer.MediaInfoChanged -= MpvPlayerPage_MediaInfoChanged;
            _mediaPlayer.StopListen();
            TeardownPlayerView();
            CleanupKeyboardInput();
            CleanupPreview();
            CleanupBuiltInPreview();
            _mediaPlayer.Close();
        }

        private async Task CreateAsync()
        {
            InitDisplayInfo();

            var configFolder = await AppData.Current.OpenOrCreateLocalDataFolderAsync(MpvConfigFolderName);
            if (_logger.IsDebugEnabled)
            {
                _logger.Debug("mpv config folder, path={}", configFolder.Path);
            }

            _mediaPlayer.SwapChainChanged += MpvPlayer_SwapChainChanged;
            await _mediaPlayer.InitializeAsync(configFolder.Path, AppContext.AppSetting.LastVideoVolume, _lastColorKind, (int)_lastRefreshRate);

            _isPlayerInitialized = true;
        }

        private void VolumeChangedChanged(MpvMediaPlayer player, int volume)
        {
            AppContext.AppSetting.LastVideoVolume = volume;
        }

        private void PlayerView_Tapped(object sender, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            PlayerControl.ToggleControlPanel();
        }

        private void AppQuit()
        {
            Application.Current.Exit();
        }

        private void OnException(Exception ex)
        {
            //TODO add notify
            _logger.Error(ex);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            _pendingPaths = null;
            if (e.NavigationMode == NavigationMode.New)
            {
                _pendingPaths = e.Parameter as IReadOnlyList<FileItem>;
            }
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
        }

        void IParameterRefreshSupportView.OnRefresh(object? parameter)
        {
            var paths = parameter as IReadOnlyList<FileItem>;
            if (paths?.Count > 0)
            {
                _pendingPaths = paths;
                OpenPendingPath().FireAndForget(OnException);
            }
        }

        async Task IMpvOptionApplySupport.ApplyMpvOptionAsync(string key, string value)
        {
            if (!_isPlayerInitialized || string.IsNullOrEmpty(key))
            {
                return;
            }

            await Task.Run(() =>
            {
                _mediaPlayer.Command(["set", key, value ?? string.Empty]);
            });
        }
    }
}
