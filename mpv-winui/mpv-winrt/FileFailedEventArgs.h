#pragma once
#include "FileFailedEventArgs.g.h"

namespace winrt::mpv_winrt::implementation
{
    struct FileFailedEventArgs : FileFailedEventArgsT<FileFailedEventArgs>
    {
        FileFailedEventArgs(hstring const& message) : m_message(message)
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