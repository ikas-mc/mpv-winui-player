using Microsoft.UI;
using Microsoft.UI.Composition;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using mpv_winui.Modules.Common.Utils;
using mpv_winui.Modules.Settings;
using System;
using Windows.UI.ViewManagement;
using WinRT;

namespace mpv_winui.Modules.Common.View;

public sealed partial class WindowStyleManager : IDisposable
{
    private readonly Window _window;
    private readonly FrameworkElement _contentRoot;
    private readonly UISettings _uiSettings = new();
    private SystemBackdropConfiguration? _configurationSource;
    private DesktopAcrylicController? _acrylicController;
    private MicaController? _micaController;
    private ElementTheme _theme;

    public WindowStyleManager(Window window)
    {
        _window = window;
        _contentRoot = (FrameworkElement)window.Content;
    }

    public void Setup()
    {
        _theme = GetThemeType();
        UpdateTitleBarColors(_theme);
        UpdateContentTheme(_theme);

        _window.DispatcherQueue.EnsureSystemDispatcherQueue();
        _configurationSource = new SystemBackdropConfiguration
        {
            IsInputActive = true
        };

        switch (AppContext.AppSetting.BackdropType)
        {
            case AppSettings.BackdropType_Mica:
            {
                if (MicaController.IsSupported())
                {
                    _micaController = new MicaController();
                    _micaController?.AddSystemBackdropTarget(_window.As<ICompositionSupportsSystemBackdrop>());
                    _micaController?.SetSystemBackdropConfiguration(_configurationSource);
                }
                break;
            }
            default:
            {
                if (DesktopAcrylicController.IsSupported())
                {
                    _acrylicController = new DesktopAcrylicController();
                    _acrylicController?.AddSystemBackdropTarget(_window.As<ICompositionSupportsSystemBackdrop>());
                    _acrylicController?.SetSystemBackdropConfiguration(_configurationSource);
                }
                break;
            }
        }

        UpdateBackdropTheme(_theme);

        _uiSettings.ColorValuesChanged += OnColorValuesChanged;
    }

    public ElementTheme GetThemeType()
    {
        return AppContext.AppSetting.ThemeType switch
        {
            AppSettings.ThemeType_Dark => ElementTheme.Dark,
            AppSettings.ThemeType_Light => ElementTheme.Light,
            _ => App.Current.RequestedTheme == ApplicationTheme.Light ? ElementTheme.Light : ElementTheme.Dark,
        };
    }

    public void UpdateTheme(ElementTheme theme)
    {
        if (_theme == theme)
        {
            return;
        }

        _theme = theme;
        UpdateContentTheme(theme);
        UpdateBackdropTheme(theme);
        UpdateTitleBarColors(theme);
    }

    public void Cleanup()
    {
        _uiSettings.ColorValuesChanged -= OnColorValuesChanged;
        _acrylicController?.Dispose();
        _acrylicController = null;
        _micaController?.Dispose();
        _micaController = null;
        _configurationSource = null;
    }

    private void OnColorValuesChanged(UISettings sender, object args)
    {
        _window.DispatcherQueue.RunAsync(() =>
        {
            var theme = GetThemeType();
            if (_theme != theme)
            {
                UpdateTheme(theme);
            }
            else
            {
                _acrylicController?.TintColor = sender.GetColorValue(UIColorType.AccentLight1);
            }
        });
    }

    private void UpdateContentTheme(ElementTheme theme)
    {
        _contentRoot.RequestedTheme = theme;
    }

    private void UpdateBackdropTheme(ElementTheme theme)
    {
        _configurationSource?.Theme = theme switch
        {
            ElementTheme.Dark => SystemBackdropTheme.Dark,
            _ => SystemBackdropTheme.Light
        };

        if (theme == ElementTheme.Dark)
        {
            _acrylicController?.Kind = DesktopAcrylicKind.Thin;
            _acrylicController?.TintOpacity = 0.2F;
            _acrylicController?.TintColor = _uiSettings.GetColorValue(UIColorType.AccentLight1);
            _acrylicController?.LuminosityOpacity = 0.1F;
        }
        else
        {
            _acrylicController?.Kind = DesktopAcrylicKind.Thin;
            _acrylicController?.TintOpacity = 0.2F;
            _acrylicController?.TintColor = Colors.White;
            _acrylicController?.LuminosityOpacity = 0.8F;
        }
    }

    private void UpdateTitleBarColors(ElementTheme theme)
    {
        if (!AppWindowTitleBar.IsCustomizationSupported())
        {
            return;
        }

        var titleBar = _window.AppWindow.TitleBar;
        //titleBar.ForegroundColor = Colors.White;
        //titleBar.BackgroundColor = Colors.Green;
        titleBar.ButtonForegroundColor = theme == ElementTheme.Dark ? Colors.White : Colors.Black;
        titleBar.ButtonBackgroundColor = Colors.Transparent;
        //titleBar.ButtonHoverForegroundColor = Colors.Gainsboro;
        //titleBar.ButtonHoverBackgroundColor = Colors.Transparent;
        //titleBar.ButtonPressedForegroundColor = Colors.Gray;
        //titleBar.ButtonPressedBackgroundColor = Colors.LightGreen;

        //titleBar.InactiveForegroundColor = Colors.Gainsboro;
        //titleBar.InactiveBackgroundColor = Colors.SeaGreen;
        //titleBar.ButtonInactiveForegroundColor = Colors.Gainsboro;
        titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
    }

    public void Dispose()
    {
        Cleanup();
    }
}