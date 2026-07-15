#pragma once
#include "MpvProfile.g.h"

namespace winrt::mpv_winrt::implementation
{
    struct MpvProfile : MpvProfileT<MpvProfile>
    {
        MpvProfile(hstring const& name)
            : m_name(name)
        {
        }

        hstring Name()
        {
            return m_name;
        }

    private:
        hstring m_name;
    };
}
