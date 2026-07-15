namespace mpv_winui.Modules.Settings
{
    public interface IDataSetting
    {
        T GetValue<T>(string propertyName, T defaultValue);
        bool SetValue<T>(string propertyName, T value);
    }
}