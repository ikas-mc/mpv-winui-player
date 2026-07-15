#pragma once
#include "MpvChapter.g.h"

namespace winrt::mpv_winrt::implementation
{
    struct MpvChapter : MpvChapterT<MpvChapter>
    {
        MpvChapter(int32_t id, hstring const& title, double time)
            : m_id(id), m_title(title), m_time(time)
        {
        }

        int32_t Id()
        {
            return m_id;
        }
        hstring Title()
        {
            return m_title;
        }
        double Time()
        {
            return m_time;
        }

    private:
        int32_t m_id;
        hstring m_title;
        double m_time;
    };
}
