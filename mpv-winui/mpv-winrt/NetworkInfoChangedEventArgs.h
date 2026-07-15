#pragma once
#include "NetworkInfoChangedEventArgs.g.h"

namespace winrt::mpv_winrt::implementation
{
    struct NetworkInfoChangedEventArgs : NetworkInfoChangedEventArgsT<NetworkInfoChangedEventArgs>
    {
        NetworkInfoChangedEventArgs(int64_t cacheSpeed) : m_cacheSpeed(cacheSpeed)
        {
        }

        int64_t CacheSpeed()
        {
            return m_cacheSpeed;
        }

    private:
        int64_t m_cacheSpeed;
    };
}
