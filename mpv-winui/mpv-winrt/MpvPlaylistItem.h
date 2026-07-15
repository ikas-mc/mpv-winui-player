#pragma once
#include "MpvPlaylistItem.g.h"

namespace winrt::mpv_winrt::implementation
{
    struct MpvPlaylistItem : MpvPlaylistItemT<MpvPlaylistItem>
    {
        MpvPlaylistItem(int32_t id, hstring const& filename, hstring const& title, bool isCurrent, bool isPlaying)
            : m_id(id), m_filename(filename), m_title(title), m_isCurrent(isCurrent), m_isPlaying(isPlaying)
        {
        }

        int32_t Id()
        {
            return m_id;
        }
        hstring Filename()
        {
            return m_filename;
        }
        hstring Title()
        {
            return m_title;
        }
        bool IsCurrent()
        {
            return m_isCurrent;
        }
        bool IsPlaying()
        {
            return m_isPlaying;
        }

    private:
        int32_t m_id;
        hstring m_filename;
        hstring m_title;
        bool m_isCurrent;
        bool m_isPlaying;
    };
}
