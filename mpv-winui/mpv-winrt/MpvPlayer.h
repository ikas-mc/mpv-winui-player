#pragma once
#include "MpvPlayer.g.h"
#include <atomic>
#include <d3d11_4.h>
#include <dxgi1_6.h>
#include <mpv/client.h>
#include <mutex>
#include <string>
#include <string_view>
#include <thread>
#include <winrt/Windows.Foundation.Collections.h>

namespace winrt::mpv_winrt::implementation
{
    struct MpvPlayer : MpvPlayerT<MpvPlayer>
    {
        MpvPlayer();
        ~MpvPlayer();

        void Initialize(hstring const& configPath, uint32_t width, uint32_t height, int32_t volume);
        void Destroy();
        void AttachSwapChain(winrt::Microsoft::UI::Xaml::Controls::SwapChainPanel const& panel);
        void UpdateSwapChainScale(float scaleX, float scaleY);
        void UpdateSize(uint32_t width, uint32_t height);
        void LoadFile(hstring const& url, double position);
        void LoadList(hstring const& url);

        void Play();
        void Pause();
        void Stop();
        void TogglePlayPause();
        bool IsPaused();

        void Command(winrt::Windows::Foundation::Collections::IVector<hstring> const& args);

        double Volume();
        void Volume(double value);
        bool IsMuted();
        void IsMuted(bool value);

        double Position();
        void Position(double value);
        double Duration();

        int32_t CurrentVideoTrack();
        void CurrentVideoTrack(int32_t value);
        int32_t CurrentAudioTrack();
        void CurrentAudioTrack(int32_t value);
        int32_t CurrentSubtitleTrack();
        void CurrentSubtitleTrack(int32_t value);
        int32_t CurrentSecondSubtitleTrack();
        void CurrentSecondSubtitleTrack(int32_t value);
        void AddSubtitle(hstring const& url, bool const& selected, hstring const& title);

        double PlaybackSpeed();
        void PlaybackSpeed(double value);

        bool LoopFile();
        void LoopFile(bool enabled);
        void SetLoopPlaylist(bool enabled);
        bool LoopPlaylist();
        void SetShuffle(bool enabled);
        bool Shuffle();

        void SetAspectRatio(hstring const& ratio);

        winrt::Windows::Foundation::Collections::IVectorView<winrt::mpv_winrt::MpvPlaylistItem> GetPlaylist();

        winrt::Windows::Foundation::Collections::IVectorView<winrt::mpv_winrt::MpvTrack> GetAudioTracks();
        winrt::Windows::Foundation::Collections::IVectorView<winrt::mpv_winrt::MpvTrack> GetVideoTracks();
        winrt::Windows::Foundation::Collections::IVectorView<winrt::mpv_winrt::MpvTrack> GetSubtitleTracks();
        winrt::Windows::Foundation::Collections::IVectorView<winrt::mpv_winrt::MpvChapter> GetChapters();
        winrt::Windows::Foundation::Collections::IVectorView<winrt::mpv_winrt::MpvEdition> GetEditions();
        winrt::Windows::Foundation::Collections::IVectorView<winrt::mpv_winrt::MpvProfile> GetProfiles();
        winrt::Windows::Foundation::Collections::IVectorView<winrt::mpv_winrt::MpvAudioDevice> GetAudioDevices();
        int32_t CurrentChapter();
        int32_t CurrentEdition();

        winrt::Windows::Foundation::Collections::IVectorView<winrt::mpv_winrt::MpvMenuItem> GetMenu();

        winrt::event_token MediaLoaded(winrt::mpv_winrt::MediaLoadedEventHandler const& handler);
        void MediaLoaded(winrt::event_token const& token) noexcept;
        winrt::event_token PlaybackEnded(winrt::mpv_winrt::PlaybackEndedEventHandler const& handler);
        void PlaybackEnded(winrt::event_token const& token) noexcept;
        winrt::event_token PlaybackFailed(winrt::mpv_winrt::PlaybackFailedEventHandler const& handler);
        void PlaybackFailed(winrt::event_token const& token) noexcept;
        winrt::event_token Seeked(winrt::mpv_winrt::SeekEventHandler const& handler);
        void Seeked(winrt::event_token const& token) noexcept;
        winrt::event_token FileLoaded(winrt::mpv_winrt::FileLoadedEventHandler const& handler);
        void FileLoaded(winrt::event_token const& token) noexcept;
        winrt::event_token TrackChanged(winrt::mpv_winrt::TrackChangedEventHandler const& handler);
        void TrackChanged(winrt::event_token const& token) noexcept;

        winrt::event_token PlaybackStateChanged(winrt::mpv_winrt::PlaybackStateChangedEventHandler const& handler);
        void PlaybackStateChanged(winrt::event_token const& token) noexcept;
        winrt::event_token VolumeChanged(winrt::mpv_winrt::VolumeChangedEventHandler const& handler);
        void VolumeChanged(winrt::event_token const& token) noexcept;
        winrt::event_token PositionChanged(winrt::mpv_winrt::PositionChangedEventHandler const& handler);
        void PositionChanged(winrt::event_token const& token) noexcept;
        winrt::event_token SpeedChanged(winrt::mpv_winrt::SpeedChangedEventHandler const& handler);
        void SpeedChanged(winrt::event_token const& token) noexcept;
        winrt::event_token MediaInfoChanged(winrt::mpv_winrt::MediaInfoChangedEventHandler const& handler);
        void MediaInfoChanged(winrt::event_token const& token) noexcept;
        winrt::event_token NetworkInfoChanged(winrt::mpv_winrt::NetworkInfoChangedEventHandler const& handler);
        void NetworkInfoChanged(winrt::event_token const& token) noexcept;
        winrt::event_token TrackListChanged(winrt::mpv_winrt::TrackListChangedEventHandler const& handler);
        void TrackListChanged(winrt::event_token const& token) noexcept;
        winrt::event_token TrackListCountChanged(winrt::mpv_winrt::TrackListCountChangedEventHandler const& handler);
        void TrackListCountChanged(winrt::event_token const& token) noexcept;
        winrt::event_token VoConfigured(winrt::mpv_winrt::VoConfiguredEventHandler const& handler);
        void VoConfigured(winrt::event_token const& token) noexcept;
        winrt::event_token WindowChanged(winrt::mpv_winrt::WindowChangedEventHandler const& handler);
        void WindowChanged(winrt::event_token const& token) noexcept;

    private:
        static mpv_node* FindMapField(mpv_node* map, const char* key);
        winrt::Windows::Foundation::Collections::IVectorView<winrt::mpv_winrt::MpvTrack> GetTracks(const char* type);
        void CreateContext();
        void SetOption(std::string const& name, std::string const& value);
        void StartEventThread();
        void StopEventThread();
        void ProcessEvents();
        void HandleMpvEvent(mpv_event* event);

        double GetDoubleProperty(const char* name);
        int64_t GetInt64Property(const char* name);
        winrt::hstring GetHStringProperty(const char* name);
        bool IsStringPropertyEqual(const char* name, std::string_view expected);
        void SetDoubleProperty(const char* name, double value);
        void SetInt64Property(const char* name, int64_t value);
        void SetStringProperty(const char* name, const std::string& value);

        mpv_handle* m_mpv{nullptr};
        std::atomic<IDXGISwapChain3*> m_swapChain{nullptr};

        std::thread m_eventThread;
        std::atomic<bool> m_eventThreadRunning{false};

        winrt::event<winrt::mpv_winrt::MediaLoadedEventHandler> m_mediaLoadedEvent;
        winrt::event<winrt::mpv_winrt::PlaybackEndedEventHandler> m_playbackEndedEvent;
        winrt::event<winrt::mpv_winrt::PlaybackFailedEventHandler> m_playbackFailedEvent;
        winrt::event<winrt::mpv_winrt::SeekEventHandler> m_seekedEvent;
        winrt::event<winrt::mpv_winrt::FileLoadedEventHandler> m_fileLoadedEvent;
        winrt::event<winrt::mpv_winrt::TrackChangedEventHandler> m_trackChangedEvent;

        winrt::event<winrt::mpv_winrt::PlaybackStateChangedEventHandler> m_playbackStateChangedEvent;
        winrt::event<winrt::mpv_winrt::VolumeChangedEventHandler> m_volumeChangedEvent;
        winrt::event<winrt::mpv_winrt::PositionChangedEventHandler> m_positionChangedEvent;
        winrt::event<winrt::mpv_winrt::SpeedChangedEventHandler> m_speedChangedEvent;
        winrt::event<winrt::mpv_winrt::MediaInfoChangedEventHandler> m_mediaInfoChangedEvent;
        winrt::event<winrt::mpv_winrt::NetworkInfoChangedEventHandler> m_networkInfoChangedEvent;
        winrt::event<winrt::mpv_winrt::TrackListChangedEventHandler> m_trackListChangedEvent;
        winrt::event<winrt::mpv_winrt::TrackListCountChangedEventHandler> m_trackListCountChangedEvent;
        winrt::event<winrt::mpv_winrt::VoConfiguredEventHandler> m_voConfiguredEvent;
        winrt::event<winrt::mpv_winrt::WindowChangedEventHandler> m_windowChangedEvent;
    };
}

namespace winrt::mpv_winrt::factory_implementation
{
    struct MpvPlayer : MpvPlayerT<MpvPlayer, implementation::MpvPlayer>
    {
    };
}
