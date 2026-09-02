using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.Windows.AppLifecycle;
using mpv_winui.Modules.Activation;
using mpv_winui.Modules.AppModel;
using mpv_winui.Modules.Common.Utils;
using mpv_winui.Modules.Common.View;
using mpv_winui.Modules.MediaInfo;
using mpv_winui.Modules.Menu.MenuEditor;
using mpv_winui.Modules.MpvConf;
using mpv_winui.Modules.Player;
using mpv_winui.Modules.Settings;
using System;
using System.Collections.Generic;

namespace mpv_winui
{
    public sealed partial class MainWindow : BaseWindow
    {
        private readonly WindowsManager _windowsManager;

        public MainWindow()
        {
            InitializeComponent();

            SetupStyle();

            _windowsManager = new WindowsManager();

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
                        }
                    });
                }
            }
            catch (System.Exception ex)
            {
                AppContext.AppLogger.Error(ex);
            }
        }

        public void ApplyMpvOption(string key, string value)
        {
            DispatcherQueue.RunAsync(() =>
            {
                if (ShellFrame?.Content is IMpvOptionApplySupport view)
                {
                    view.ApplyMpvOptionAsync(key, value).FireAndForget(ex =>
                    {
                        AppContext.AppLogger.Error(ex);
                    });
                }
            });
        }

        public void RunMpvCommand(string command)
        {
            DispatcherQueue.RunAsync(() =>
            {
                if (ShellFrame?.Content is IMpvCommandApplySupport view)
                {
                    view.ApplyMpvCommandAsync(command).FireAndForget(ex =>
                    {
                        AppContext.AppLogger.Error(ex);
                    });
                }
            });
        }

        public void UpdatePlayControlStyle()
        {
            DispatcherQueue.RunAsync(() =>
            {
                if (ShellFrame?.Content is MpvPlayerPage page)
                {
                    page.UpdatePlayControlStyle();
                }
            });
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

        public void OpenSettingWindow()
        {
            _windowsManager.Open("settings", () => new SettingsWindow(), this, 0.8, 320, 200);
        }

        public void OpenMpvConfigWindow()
        {
            _windowsManager.Open("mpvconf", () => new MpvConfEditorWindow(), this, 0.8, 480, 320);
        }

        public void OpenMediaInfoWindow(string? path)
        {
            _windowsManager.Open("mediainfo_" + HashUtil.ComputeMd5(path ?? string.Empty), () => new MediaInfoWindow(path), this, 0.8, 480, 320);
        }

        public void OpenMenuEditorWindow(string filePath, MenuType type)
        {
            _windowsManager.Open("menueditor_" + HashUtil.ComputeMd5(filePath ?? string.Empty), () => new MenuEditorWindow(filePath ?? string.Empty, type), this, 0.8, 720, 480);
        }

        private void Window_Activated(object sender, WindowActivatedEventArgs args)
        {
        }

        private void Window_Closed(object sender, WindowEventArgs args)
        {
            Activated -= Window_Activated;
            _windowsManager.Close();
        }
    }
}
