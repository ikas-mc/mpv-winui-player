using System;
using Windows.ApplicationModel.DataTransfer;

namespace mpv_winui.Modules.Common.Utils
{
    public static class ClipboardHelper
    {
        public static void SetCopyText(string? text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            var data = new DataPackage { RequestedOperation = DataPackageOperation.Copy };
            data.SetText(text);
            Clipboard.SetContent(data);
        }

        public static void SetCopyUri(Uri? uri)
        {
            if (uri == null)
            {
                return;
            }

            var data = new DataPackage { RequestedOperation = DataPackageOperation.Copy };
            data.SetUri(uri);
            Clipboard.SetContent(data);
        }
    }
}
