using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using mpv_winui.Modules.Settings.Controls;

namespace mpv_winui.Modules.Settings;

public abstract class OptionControlBase : UserControl
{
    public Option? Setting
    {
        get => (Option?)GetValue(SettingProperty);
        set => SetValue(SettingProperty, value);
    }

    public static readonly DependencyProperty SettingProperty =
        DependencyProperty.Register(nameof(Setting), typeof(Option),
            typeof(OptionControlBase), new PropertyMetadata(null, OnSettingChanged));

    private static void OnSettingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is OptionControlBase control)
        {
            control.OnSettingChanged((Option?)e.OldValue, (Option?)e.NewValue);
        }
    }

    protected virtual void OnSettingChanged(Option? oldValue, Option? newValue)
    {
    }

    public virtual (bool IsValid, string? ErrorMessage) Validate() => (true, null);
}