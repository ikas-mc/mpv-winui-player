using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Settings;
using Microsoft.Windows.AppLifecycle;
using mpv_winui.Modules.Common.Threading;
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

            //TODO config
            var activatedArgs = AppInstance.GetCurrent().GetActivatedEventArgs();
            var instance = AppInstance.FindOrRegisterForKey("main");
            if (!instance.IsCurrent)
            {
                instance.RedirectActivationToAsync(activatedArgs).GetAwaiter().GetResult();
                return;
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
    }
}
