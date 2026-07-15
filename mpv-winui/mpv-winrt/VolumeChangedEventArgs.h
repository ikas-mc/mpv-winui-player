#pragma once
#include "VolumeChangedEventArgs.g.h"

namespace winrt::mpv_winrt::implementation
{
    struct VolumeChangedEventArgs : VolumeChangedEventArgsT<VolumeChangedEventArgs>
    {
        VolumeChangedEventArgs(double volume, bool isMuted) : m_volume(volume), m_isMuted(isMuted)
        {
        }

        double Volume()
        {
            return m_volume;
        }
        bool IsMuted()
        {
            return m_isMuted;
        }

    private:
        double m_volume;
        bool m_isMuted;
    };
}
