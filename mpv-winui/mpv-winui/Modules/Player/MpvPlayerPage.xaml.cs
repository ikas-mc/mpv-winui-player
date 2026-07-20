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
    public sealed partial class MpvPlayerPage : Page, IParameterRefreshSupportView
    {
        private static readonly Logger _logger = LogManager.GetLogger("MpvPlayer");
        private const string MpvConfigFolderName = "mpv";
        private readonly MpvMediaPlayer _mediaPlayer = new();
        private bool _isPlayerInitialized;

        private readonly AppWindow _appWindow;

        private Task? _task;

        public MpvPlayerPage()
        {
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

                PlayerControl.InfoButtonVisibility = Visibility.Collapsed;
                PlayerControl.MediaPlayer = _mediaPlayer;

                _mediaPlayer.MediaOpened += MediaOpened;
                _mediaPlayer.VolumeChangedChanged += VolumeChangedChanged;
                _mediaPlayer.WindowChanged += MpvPlayerPage_WindowChanged;
                _mediaPlayer.StartListen();

                SetupWindowHook(this);

                OpenPedingPath();
            }
            else
            {
                //TODO
            }
        }

        private void MpvPlayerPage_Unloaded(object sender, RoutedEventArgs e)
        {
            _mediaPlayer.MediaOpened -= MediaOpened;
            _mediaPlayer.VolumeChangedChanged -= VolumeChangedChanged;
            _mediaPlayer.WindowChanged -= MpvPlayerPage_WindowChanged;
            _mediaPlayer.StopListen();
            TeardownPlayerView();
            RemoveWindowHook();
            _mediaPlayer.Close();
        }

        public async Task CreateAsync()
        {
            var configFolder = await AppData.Current.OpenOrCreateLocalDataFolderAsync(MpvConfigFolderName);
            if (_logger.IsDebugEnabled)
            {
                _logger.Debug("mpv config folder, path={}", configFolder.Path);
            }
            await _mediaPlayer.InitializeAsync(configFolder.Path, AppContext.AppSetting.LastVideoVolume);

            _isPlayerInitialized = true;
        }

        private void VolumeChangedChanged(MpvMediaPlayer player, int volume)
        {
            AppContext.AppSetting.LastVideoVolume = volume;
        }

        private void MediaOpened(MpvMediaPlayer player, object? arg2)
        {
            DispatcherQueue.RunAsync(() =>
            {
                if (NeedUpdatePlaylist())
                {
                    RefreshPlaylistAsync();
                }
            });
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
                _pendingPaths = e.Parameter as IReadOnlyList<string>;
            }
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
        }

        void IParameterRefreshSupportView.OnRefresh(object? parameter)
        {
            var paths = parameter as IReadOnlyList<string>;
            if (paths?.Count > 0)
            {
                //TODO impl open 
                _pendingPaths = paths;
                OpenPedingPath();
            }
        }
    }
}
