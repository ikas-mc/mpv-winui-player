using Microsoft.UI.Xaml.Controls;
using System.Runtime.InteropServices;
using Windows.Win32.Graphics.Dxgi;
using WinRT;

namespace mpv_winrt;

internal static class NativeInterop
{
    private static readonly Guid IID_ISwapChainPanelNative = new("63aad0b8-7c24-40ff-85a8-640d944cc325");

    public static unsafe void SetInverseScaleMatrix(nint swapChain, double scaleX, double scaleY)
    {
        var iid = typeof(IDXGISwapChain2).GUID;
        if (Marshal.QueryInterface(swapChain, in iid, out nint swapChain2) != 0)
        {
            return;
        }

        try
        {
            var matrix = new DXGI_MATRIX_3X2_F
            {
                _11 = (float)(1.0 / scaleX),
                _22 = (float)(1.0 / scaleY),
            };

            ((IDXGISwapChain2*)swapChain2)->SetMatrixTransform(&matrix);
        }
        finally
        {
            Marshal.Release(swapChain2);
        }
    }

    public static unsafe void SetSwapChainPanel(SwapChainPanel panel, nint swapChain)
    {
        nint panelUnknown = ((IWinRTObject)panel).NativeObject.ThisPtr;
        var panelGuid = IID_ISwapChainPanelNative;
        if (Marshal.QueryInterface(panelUnknown, in panelGuid, out nint nativePanel) != 0)
        {
            return;
        }

        try
        {
            void** vtbl = *(void***)nativePanel;
            var setSwapChain = (delegate* unmanaged[Stdcall]<nint, nint, int>)vtbl[3];
            setSwapChain(nativePanel, swapChain);
        }
        finally
        {
            Marshal.Release(nativePanel);
        }
    }
}
