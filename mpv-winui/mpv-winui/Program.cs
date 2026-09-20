using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Settings;
using Microsoft.Windows.AppLifecycle;
using mpv_winui.Modules.Common.Threading;
using mpv_winui.Modules.Settings;
using System;
using System.Threading;
using WinRT;

namespace mpv_winui
{
    public class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            ComWrappersSupport.InitializeComWrappers();

            var instance = AppInstance.FindOrRegisterForKey("main");
            if (!instance.IsCurrent)
            {
                if (IsSingleInstanceEnabled())
                {
                    var activatedArgs = AppInstance.GetCurrent().GetActivatedEventArgs();
                    instance.RedirectActivationToAsync(activatedArgs).GetAwaiter().GetResult();
                    return;
                }
            }

            instance.Activated += OnActivated;

            XamlOptionalChanges.EnableChange(XamlChangeId.DefaultStyleOptimizations);
            XamlOptionalChanges.EnableChange(XamlChangeId.OptimizeApplyStyles);
            XamlOptionalChanges.EnableChange(XamlChangeId.IconNoGridOptimization);
            XamlOptionalChanges.EnableChange(XamlChangeId.DeferContextFlyoutInit);
            Application.Start((p) =>
            {
                var context = new DefaultSynchronizationContext(DispatcherQueue.GetForCurrentThread());
                SynchronizationContext.SetSynchronizationContext(context);
                _ = new App();
            });
        }

        private static void OnActivated(object? _, AppActivationArguments args)
        {
            if (Application.Current is App app)
            {
                app.OnActivated(args);
            }
        }

        private static bool IsSingleInstanceEnabled()
        {
            try
            {
                return AppSettings.CreateDataSetting().GetValue(nameof(AppSettings.SingleAppInstance), true);
            }
            catch (Exception)
            {
                return true;
            }
        }
    }
}
