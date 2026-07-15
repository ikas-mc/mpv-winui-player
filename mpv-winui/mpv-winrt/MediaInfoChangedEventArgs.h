#pragma once
#include "MediaInfoChangedEventArgs.g.h"

namespace winrt::mpv_winrt::implementation
{
    struct MediaInfoChangedEventArgs : MediaInfoChangedEventArgsT<MediaInfoChangedEventArgs>
    {
        MediaInfoChangedEventArgs(hstring const& filename, hstring const& title)
            : m_filename(filename), m_title(title)
        {
        }

        hstring Filename()
        {
            return m_filename;
        }
        hstring MediaTitle()
        {
            return m_title;
        }

    private:
        hstring m_filename;
        hstring m_title;
    };
}
