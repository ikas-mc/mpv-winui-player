#pragma once
#include "PositionChangedEventArgs.g.h"

namespace winrt::mpv_winrt::implementation
{
    struct PositionChangedEventArgs : PositionChangedEventArgsT<PositionChangedEventArgs>
    {
        PositionChangedEventArgs(double position, double duration, double percentPos)
            : m_position(position), m_duration(duration), m_percentPos(percentPos)
        {
        }

        double Position()
        {
            return m_position;
        }
        double Duration()
        {
            return m_duration;
        }
        double PercentPosition()
        {
            return m_percentPos;
        }

    private:
        double m_position;
        double m_duration;
        double m_percentPos;
    };
}
