#pragma once
#include "SpeedChangedEventArgs.g.h"

namespace winrt::mpv_winrt::implementation
{
    struct SpeedChangedEventArgs : SpeedChangedEventArgsT<SpeedChangedEventArgs>
    {
        SpeedChangedEventArgs(double speed) : m_speed(speed)
        {
        }

        double Speed()
        {
            return m_speed;
        }

    private:
        double m_speed;
    };
}
