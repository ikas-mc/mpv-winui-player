#pragma once
#include "MpvTrack.g.h"

namespace winrt::mpv_winrt::implementation
{
    struct MpvTrack : MpvTrackT<MpvTrack>
    {
        MpvTrack(int32_t index, int32_t id, winrt::mpv_winrt::TrackType type, hstring const& title,
                 hstring const& lang, bool selected, hstring const& codec, bool isDefault,
                 int32_t demuxChannelCount, int32_t demuxSamplerate)
            : m_index(index), m_id(id), m_type(type), m_title(title), m_lang(lang), m_selected(selected),
              m_codec(codec), m_isDefault(isDefault),
              m_demuxChannelCount(demuxChannelCount), m_demuxSamplerate(demuxSamplerate)
        {
        }

        MpvTrack(int32_t index, int32_t id, winrt::mpv_winrt::TrackType type, hstring const& title,
                 hstring const& lang, bool selected, hstring const& codec, bool isDefault,
                 int32_t demuxW, int32_t demuxH, double demuxFps)
            : m_index(index), m_id(id), m_type(type), m_title(title), m_lang(lang), m_selected(selected),
              m_codec(codec), m_isDefault(isDefault),
              m_demuxW(demuxW), m_demuxH(demuxH), m_demuxFps(demuxFps)
        {
        }

        MpvTrack(int32_t index, int32_t id, winrt::mpv_winrt::TrackType type, hstring const& title,
                 hstring const& lang, bool selected, hstring const& codec, bool isDefault,
                 bool isForced, bool isExternal)
            : m_index(index), m_id(id), m_type(type), m_title(title), m_lang(lang), m_selected(selected),
              m_codec(codec), m_isDefault(isDefault),
              m_isForced(isForced), m_isExternal(isExternal)
        {
        }

        int32_t Index() { return m_index; }
        int32_t Id() { return m_id; }
        winrt::mpv_winrt::TrackType Type() { return m_type; }
        hstring Title() { return m_title; }
        hstring Lang() { return m_lang; }
        bool Selected() { return m_selected; }
        hstring Codec() { return m_codec; }
        bool IsDefault() { return m_isDefault; }
        bool IsForced() { return m_isForced; }
        bool IsExternal() { return m_isExternal; }
        int32_t DemuxW() { return m_demuxW; }
        int32_t DemuxH() { return m_demuxH; }
        double DemuxFps() { return m_demuxFps; }
        int32_t DemuxChannelCount() { return m_demuxChannelCount; }
        int32_t DemuxSamplerate() { return m_demuxSamplerate; }

    private:
        int32_t m_index{};
        int32_t m_id{};
        winrt::mpv_winrt::TrackType m_type{};
        hstring m_title;
        hstring m_lang;
        bool m_selected{};
        hstring m_codec;
        bool m_isDefault{};
        bool m_isForced{};
        bool m_isExternal{};
        int32_t m_demuxW{};
        int32_t m_demuxH{};
        double m_demuxFps{};
        int32_t m_demuxChannelCount{};
        int32_t m_demuxSamplerate{};
    };
}
