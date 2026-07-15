using System;
using System.Linq;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Core;
namespace mpv_winui.Modules.AppModel
{
    public class PackageHelper
    {
        private static readonly Lazy<bool> _isPackagedValue = new(CheckPackaged, true);

        public static readonly bool IsPackaged = _isPackagedValue.Value;

        private static readonly Lazy<string> _appNameValue = new(GetAppName, true);

        public static readonly string AppName = _appNameValue.Value;

        private static readonly Lazy<string> _appVersionValue = new(GetAppVersion, true);

        public static readonly string AppVersion = _appVersionValue.Value;

        public static bool CheckPackaged()
        {
            try
            {
                return Package.Current != null && Package.Current.Id != null;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static string GetAppVersion()
        {
            if (IsPackaged)
            {
                return $"{Package.Current.Id.Version.Major}.{Package.Current.Id.Version.Minor}";
            }
            else
            {
                return AppContext.AppLang.AppVersion;
            }
        }

        public static string GetAppName()
        {
            if (IsPackaged)
            {
                if (Package.Current.GetAppListEntries().FirstOrDefault() is AppListEntry entry)
                {
                    return entry.DisplayInfo.DisplayName;
                }
                return Package.Current.DisplayName;
            }
            else
            {
                return AppContext.AppLang.AppName;
            }
        }
    }
}
