#pragma once
#include "MpvPlaylistItem.g.h"

namespace winrt::mpv_winrt::implementation
{
    struct MpvPlaylistItem : MpvPlaylistItemT<MpvPlaylistItem>
    {
        MpvPlaylistItem(int32_t id, int32_t index, hstring const& filename, hstring const& title, bool isCurrent, bool isPlaying)
            : m_id(id), m_index(index), m_filename(filename), m_title(title), m_isCurrent(isCurrent), m_isPlaying(isPlaying)
        {
        }

        int32_t Id()
        {
            return m_id;
        }
        int32_t Index()
        {
            return m_index;
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
        int32_t m_index;
        hstring m_filename;
        hstring m_title;
        bool m_isCurrent;
        bool m_isPlaying;
    };
}
