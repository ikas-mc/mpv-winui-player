using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.Windows.AppLifecycle;
using mpv_winui.Modules.Activation;
using mpv_winui.Modules.AppModel;
using mpv_winui.Modules.Common.Utils;
using mpv_winui.Modules.Common.View;
using mpv_winui.Modules.Player;
using mpv_winui.Modules.Settings;
using System;
using System.Collections.Generic;
using Windows.Graphics;
using Windows.UI.ViewManagement;

namespace mpv_winui
{
    public sealed partial class MainWindow : Window
    {
        private readonly UISettings _uISettings = new();

        public MainWindow()
        {
            InitializeComponent();

            SetupStyle();

            AppWindow.TitleBar.ExtendsContentIntoTitleBar = true;
            ShellTitleBar.Title = PackageHelper.AppName;
            SetTitleBar(ShellTitleBar);

            AppWindow.Title = PackageHelper.AppName;
            AppWindow.SetIcon("App.ico");

            SetupWindowSize();

            Activated += Window_Activated;
            Closed += Window_Closed;
        }

        public async void Open()
        {
            IReadOnlyList<FileItem>? fileItems = null;
            try
            {
                var activatedArgs = AppInstance.GetCurrent().GetActivatedEventArgs();
                fileItems = await ActivationService.Instance.ParseFileItemsAsync(activatedArgs);
            }
            catch (System.Exception ex)
            {
                AppContext.AppLogger.Error(ex);
            }

            ShellFrame.Navigate(typeof(MpvPlayerPage), fileItems);
        }

        public async void Refresh(AppActivationArguments activatedArgs)
        {
            try
            {
                var fileItems = await ActivationService.Instance.ParseFileItemsAsync(activatedArgs);
                if (fileItems?.Count > 0)
                {
                    DispatcherQueue.RunAsync(() =>
                    {
                        if (ShellFrame?.Content is IParameterRefreshSupportView view)
                        {
                            view.OnRefresh(fileItems);
                            this.ShowWindow();
                        }
                    });
                }
            }
            catch (System.Exception ex)
            {
                AppContext.AppLogger.Error(ex);
            }
        }

        public void ChangeFullWindow(bool full)
        {
            if (full)
            {
                TitleBarRow.Height = new GridLength(0);
            }
            else
            {
                TitleBarRow.Height = GridLength.Auto;
            }
        }

        public void UpdateTitle(string title)
        {
            ShellTitleBar?.Title = title;
        }

        private SettingsWindow? _settingsWindow;
        public void OpenSettingWindow()
        {
            if (null == _settingsWindow)
            {
                _settingsWindow = new();
                _settingsWindow?.Activate();
                _settingsWindow?.Closed += SettingsWindow_Closed;
            }

            var position = AppWindow.Position;
            var size = AppWindow.Size;
            var rect = new RectInt32(
                (int)(position.X + (size.Width * 0.1)),
                (int)(position.Y + (size.Height * 0.1)),
                (int)(size.Width * 0.8),
                (int)(size.Height * 0.8)
                );
            _settingsWindow?.MoveAndResize(rect);

            _settingsWindow?.ShowWindow();
        }

        private void SettingsWindow_Closed(object sender, WindowEventArgs args)
        {
            _settingsWindow = null;
        }

        private void Window_Activated(object sender, WindowActivatedEventArgs args)
        {
        }

        private void Window_Closed(object sender, WindowEventArgs args)
        {
            Activated -= Window_Activated;
            _settingsWindow?.Close();
            CleanupTheme();
        }
    }
}