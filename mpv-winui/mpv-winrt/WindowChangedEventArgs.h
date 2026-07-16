#pragma once
#include "WindowChangedEventArgs.g.h"

namespace winrt::mpv_winrt::implementation
{
    struct WindowChangedEventArgs : WindowChangedEventArgsT<WindowChangedEventArgs>
    {
        WindowChangedEventArgs(winrt::hstring const& propertyName, int32_t propertyId, bool value) :
            m_propertyName(propertyName), m_propertyId(propertyId), m_value(value)
        {
        }

        winrt::hstring PropertyName()
        {
            return m_propertyName;
        }

        int32_t PropertyId()
        {
            return m_propertyId;
        }

        bool Value()
        {
            return m_value;
        }

    private:
        winrt::hstring m_propertyName;
        int32_t m_propertyId;
        bool m_value;
    };
}