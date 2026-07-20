using Microsoft.UI.Xaml;
using mpv_winui.Modules.Win32;

namespace mpv_winui.Modules.Common.View
{
    public static class WindowExtensions
    {
        extension(Window window)
        {
            public async void ShowWindow()
            {
                Win32WindowHelper.SetForegroundWindow(window);
            }
        }
    }
}
