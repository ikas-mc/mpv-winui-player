using NLog;
using System;
using System.Runtime.InteropServices;

namespace mpv_winui.Modules.MediaInfo;

public partial class NativeMediaInfo : IDisposable
{
    private static readonly Logger _logger = LogManager.GetLogger(nameof(NativeMediaInfo));

    private nint _handle;
    private bool _disposed;

    public NativeMediaInfo()
    {
        _handle = Native.MediaInfo_New();
        if (_logger.IsDebugEnabled)
        {
            _logger.Debug("MediaInfo created, handle=0x{Handle}", _handle.ToString("X"));
        }

        if (_handle == 0)
        {
            throw new InvalidOperationException("MediaInfo_New failed.");
        }
    }

    public unsafe string? Read(string fileName)
    {
        ThrowIfDisposed();

        fixed (char* p = fileName)
        {
            var ret = Native.MediaInfo_Open(_handle, p);
            if (ret == 0)
            {
                _logger.Error("MediaInfo open failed, ret={}, path={}", ret, fileName);
                return null;
            }
        }

        var info = Native.MediaInfo_Inform(_handle, 0);
        return info != null ? new string(info) : null;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
        }

        if (_handle != 0)
        {
            Native.MediaInfo_Close(_handle);
            Native.MediaInfo_Delete(_handle);
            _handle = 0;
        }

        _disposed = true;
    }

    ~NativeMediaInfo()
    {
        Dispose(false);
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }

    private static unsafe partial class Native
    {
        [LibraryImport("MediaInfo.dll")]
        internal static partial nint MediaInfo_New();

        [LibraryImport("MediaInfo.dll")]
        internal static partial void MediaInfo_Delete(nint handle);

        [LibraryImport("MediaInfo.dll")]
        internal static partial nint MediaInfo_Open(nint handle, char* fileName);

        [LibraryImport("MediaInfo.dll")]
        internal static partial void MediaInfo_Close(nint handle);

        [LibraryImport("MediaInfo.dll")]
        internal static partial char* MediaInfo_Inform(nint handle, nint reserved);
    }
}