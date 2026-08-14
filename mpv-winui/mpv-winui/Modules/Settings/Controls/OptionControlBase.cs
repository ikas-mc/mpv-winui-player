using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace mpv_winui.Modules.Settings.Controls;

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

    protected static void ApplyText(TextBlock label, TextBlock description, Option option)
    {
        label.Text = option.Label;

        if (string.IsNullOrEmpty(option.Description))
        {
            description.Visibility = Visibility.Collapsed;
        }
        else
        {
            description.Text = option.Description;
            description.Visibility = Visibility.Visible;
        }
    }

    protected static void ApplyIcon(FontIcon icon, Option option)
    {
        if (!string.IsNullOrEmpty(option.Icon))
        {
            icon.Glyph = option.Icon;
            icon.Visibility = Visibility.Visible;
        }
        else
        {
            icon.Visibility = Visibility.Collapsed;
        }
    }
}