using Microsoft.UI.Dispatching;
using System;

namespace mpv_winui.Modules.Player.PlayerControl;

internal sealed class PlayControlAutoHideController : IDisposable
{
    [Flags]
    internal enum AutoHideBlockReason
    {
        None = 0,
        PointerOverPlayControl = 1 << 0,
        PopupOpen = 1 << 1,
        PointerButtonPressed = 1 << 2,
        DiscMenuActive = 1 << 3,
        WindowInactive = 1 << 4,
    }

    private const int MinDelayMs = 200;
    private const int MaxDelayMs = 60000;
    private const int DefaultDelayMs = 3000;
    private const long PointerActivityIntervalMs = 50;

    private readonly DispatcherQueueTimer _idleTimer;
    private readonly Func<bool> _isControlVisible;
    private readonly Action _showControl;
    private readonly Action _hideControl;

    private Func<AutoHideBlockReason>? _blockReasonProvider;
    private bool _enabled = true;
    private bool _isRunning;
    private bool _isDisposed;
    private long _lastPointerActivityTick;

    public PlayControlAutoHideController(
        DispatcherQueue dispatcherQueue,
        Func<bool> isControlVisible,
        Action showControl,
        Action hideControl)
    {
        _isControlVisible = isControlVisible;
        _showControl = showControl;
        _hideControl = hideControl;

        _idleTimer = dispatcherQueue.CreateTimer();
        _idleTimer.IsRepeating = false;
        _idleTimer.Interval = TimeSpan.FromMilliseconds(DefaultDelayMs);
        _idleTimer.Tick += IdleTimer_Tick;
    }

    public bool IsRunning
    {
        get
        {
            return _isRunning;
        }
    }

    public void Configure(bool enabled, int delayMs)
    {
        _enabled = enabled;
        _idleTimer.Interval = TimeSpan.FromMilliseconds(Math.Clamp(delayMs, MinDelayMs, MaxDelayMs));

        if (!_enabled)
        {
            Stop();
        }
    }

    public void SetBlockReasonProvider(Func<AutoHideBlockReason>? provider)
    {
        _blockReasonProvider = provider;
    }

    public void Start()
    {
        if (_isDisposed || !_enabled)
        {
            return;
        }

        _isRunning = true;
        RestartIdleTimer();
    }

    public void Stop()
    {
        _isRunning = false;

        if (!_isDisposed)
        {
            _idleTimer.Stop();
        }
    }

    public void OnPointerActivity()
    {
        if (_isDisposed || !_isRunning || !_enabled)
        {
            return;
        }

        long now = Environment.TickCount64;

        if (now - _lastPointerActivityTick < PointerActivityIntervalMs)
        {
            return;
        }

        _lastPointerActivityTick = now;

        if (!_isControlVisible())
        {
            _showControl();
        }

        RestartIdleTimer();
    }

    private void RestartIdleTimer()
    {
        _idleTimer.Stop();
        _idleTimer.Start();
    }

    private void IdleTimer_Tick(DispatcherQueueTimer sender, object args)
    {
        _idleTimer.Stop();

        if (_isDisposed || !_isRunning || !_enabled)
        {
            return;
        }

        if ((_blockReasonProvider?.Invoke() ?? AutoHideBlockReason.None) != AutoHideBlockReason.None)
        {
            RestartIdleTimer();
            return;
        }

        if (_isControlVisible())
        {
            _hideControl();
        }
    }

    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        _isDisposed = true;
        _idleTimer.Stop();
        _idleTimer.Tick -= IdleTimer_Tick;
        _blockReasonProvider = null;
    }
}
