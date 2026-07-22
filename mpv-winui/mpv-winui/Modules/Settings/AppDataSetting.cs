using Windows.Storage;

namespace mpv_winui.Modules.Settings
{
    public class AppDataSetting : IDataSetting
    {
        private readonly ApplicationDataContainer _container;

        public AppDataSetting(string typeName)
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            _container = localSettings.CreateContainer(typeName, ApplicationDataCreateDisposition.Always);
        }

        public T GetValue<T>(string propertyName, T defaultValue)
        {
            if (string.IsNullOrEmpty(propertyName))
            {
                return defaultValue;
            }

            var value = _container.Values[propertyName];
            if (value is T t)
            {
                return t;
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