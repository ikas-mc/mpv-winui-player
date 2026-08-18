using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;

namespace mpv_winui.Modules.MpvConf.Option;

public abstract class MpvConfOptionControlBase : UserControl
{
    public event EventHandler? ApplyRequested;

    protected void RaiseApplyRequested()
    {
        ApplyRequested?.Invoke(this, EventArgs.Empty);
    }

    public MpvConfOptionItem? Item
    {
        get => (MpvConfOptionItem?)GetValue(ItemProperty);
        set => SetValue(ItemProperty, value);
    }

    public static readonly DependencyProperty ItemProperty = DependencyProperty.Register(nameof(Item), typeof(MpvConfOptionItem), typeof(MpvConfOptionControlBase), new PropertyMetadata(null, OnItemChanged));

    private static void OnItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is MpvConfOptionControlBase control)
        {
            control.RefreshValue();
        }
    }

    protected bool SuppressEdit
    {
        get;
        private set;
    }

    protected void UsingSuppressEdit(Action action)
    {
        SuppressEdit = true;
        try
        {
            action();
        }
        finally
        {
            SuppressEdit = false;
        }
    }

    private void RefreshValue()
    {
        UpdateInfo();
        UpdateEnabledState();
        UpdateModifiedState();
        UpdateOptionValue();
    }

    protected abstract void UpdateInfo();

    protected abstract void UpdateModifiedState();

    protected abstract void UpdateOptionValue();

    protected abstract void UpdateEnabledState();
}
