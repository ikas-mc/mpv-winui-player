#pragma once
#include "PlaybackFailedEventArgs.g.h"

namespace winrt::mpv_winrt::implementation
{
    struct PlaybackFailedEventArgs : PlaybackFailedEventArgsT<PlaybackFailedEventArgs>
    {
        PlaybackFailedEventArgs(hstring const& message) : m_message(message)
        {
        }

        hstring Message()
        {
            return m_message;
        }

    private:
        hstring m_message;
    };
}
