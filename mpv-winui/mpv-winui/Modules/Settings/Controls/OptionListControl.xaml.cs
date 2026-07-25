using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using mpv_winui.Modules.Settings.Controls;
using System.Collections.Generic;

namespace mpv_winui.Modules.Settings.Controls;

public sealed partial class OptionListControl : UserControl
{
    public OptionListControl()
    {
        InitializeComponent();
    }

    private void OnContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
    {
        if (args.InRecycleQueue)
        {
            return;
        }

        if (args.Item is not Option option)
        {
            return;
        }

        if (args.ItemContainer.ContentTemplateRoot is OptionControlBase control)
        {
            control.Setting = option;
        }
        args.Handled = true;
    }

    public List<Option> OptionList
    {
        get => (List<Option>)GetValue(SettingProperty);
        set => SetValue(SettingProperty, value);
    }

    public static readonly DependencyProperty SettingProperty =
        DependencyProperty.Register(nameof(OptionList), typeof(List<Option>),
            typeof(OptionListControl), new PropertyMetadata((List<Option>)[]));

}