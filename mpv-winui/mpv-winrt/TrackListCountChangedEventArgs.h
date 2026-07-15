#pragma once
#include "TrackListCountChangedEventArgs.g.h"

namespace winrt::mpv_winrt::implementation
{
    struct TrackListCountChangedEventArgs : TrackListCountChangedEventArgsT<TrackListCountChangedEventArgs>
    {
        TrackListCountChangedEventArgs(int32_t const& count) : m_count(count)
        {
        }

        int32_t Count()
        {
            return m_count;
        }

    private:
        int32_t m_count;
    };
}
