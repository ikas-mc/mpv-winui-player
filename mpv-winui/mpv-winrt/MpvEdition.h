#pragma once
#include "MpvEdition.g.h"

namespace winrt::mpv_winrt::implementation
{
    struct MpvEdition : MpvEditionT<MpvEdition>
    {
        MpvEdition(int32_t id, hstring const& title)
            : m_id(id), m_title(title)
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

    private:
        int32_t m_id;
        hstring m_title;
    };
}
