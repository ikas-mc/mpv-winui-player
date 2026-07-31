#pragma once
#include "MpvMenuItem.g.h"

namespace winrt::mpv_winrt::implementation
{
    struct MpvMenuItem: MpvMenuItemT<MpvMenuItem>
    {
        MpvMenuItem(hstring const& title, hstring const& command, hstring const& type, bool isChecked, bool isDisabled, bool isHidden, winrt::Windows::Foundation::Collections::IVectorView<winrt::mpv_winrt::MpvMenuItem> const& items)
            : m_title(title), m_command(command), m_type(type),
            m_isChecked(isChecked), m_isDisabled(isDisabled), m_isHidden(isHidden), m_items(items)
        {
        }

        hstring Title()
        {
            return m_title;
        }
        hstring Command()
        {
            return m_command;
        }
        hstring Type()
        {
            return m_type;
        }
        bool IsChecked()
        {
            return m_isChecked;
        }
        bool IsDisabled()
        {
            return m_isDisabled;
        }
        bool IsHidden()
        {
            return m_isHidden;
        }
        winrt::Windows::Foundation::Collections::IVectorView<winrt::mpv_winrt::MpvMenuItem> Items()
        {
            return m_items;
        }

    private:
        hstring m_title;
        hstring m_command;
        hstring m_type;
        bool m_isChecked{};
        bool m_isDisabled{};
        bool m_isHidden{};
        winrt::Windows::Foundation::Collections::IVectorView<winrt::mpv_winrt::MpvMenuItem> m_items;
    };
}
