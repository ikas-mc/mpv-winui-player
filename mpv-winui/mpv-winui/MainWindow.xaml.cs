using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.Windows.AppLifecycle;
using mpv_winui.Modules.Activation;
using mpv_winui.Modules.AppModel;
using mpv_winui.Modules.Common.Utils;
using mpv_winui.Modules.Common.View;
using mpv_winui.Modules.Player;
using System.Collections.Generic;

namespace mpv_winui
{
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            TrySetBackdrop();

            AppWindow.TitleBar.ExtendsContentIntoTitleBar = true;
            ShellTitleBar.Title = PackageHelper.AppName;
            SetTitleBarColors();
            SetTitleBar(ShellTitleBar);

            AppWindow.Title = PackageHelper.AppName;
            AppWindow.SetIcon("App.ico");

            SetupWindowSize();
        }

        private bool SetTitleBarColors()
        {
            if (AppWindowTitleBar.IsCustomizationSupported())
            {
                AppWindowTitleBar titleBar = AppWindow.TitleBar;

                titleBar.ForegroundColor = Colors.White;
                //titleBar.BackgroundColor = Colors.Green;
                titleBar.ButtonForegroundColor = Colors.White;
                titleBar.ButtonBackgroundColor = Colors.Transparent;
                //titleBar.ButtonHoverForegroundColor = Colors.Gainsboro;
                //titleBar.ButtonHoverBackgroundColor = Colors.Transparent;
                //titleBar.ButtonPressedForegroundColor = Colors.Gray;
                //titleBar.ButtonPressedBackgroundColor = Colors.LightGreen;

                //titleBar.InactiveForegroundColor = Colors.Gainsboro;
                //titleBar.InactiveBackgroundColor = Colors.SeaGreen;
                //titleBar.ButtonInactiveForegroundColor = Colors.Gainsboro;
                titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
                return true;
            }
            return false;
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
    }
}