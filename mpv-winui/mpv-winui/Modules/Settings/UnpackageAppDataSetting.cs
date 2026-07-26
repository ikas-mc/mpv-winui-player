using Microsoft.Windows.Storage;
using mpv_winui.Modules.FileSystem;

namespace mpv_winui.Modules.Settings
{
    public class UnpackageAppDataSetting : IDataSetting
    {
        private readonly ApplicationDataContainer _container;

        public UnpackageAppDataSetting(string typeName)
        {
            //HKEY_CURRENT_USER\Software\Classes\Local Settings\Software\mpv-winui\mpv-winui\app
            var application = ApplicationData.GetForUnpackaged(AppData.AppDataId, AppData.AppDataId);
            _container = application.LocalSettings.CreateContainer(typeName, ApplicationDataCreateDisposition.Always);
        }

        public T GetValue<T>(string propertyName, T defaultValue)
        {
            if (string.IsNullOrEmpty(propertyName))
            {
                return defaultValue;
            }

            try
            {
                var value = _container.Values[propertyName];
                if (value is T t)
                {
                    return t;
                }
            }
            catch (System.Exception)
            {
                //error when no key 
                _container.Values[propertyName] = defaultValue;
            }

            return defaultValue;
        }

        public bool SetValue<T>(string propertyName, T value)
        {
            if (string.IsNullOrEmpty(propertyName))
            {
                return false;
            }

            _container.Values[propertyName] = value;

            return true;
        }
    }
}