#pragma once
#include "PlaybackStateChangedEventArgs.g.h"

namespace winrt::mpv_winrt::implementation
{
    struct PlaybackStateChangedEventArgs : PlaybackStateChangedEventArgsT<PlaybackStateChangedEventArgs>
    {
        PlaybackStateChangedEventArgs(bool isPaused, bool isIdle) : m_isPaused(isPaused), m_isIdle(isIdle)
        {
        }

        bool IsPaused()
        {
            return m_isPaused;
        }
        bool IsIdle()
        {
            return m_isIdle;
        }

    private:
        bool m_isPaused;
        bool m_isIdle;
    };
}
