# mpv-winui-player

base on [mpv](https://github.com/mpv-player/mpv/) and [WinUI 3](https://github.com/microsoft/microsoft-ui-xaml).

## Screenshot

<img src="https://raw.githubusercontent.com/ikas-mc/mpv-winui-player/main/screenshot/screenshot.png" width="600" />



## Limitation

### menu.conf

Some commands aren't supported,  like quit and window related cmd...

### vo

The player use the `d3d11-output-mode=composition` mode, mpv can't get display information.

Use these custom properties as a workaround

```
user-data/mpvw/color-kind : SDR, WCG, HDR
user-data/mpvw/refresh-rate : 60
```

example:

```
[mpvw-sdr]
profile-cond=p["user-data/mpvw/color-kind"] == "SDR"
profile-restore=copy
d3d11-output-csp=srgb
d3d11-output-format=rgb10_a2
```
## Thumbnail Preview

* Built-in preview
* Supports plugins using [osc-preview-api](https://mpv.io/manual/master/#osc-preview-api)


## Msix or Unpackaged

|  | Msix | Unpackaged |
| :--- | :--- | :--- |
| **Data** | `C:\Users\user\AppData\Local\Packages\--\LocalState` | `C:\Users\user\AppData\Local\ikas-mc\mpvw` |
| **Settings** | `C:\Users\user\AppData\Local\Packages\--\Settings` | `HKEY_CURRENT_USER\Software\Classes\Local Settings\Software\ikas-mc\mpvw\app` |
| **File Association** | - | TODO |
| **Protocol** | mpvw://?file= | TODO |


## License

LGPL-2.1 — see [LICENSE.txt](LICENSE.txt).
