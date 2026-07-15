#pragma once
#include "TrackListChangedEventArgs.g.h"

namespace winrt::mpv_winrt::implementation
{
    struct TrackListChangedEventArgs : TrackListChangedEventArgsT<TrackListChangedEventArgs>
    {
        TrackListChangedEventArgs(
            winrt::Windows::Foundation::Collections::IVectorView<winrt::mpv_winrt::MpvTrack> const& tracks)
            : m_tracks(tracks)
        {
        }

        winrt::Windows::Foundation::Collections::IVectorView<winrt::mpv_winrt::MpvTrack> Tracks()
        {
            return m_tracks;
        }

    private:
        winrt::Windows::Foundation::Collections::IVectorView<winrt::mpv_winrt::MpvTrack> m_tracks;
    };
}
