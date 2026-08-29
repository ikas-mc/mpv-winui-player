#include "pch.h"
#include "MpvPlayer.h"
#include "MpvPlayer.g.cpp"
#include "MediaInfoChangedEventArgs.h"
#include "MpvAudioDevice.h"
#include "MpvChapter.h"
#include "MpvEdition.h"
#include "MpvMenuItem.h"
#include "MpvPlaylistItem.h"
#include "MpvProfile.h"
#include "MpvTrack.h"
#include "NetworkInfoChangedEventArgs.h"
#include "FileFailedEventArgs.h"
#include "PlaybackStateChangedEventArgs.h"
#include "PositionChangedEventArgs.h"
#include "SpeedChangedEventArgs.h"
#include "TrackListChangedEventArgs.h"
#include "TrackListCountChangedEventArgs.h"
#include "VolumeChangedEventArgs.h"
#include "WindowChangedEventArgs.h"
#include <vector>
#include <winrt/Windows.Foundation.Collections.h>
#include <winrt/Microsoft.UI.Xaml.Controls.h>
#include <microsoft.ui.xaml.media.dxinterop.h>

using namespace winrt;
using namespace Microsoft::UI::Xaml::Controls;
using namespace Windows::Foundation::Collections;

namespace winrt::mpv_winrt::implementation
{
    MpvPlayer::MpvPlayer()
    {
    }

    MpvPlayer::~MpvPlayer()
    {
        if (m_mpv)
        {
            StopEventThread();
            mpv_terminate_destroy(m_mpv);
            m_mpv = nullptr;
            m_swapChain = nullptr;
        }
    }

    void MpvPlayer::CreateContext()
    {
        m_mpv = mpv_create();
        if (!m_mpv)
        {
            throw hresult_error(E_FAIL, L"Failed to create mpv context");
        }
    }

    void MpvPlayer::Destroy()
    {
        StopEventThread();
        if (m_mpv)
        {
            mpv_terminate_destroy(m_mpv);
            m_mpv = nullptr;
        }
        m_swapChain.store(nullptr);
    }

    void MpvPlayer::Initialize(hstring const& configPath, uint32_t width, uint32_t height, int32_t volume, winrt::mpv_winrt::DisplayColorKind colorKind, int32_t refreshRate)
    {
        CreateContext();
        SetOption("config", "yes");
        SetOption("config-dir", to_string(configPath));

        SetOption("gpu-shader-cache-dir", "~~/cache/shaders_cache");
        SetOption("screenshot-dir", "~~/screenshots");
        SetOption("osc", "no");
        SetOption("idle", "yes");

        SetOption("script-opts", "select-populate_menu_data=yes");
        SetOption("load-select", "yes");
        SetOption("input-default-bindings", "yes");
        // SetOption("input-vo-keyboard", "yes");
        SetOption("input-media-keys", "yes");
        SetOption("media-controls", "yes");

        SetOption("reset-on-next-file", "pause,ab-loop-a,ab-loop-b");
        // SetOption("ao", "wasapi"); // Use WASAPI audio output for Windows
        SetOption("volume", std::to_string(volume));

        SetOption("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64)");
        SetOption("gpu-api", "d3d11");
        SetOption("d3d11-output-mode", "composition");
        SetOption("auto-window-resize", "no");
        SetOption("force-window", "yes");
        SetOption("d3d11-composition-size", std::to_string(width) + "x" + std::to_string(height));

        if (mpv_initialize(m_mpv) < 0)
        {
            throw hresult_error(E_FAIL, L"Failed to initialize mpv");
        }

        UpdateDisplayColorInfo(colorKind);
        UpdateDisplayRefreshRate(refreshRate);

        mpv_observe_property(m_mpv, MpvObserveId::CoreIdle, "core-idle", MPV_FORMAT_FLAG);
        mpv_observe_property(m_mpv, MpvObserveId::Pause, "pause", MPV_FORMAT_FLAG);
        mpv_observe_property(m_mpv, MpvObserveId::PausedForCache, "paused-for-cache", MPV_FORMAT_FLAG);
        mpv_observe_property(m_mpv, MpvObserveId::Duration, "duration", MPV_FORMAT_DOUBLE);
        // mpv_observe_property(m_mpv, MpvObserveId::PlaybackTime, "playback-time", MPV_FORMAT_DOUBLE);
        // mpv_observe_property(m_mpv, MpvObserveId::TimePos, "time-pos", MPV_FORMAT_DOUBLE);

        mpv_observe_property(m_mpv, MpvObserveId::LoopFile, "loop-file", MPV_FORMAT_STRING);
        mpv_observe_property(m_mpv, MpvObserveId::LoopPlaylist, "loop-playlist", MPV_FORMAT_STRING);
        mpv_observe_property(m_mpv, MpvObserveId::Shuffle, "shuffle", MPV_FORMAT_STRING);
        mpv_observe_property(m_mpv, MpvObserveId::Playlist, "playlist", MPV_FORMAT_NODE);

        // mpv_observe_property(m_mpv, MpvObserveId::CacheSpeed, "cache-speed", MPV_FORMAT_INT64);
        mpv_observe_property(m_mpv, MpvObserveId::Speed, "speed", MPV_FORMAT_DOUBLE);

        // Audio properties
        mpv_observe_property(m_mpv, MpvObserveId::Volume, "volume", MPV_FORMAT_DOUBLE);
        mpv_observe_property(m_mpv, MpvObserveId::Mute, "mute", MPV_FORMAT_FLAG);

        // mpv_observe_property(m_mpv, MpvObserveId::Aid, "aid", MPV_FORMAT_INT64);
        // mpv_observe_property(m_mpv, MpvObserveId::Sid, "sid", MPV_FORMAT_INT64);

        // mpv_observe_property(m_mpv, MpvObserveId::Filename, "filename", MPV_FORMAT_STRING);
        mpv_observe_property(m_mpv, MpvObserveId::MediaTitle, "media-title", MPV_FORMAT_STRING);

        // mpv_observe_property(m_mpv, MpvObserveId::TrackList, "track-list", MPV_FORMAT_NODE);
        // mpv_observe_property(m_mpv, MpvObserveId::TrackListCount, "track-list/count", MPV_FORMAT_INT64);
        // mpv_observe_property(m_mpv, MpvObserveId::MenuData, "menu-data", MPV_FORMAT_NODE);

        mpv_observe_property(m_mpv, MpvObserveId::Fullscreen, "fullscreen", MPV_FORMAT_FLAG);
        mpv_observe_property(m_mpv, MpvObserveId::Ontop, "ontop", MPV_FORMAT_FLAG);
        mpv_observe_property(m_mpv, MpvObserveId::WindowMinimized, "window-minimized", MPV_FORMAT_FLAG);
        mpv_observe_property(m_mpv, MpvObserveId::WindowMaximized, "window-maximized", MPV_FORMAT_FLAG);
        mpv_observe_property(m_mpv, MpvObserveId::TitleBar, "title-bar", MPV_FORMAT_FLAG);
        mpv_observe_property(m_mpv, MpvObserveId::Border, "border", MPV_FORMAT_FLAG);

        // mpv_get_property(m_mpv, "display-swapchain", MPV_FORMAT_INT64, &m_swapChain);

        StartEventThread();
    }

    void MpvPlayer::InitializeForPreview(uint32_t width, uint32_t height)
    {
        CreateContext();

        SetOption("config", "no");
        SetOption("msg-level", "all=no");
        SetOption("profile", "fast");
        SetOption("osc", "no");
        SetOption("load-scripts", "no");
        SetOption("idle", "yes");
        SetOption("keep-open", "yes");
        SetOption("pause", "yes");
        SetOption("really-quiet", "yes");
        SetOption("load-stats-overlay", "no");
        SetOption("load-stats-console", "no");
        SetOption("load-auto-profiles", "no");
        SetOption("sub", "no");
        SetOption("hr-seek", "no");
        SetOption("audio", "no");
        SetOption("input-default-bindings", "no");
        SetOption("input-media-keys", "no");
        SetOption("media-controls", "no");
        SetOption("terminal", "no");
        SetOption("cache", "no");

        SetOption("force-window", "yes");
        SetOption("gpu-api", "d3d11");
        SetOption("d3d11-output-mode", "composition");
        SetOption("auto-window-resize", "no");
        SetOption("d3d11-composition-size", std::to_string(width) + "x" + std::to_string(height));


        if (mpv_initialize(m_mpv) < 0)
        {
            throw hresult_error(E_FAIL, L"Failed to initialize mpv for preview");
        }

        StartEventThread();
    }

    void MpvPlayer::StartEventThread()
    {
        if (m_eventThreadRunning.load())
        {
            return;
        }

        m_eventThreadRunning.store(true);
        m_eventThread = std::thread([this]()
        { ProcessEvents(); });
    }

    void MpvPlayer::StopEventThread()
    {
        if (!m_eventThreadRunning.load())
        {
            return;
        }

        m_eventThreadRunning.store(false);

        if (m_mpv)
        {
            mpv_wakeup(m_mpv);
        }

        if (m_eventThread.joinable())
        {
            m_eventThread.join();
        }
    }

    void MpvPlayer::ProcessEvents()
    {
        while (m_eventThreadRunning.load())
        {
            if (!m_mpv)
            {
                break;
            }

            mpv_event* event = mpv_wait_event(m_mpv, 1.0); // Wait up to 1 second
            if (event->event_id == MPV_EVENT_NONE)
            {
                continue;
            }

            if (event->event_id == MPV_EVENT_SHUTDOWN)
            {
                break;
            }

            HandleMpvEvent(event);
        }
    }

    void MpvPlayer::HandleMpvEvent(mpv_event* event)
    {
        switch (event->event_id)
        {
            case MPV_EVENT_START_FILE:
                {
                    m_fileStartedEvent();
                    break;
                }

            case MPV_EVENT_FILE_LOADED:
                {
                    m_fileLoadedEvent();
                    break;
                }

            case MPV_EVENT_PLAYBACK_RESTART:
                {
                    m_playbackRestartedEvent();
                    break;
                }

            case MPV_EVENT_END_FILE:
                {
                    auto end_file = static_cast<mpv_event_end_file*>(event->data);
                    if (end_file->reason == MPV_END_FILE_REASON_EOF)
                    {
                        m_fileEndedEvent();
                    }
                    else if (end_file->reason == MPV_END_FILE_REASON_ERROR)
                    {
                        auto args = winrt::make<implementation::FileFailedEventArgs>(
                            winrt::to_hstring(mpv_error_string(end_file->error)));
                        m_fileFailedEvent(args);
                    }
                    else if (end_file->reason == MPV_END_FILE_REASON_STOP)
                    {
                        m_fileStoppedEvent();
                    }
                    break;
                }

            case MPV_EVENT_SEEK:
                {
                    m_seekStartedEvent();
                    break;
                }

            case MPV_EVENT_VIDEO_RECONFIG:
                {
                    IDXGISwapChain* swapChain = nullptr;
                    mpv_get_property(m_mpv, "display-swapchain", MPV_FORMAT_INT64, &swapChain);
                    if (swapChain != m_swapChain.load())
                    {
                        m_swapChain.store(swapChain);
                        m_swapChainChangedEvent();
                    }
                    break;
                }

            case MPV_EVENT_PROPERTY_CHANGE:
                {
                    auto prop = static_cast<mpv_event_property*>(event->data);
                    if (!prop)
                    {
                        break;
                    }

                    switch (event->reply_userdata)
                    {
                        case MpvObserveId::CoreIdle:
                            break;

                        case MpvObserveId::PausedForCache:
                            {
                                int buffering = prop->data ? *static_cast<int*>(prop->data) : 0;
                                m_bufferingChangedEvent(buffering != 0);
                                break;
                            }

                        case MpvObserveId::Pause:
                            {
                                int video_paused = prop->data ? *static_cast<int*>(prop->data) : 0;
                                auto args = winrt::make<implementation::PlaybackStateChangedEventArgs>(video_paused, false);
                                m_playbackStateChangedEvent(args);
                                break;
                            }

                        case MpvObserveId::Volume:
                        case MpvObserveId::Mute:
                            {
                                double volume = GetDoubleProperty("volume");
                                bool isMuted = IsStringPropertyEqual("mute", "yes");
                                auto args = winrt::make<implementation::VolumeChangedEventArgs>(volume, isMuted);
                                m_volumeChangedEvent(args);
                                break;
                            }

                        case MpvObserveId::PlaybackTime:
                        case MpvObserveId::TimePos:
                        case MpvObserveId::Duration:
                            {
                                double position = GetDoubleProperty("time-pos");
                                double duration = GetDoubleProperty("duration");
                                double percentPos = GetDoubleProperty("percent-pos");
                                auto args = winrt::make<implementation::PositionChangedEventArgs>(
                                    position, duration, percentPos);
                                m_positionChangedEvent(args);
                                break;
                            }

                        case MpvObserveId::Speed:
                            {
                                double speed = GetDoubleProperty("speed");
                                auto args = winrt::make<implementation::SpeedChangedEventArgs>(speed);
                                m_speedChangedEvent(args);
                                break;
                            }

                        case MpvObserveId::CacheSpeed:
                            {
                                int64_t cacheSpeed = GetInt64Property("cache-speed");
                                auto args = winrt::make<implementation::NetworkInfoChangedEventArgs>(cacheSpeed);
                                m_networkInfoChangedEvent(args);
                                break;
                            }

                        case MpvObserveId::Filename:
                        case MpvObserveId::MediaTitle:
                            {
                                auto args = winrt::make<implementation::MediaInfoChangedEventArgs>(GetHStringProperty("filename"), GetHStringProperty("media-title"));
                                m_mediaInfoChangedEvent(args);
                                break;
                            }

                        case MpvObserveId::LoopFile:
                            {
                                m_loopFileChangedEvent();
                                break;
                            }

                        case MpvObserveId::LoopPlaylist:
                            {
                                m_loopPlaylistChangedEvent();
                                break;
                            }

                        case MpvObserveId::Shuffle:
                            {
                                m_shuffleChangedEvent();
                                break;
                            }

                        case MpvObserveId::Playlist:
                            {
                                m_playlistChangedEvent();
                                break;
                            }

                        case MpvObserveId::Aid:
                        case MpvObserveId::Sid:
                            {
                                m_trackChangedEvent();
                                break;
                            }

                        case MpvObserveId::MenuData:
                            {
                                if (prop->format == MPV_FORMAT_NODE)
                                {
                                    mpv_node* root = static_cast<mpv_node*>(prop->data);
                                    if (root && root->format == MPV_FORMAT_NODE_ARRAY)
                                    {
                                        // TODO
                                    }
                                }
                                break;
                            }

                        case MpvObserveId::TrackListCount:
                            {
                                if (prop->format == MPV_FORMAT_INT64 && prop->data)
                                {
                                    auto count = *static_cast<int*>(prop->data);
                                    auto args = winrt::make<implementation::TrackListCountChangedEventArgs>(count);
                                    m_trackListCountChangedEvent(args);
                                }
                                break;
                            }

                        case MpvObserveId::TrackList:
                            {
                                if (prop->format == MPV_FORMAT_NODE && prop->data)
                                {
                                    // TODO
                                    auto tracks = winrt::single_threaded_vector<winrt::mpv_winrt::MpvTrack>();
                                    auto args = winrt::make<implementation::TrackListChangedEventArgs>(tracks.GetView());
                                    m_trackListChangedEvent(args);
                                }
                                break;
                            }

                        case MpvObserveId::Fullscreen:
                        case MpvObserveId::Ontop:
                        case MpvObserveId::WindowMinimized:
                        case MpvObserveId::WindowMaximized:
                        case MpvObserveId::TitleBar:
                        case MpvObserveId::Border:
                            {
                                bool value = prop->data ? *static_cast<int*>(prop->data) != 0 : false;
                                auto args = winrt::make<implementation::WindowChangedEventArgs>(
                                    winrt::to_hstring(prop->name), static_cast<int32_t>(event->reply_userdata), value);
                                m_windowChangedEvent(args);
                                break;
                            }

                        default:
                            break;
                    }
                    break;
                }
        }
    }

    winrt::event_token MpvPlayer::FileStarted(winrt::mpv_winrt::FileStartedEventHandler const& handler)
    {
        return m_fileStartedEvent.add(handler);
    }

    void MpvPlayer::FileStarted(winrt::event_token const& token) noexcept
    {
        m_fileStartedEvent.remove(token);
    }

    winrt::event_token MpvPlayer::FileEnded(winrt::mpv_winrt::FileEndedEventHandler const& handler)
    {
        return m_fileEndedEvent.add(handler);
    }

    void MpvPlayer::FileEnded(winrt::event_token const& token) noexcept
    {
        m_fileEndedEvent.remove(token);
    }

    winrt::event_token MpvPlayer::FileFailed(winrt::mpv_winrt::FileFailedEventHandler const& handler)
    {
        return m_fileFailedEvent.add(handler);
    }

    void MpvPlayer::FileFailed(winrt::event_token const& token) noexcept
    {
        m_fileFailedEvent.remove(token);
    }

    winrt::event_token MpvPlayer::SeekStarted(winrt::mpv_winrt::SeekStartedEventHandler const& handler)
    {
        return m_seekStartedEvent.add(handler);
    }

    void MpvPlayer::SeekStarted(winrt::event_token const& token) noexcept
    {
        m_seekStartedEvent.remove(token);
    }

    winrt::event_token MpvPlayer::FileLoaded(winrt::mpv_winrt::FileLoadedEventHandler const& handler)
    {
        return m_fileLoadedEvent.add(handler);
    }

    void MpvPlayer::FileLoaded(winrt::event_token const& token) noexcept
    {
        m_fileLoadedEvent.remove(token);
    }

    winrt::event_token MpvPlayer::FileStopped(winrt::mpv_winrt::FileStoppedEventHandler const& handler)
    {
        return m_fileStoppedEvent.add(handler);
    }

    void MpvPlayer::FileStopped(winrt::event_token const& token) noexcept
    {
        m_fileStoppedEvent.remove(token);
    }

    winrt::event_token MpvPlayer::PlaybackRestarted(winrt::mpv_winrt::PlaybackRestartedEventHandler const& handler)
    {
        return m_playbackRestartedEvent.add(handler);
    }

    void MpvPlayer::PlaybackRestarted(winrt::event_token const& token) noexcept
    {
        m_playbackRestartedEvent.remove(token);
    }

    winrt::event_token MpvPlayer::TrackChanged(winrt::mpv_winrt::TrackChangedEventHandler const& handler)
    {
        return m_trackChangedEvent.add(handler);
    }

    void MpvPlayer::TrackChanged(winrt::event_token const& token) noexcept
    {
        m_trackChangedEvent.remove(token);
    }

    winrt::event_token MpvPlayer::BufferingChanged(winrt::mpv_winrt::BufferingChangedEventHandler const& handler)
    {
        return m_bufferingChangedEvent.add(handler);
    }

    void MpvPlayer::BufferingChanged(winrt::event_token const& token) noexcept
    {
        m_bufferingChangedEvent.remove(token);
    }

    winrt::event_token MpvPlayer::PlaybackStateChanged(
        winrt::mpv_winrt::PlaybackStateChangedEventHandler const& handler)
    {
        return m_playbackStateChangedEvent.add(handler);
    }

    void MpvPlayer::PlaybackStateChanged(winrt::event_token const& token) noexcept
    {
        m_playbackStateChangedEvent.remove(token);
    }

    winrt::event_token MpvPlayer::VolumeChanged(winrt::mpv_winrt::VolumeChangedEventHandler const& handler)
    {
        return m_volumeChangedEvent.add(handler);
    }

    void MpvPlayer::VolumeChanged(winrt::event_token const& token) noexcept
    {
        m_volumeChangedEvent.remove(token);
    }

    winrt::event_token MpvPlayer::PositionChanged(winrt::mpv_winrt::PositionChangedEventHandler const& handler)
    {
        return m_positionChangedEvent.add(handler);
    }

    void MpvPlayer::PositionChanged(winrt::event_token const& token) noexcept
    {
        m_positionChangedEvent.remove(token);
    }

    winrt::event_token MpvPlayer::SpeedChanged(winrt::mpv_winrt::SpeedChangedEventHandler const& handler)
    {
        return m_speedChangedEvent.add(handler);
    }

    void MpvPlayer::SpeedChanged(winrt::event_token const& token) noexcept
    {
        m_speedChangedEvent.remove(token);
    }

    winrt::event_token MpvPlayer::MediaInfoChanged(winrt::mpv_winrt::MediaInfoChangedEventHandler const& handler)
    {
        return m_mediaInfoChangedEvent.add(handler);
    }

    void MpvPlayer::MediaInfoChanged(winrt::event_token const& token) noexcept
    {
        m_mediaInfoChangedEvent.remove(token);
    }

    winrt::event_token MpvPlayer::NetworkInfoChanged(winrt::mpv_winrt::NetworkInfoChangedEventHandler const& handler)
    {
        return m_networkInfoChangedEvent.add(handler);
    }

    void MpvPlayer::NetworkInfoChanged(winrt::event_token const& token) noexcept
    {
        m_networkInfoChangedEvent.remove(token);
    }

    winrt::event_token MpvPlayer::TrackListChanged(winrt::mpv_winrt::TrackListChangedEventHandler const& handler)
    {
        return m_trackListChangedEvent.add(handler);
    }

    void MpvPlayer::TrackListChanged(winrt::event_token const& token) noexcept
    {
        m_trackListChangedEvent.remove(token);
    }

    winrt::event_token MpvPlayer::TrackListCountChanged(
        winrt::mpv_winrt::TrackListCountChangedEventHandler const& handler)
    {
        return m_trackListCountChangedEvent.add(handler);
    }

    void MpvPlayer::TrackListCountChanged(winrt::event_token const& token) noexcept
    {
        m_trackListCountChangedEvent.remove(token);
    }

    winrt::event_token MpvPlayer::SwapChainChanged(winrt::mpv_winrt::SwapChainChangedEventHandler const& handler)
    {
        return m_swapChainChangedEvent.add(handler);
    }

    void MpvPlayer::SwapChainChanged(winrt::event_token const& token) noexcept
    {
        m_swapChainChangedEvent.remove(token);
    }

    winrt::event_token MpvPlayer::WindowChanged(winrt::mpv_winrt::WindowChangedEventHandler const& handler)
    {
        return m_windowChangedEvent.add(handler);
    }

    void MpvPlayer::WindowChanged(winrt::event_token const& token) noexcept
    {
        m_windowChangedEvent.remove(token);
    }

    winrt::event_token MpvPlayer::LoopFileChanged(winrt::mpv_winrt::LoopFileChangedEventHandler const& handler)
    {
        return m_loopFileChangedEvent.add(handler);
    }

    void MpvPlayer::LoopFileChanged(winrt::event_token const& token) noexcept
    {
        m_loopFileChangedEvent.remove(token);
    }

    winrt::event_token MpvPlayer::LoopPlaylistChanged(winrt::mpv_winrt::LoopPlaylistChangedEventHandler const& handler)
    {
        return m_loopPlaylistChangedEvent.add(handler);
    }

    void MpvPlayer::LoopPlaylistChanged(winrt::event_token const& token) noexcept
    {
        m_loopPlaylistChangedEvent.remove(token);
    }

    winrt::event_token MpvPlayer::ShuffleChanged(winrt::mpv_winrt::ShuffleChangedEventHandler const& handler)
    {
        return m_shuffleChangedEvent.add(handler);
    }

    void MpvPlayer::ShuffleChanged(winrt::event_token const& token) noexcept
    {
        m_shuffleChangedEvent.remove(token);
    }

    winrt::event_token MpvPlayer::PlaylistChanged(winrt::mpv_winrt::PlaylistChangedEventHandler const& handler)
    {
        return m_playlistChangedEvent.add(handler);
    }

    void MpvPlayer::PlaylistChanged(winrt::event_token const& token) noexcept
    {
        m_playlistChangedEvent.remove(token);
    }

    void MpvPlayer::UpdateSize(uint32_t width, uint32_t height)
    {
        if (!m_mpv)
        {
            return;
        }
        std::string size = std::to_string(width) + "x" + std::to_string(height);
        mpv_set_option_string(m_mpv, "d3d11-composition-size", size.c_str());
    }

    void MpvPlayer::LoadFile(hstring const& url, double position)
    {
        if (!m_mpv)
        {
            return;
        }

        std::string path = winrt::to_string(url);
        int64_t startSeconds = static_cast<int64_t>(position);
        if (startSeconds > 0)
        {
            const std::chrono::hh_mm_ss time{std::chrono::seconds(startSeconds)};
            const std::string formatted = std::format("start={:02}:{:02}:{:02}",
                                                      time.hours().count(),
                                                      time.minutes().count(),
                                                      time.seconds().count());
            const char* args[] = {"loadfile", path.c_str(), "replace", "0", formatted.c_str(), nullptr};
            mpv_command(m_mpv, args);
        }
        else
        {
            const char* args[] = {"loadfile", path.c_str(), nullptr};
            mpv_command(m_mpv, args);
        }
    }

    void MpvPlayer::LoadList(hstring const& url)
    {
        if (!m_mpv)
        {
            return;
        }

        std::string path = winrt::to_string(url);
        const char* args[] = {"loadlist", path.c_str(), nullptr};
        mpv_command(m_mpv, args);
    }

    void MpvPlayer::Play()
    {
        if (!m_mpv)
        {
            return;
        }
        const char* args[] = {"set", "pause", "no", nullptr};
        mpv_command(m_mpv, args);
    }

    void MpvPlayer::Pause()
    {
        if (!m_mpv)
        {
            return;
        }
        const char* args[] = {"set", "pause", "yes", nullptr};
        mpv_command(m_mpv, args);
    }

    void MpvPlayer::Stop()
    {
        if (!m_mpv)
        {
            return;
        }
        const char* args[] = {"stop", nullptr};
        mpv_command(m_mpv, args);
    }

    void MpvPlayer::TogglePlayPause()
    {
        if (!m_mpv)
        {
            return;
        }
        const char* args[] = {"cycle", "pause", nullptr};
        mpv_command(m_mpv, args);
    }

    void MpvPlayer::Command(IVector<hstring> const& args)
    {
        if (!m_mpv)
        {
            return;
        }

        if (!args)
        {
            return;
        }

        const size_t count = args.Size();
        std::vector<std::string> utf8Args;
        std::vector<const char*> cArgs;
        utf8Args.reserve(count);
        cArgs.reserve(count + 1);

        for (auto const& item : args)
        {
            utf8Args.push_back(to_string(item));
            cArgs.push_back(utf8Args.back().c_str());
        }

        cArgs.push_back(nullptr);
        mpv_command(m_mpv, cArgs.data());
    }

    void MpvPlayer::CommandString(hstring const& cmd)
    {
        const auto args = winrt::to_string(cmd);
        mpv_command_string(m_mpv, args.c_str());
    }

    winrt::hstring MpvPlayer::GetWatchHistoryPath()
    {
        if (!m_mpv)
        {
            return L"";
        }

        char* raw = nullptr;
        if (mpv_get_property(m_mpv, "watch-history-path", MPV_FORMAT_STRING, &raw) < 0 || !raw)
        {
            return L"";
        }

        const char* args[] = {"expand-path", raw, nullptr};
        mpv_node result{};
        if (mpv_command_ret(m_mpv, args, &result) < 0 || result.format != MPV_FORMAT_STRING || !result.u.string)
        {
            mpv_free(raw);
            mpv_free_node_contents(&result);
            return L"";
        }

        auto expanded = winrt::to_hstring(result.u.string);
        mpv_free(raw);
        mpv_free_node_contents(&result);
        return expanded;
    }

    winrt::hstring MpvPlayer::GetWatchLaterFolderPath()
    {
        return GetHStringProperty("current-watch-later-dir");
    }

    bool MpvPlayer::SaveWatchHistory()
    {
        return GetFlagProperty("save-watch-history");
    }

    winrt::hstring MpvPlayer::GetCurrentPath()
    {
        return GetHStringProperty("path");
    }

    bool MpvPlayer::IsPaused()
    {
        if (!m_mpv)
        {
            return true;
        }
        return IsStringPropertyEqual("pause", "yes");
    }

    // Volume control methods
    double MpvPlayer::Volume()
    {
        if (!m_mpv)
        {
            return 0.0;
        }
        return GetDoubleProperty("volume");
    }

    void MpvPlayer::Volume(double value)
    {
        if (!m_mpv)
        {
            return;
        }

        if (value < 0)
        {
            value = 0;
        }
        if (value > 100)
        {
            value = 100;
        }
        SetDoubleProperty("volume", value);
    }

    bool MpvPlayer::IsMuted()
    {
        if (!m_mpv)
        {
            return false;
        }
        return IsStringPropertyEqual("mute", "yes");
    }

    void MpvPlayer::IsMuted(bool value)
    {
        if (!m_mpv)
        {
            return;
        }
        SetStringProperty("mute", value ? "yes" : "no");
    }

    double MpvPlayer::Position()
    {
        if (!m_mpv)
        {
            return 0.0;
        }
        return GetDoubleProperty("time-pos");
    }

    void MpvPlayer::Position(double value)
    {
        if (!m_mpv)
        {
            return;
        }
        SetDoubleProperty("time-pos", value);
    }

    double MpvPlayer::Duration()
    {
        if (!m_mpv)
        {
            return 0.0;
        }
        return GetDoubleProperty("duration");
    }

    int32_t MpvPlayer::CurrentVideoTrack()
    {
        if (!m_mpv)
        {
            return -1;
        }
        return static_cast<int32_t>(GetInt64Property("vid"));
    }

    void MpvPlayer::CurrentVideoTrack(int32_t value)
    {
        if (!m_mpv)
        {
            return;
        }

        SetInt64Property("vid", value);
    }

    int32_t MpvPlayer::CurrentAudioTrack()
    {
        if (!m_mpv)
        {
            return -1;
        }

        return static_cast<int32_t>(GetInt64Property("aid"));
    }

    void MpvPlayer::CurrentAudioTrack(int32_t value)
    {
        if (!m_mpv)
        {
            return;
        }

        SetInt64Property("aid", value);
    }

    int32_t MpvPlayer::CurrentSubtitleTrack()
    {
        if (!m_mpv)
        {
            return -1;
        }

        return static_cast<int32_t>(GetInt64Property("sid"));
    }

    void MpvPlayer::CurrentSubtitleTrack(int32_t value)
    {
        if (!m_mpv)
        {
            return;
        }
        if (value <= 0)
        {
            SetStringProperty("sid", "no");
        }
        else
        {
            SetInt64Property("sid", value);
        }
    }

    int32_t MpvPlayer::CurrentSecondSubtitleTrack()
    {
        if (!m_mpv)
        {
            return -1;
        }

        return static_cast<int32_t>(GetInt64Property("secondary-sid"));
    }

    void MpvPlayer::CurrentSecondSubtitleTrack(int32_t value)
    {
        if (!m_mpv)
        {
            return;
        }
        if (value <= 0)
        {
            SetStringProperty("secondary-sid", "no");
        }
        else
        {
            SetInt64Property("secondary-sid", value);
        }
    }

    void MpvPlayer::AddSubtitle(hstring const& url, bool const& selected, hstring const& title)
    {
        if (!m_mpv)
        {
            return;
        }

        auto u8Url = winrt::to_string(url);
        auto u8Title = winrt::to_string(title);
        const char* args[] = {"sub-add", u8Url.c_str(), selected ? "select" : "auto", u8Title.c_str(), nullptr};
        mpv_command(m_mpv, args);
    }

    double MpvPlayer::PlaybackSpeed()
    {
        if (!m_mpv)
        {
            return 1.0;
        }
        return GetDoubleProperty("speed");
    }

    void MpvPlayer::PlaybackSpeed(double value)
    {
        if (!m_mpv)
        {
            return;
        }
        SetDoubleProperty("speed", value);
    }

    bool MpvPlayer::LoopFile()
    {
        if (!m_mpv)
        {
            return false;
        }

        return !IsStringPropertyEqual("loop-file", "no");
    }

    void MpvPlayer::LoopFile(bool enabled)
    {
        if (!m_mpv)
        {
            return;
        }

        SetStringProperty("loop-file", enabled ? "inf" : "no");
    }

    void MpvPlayer::SetLoopPlaylist(bool enabled)
    {
        if (!m_mpv)
        {
            return;
        }

        SetStringProperty("loop-playlist", enabled ? "inf" : "no");
    }

    bool MpvPlayer::LoopPlaylist()
    {
        if (!m_mpv)
        {
            return false;
        }

        return !IsStringPropertyEqual("loop-playlist", "no");
    }

    void MpvPlayer::SetShuffle(bool enabled)
    {
        if (!m_mpv)
        {
            return;
        }
        // TODO
        // const char* args[] = { "playlist-shuffle", nullptr };
        //
        // mpv_command(m_mpv, args);
        SetStringProperty("shuffle", enabled ? "yes" : "no");
    }

    bool MpvPlayer::Shuffle()
    {
        if (!m_mpv)
        {
            return false;
        }

        return !IsStringPropertyEqual("shuffle", "no");
    }

    void MpvPlayer::PlaylistPlayIndex(int32_t index)
    {
        if (!m_mpv)
        {
            return;
        }

        const auto cmd = std::format("playlist-play-index {}", index);
        mpv_command_string(m_mpv, cmd.c_str());
    }

    void MpvPlayer::PlaylistMove(int32_t from, int32_t to)
    {
        if (!m_mpv || from == to || to < 0)
        {
            return;
        }

        const auto cmd = std::format("playlist-move {} {}", from, from < to ? to + 1 : to);
        mpv_command_string(m_mpv, cmd.c_str());
    }

    void MpvPlayer::PlaylistRemove(int32_t index)
    {
        if (!m_mpv)
        {
            return;
        }

        const auto cmd = std::format("playlist-remove {}", index);
        mpv_command_string(m_mpv, cmd.c_str());
    }

    void MpvPlayer::PlaylistNext()
    {
        if (m_mpv)
        {
            mpv_command_string(m_mpv, "playlist-next");
        }
    }

    void MpvPlayer::PlaylistPrevious()
    {
        if (m_mpv)
        {
            mpv_command_string(m_mpv, "playlist-prev");
        }
    }

    void MpvPlayer::PlaylistShuffle()
    {
        if (m_mpv)
        {
            mpv_command_string(m_mpv, "playlist-shuffle");
        }
    }

    void MpvPlayer::SetAspectRatio(hstring const& ratio)
    {
        if (!m_mpv)
        {
            return;
        }

        std::string aspectRatio = to_string(ratio);
        SetStringProperty("video-aspect-override", aspectRatio);
    }

    void MpvPlayer::SetHoverSec(double sec)
    {
        SetDoubleProperty("user-data/osc/hover-sec", sec);
    }

    void MpvPlayer::SetDrawPreview(int32_t x, int32_t y, int32_t w, int32_t h)
    {
        if (!m_mpv)
        {
            return;
        }

        mpv_node node{};
        mpv_node_list list{};
        mpv_node values[4]{};
        const char* keyPtrs[4] = {"x", "y", "w", "h"};

        values[0].format = MPV_FORMAT_INT64;
        values[0].u.int64 = static_cast<int64_t>(x);

        values[1].format = MPV_FORMAT_INT64;
        values[1].u.int64 = static_cast<int64_t>(y);

        values[2].format = MPV_FORMAT_INT64;
        values[2].u.int64 = static_cast<int64_t>(w);

        values[3].format = MPV_FORMAT_INT64;
        values[3].u.int64 = static_cast<int64_t>(h);

        list.num = 4;
        list.keys = (char**)keyPtrs;
        list.values = values;

        node.format = MPV_FORMAT_NODE_MAP;
        node.u.list = &list;

        mpv_set_property(m_mpv, "user-data/osc/draw-preview", MPV_FORMAT_NODE, &node);
    }

    void MpvPlayer::ClearPreview()
    {
        if (!m_mpv)
        {
            return;
        }

        mpv_node null_node{};
        null_node.format = MPV_FORMAT_NONE;
        mpv_set_property(m_mpv, "user-data/osc/draw-preview", MPV_FORMAT_NODE, &null_node);
        mpv_del_property(m_mpv, "user-data/osc/hover-sec");
    }

    void MpvPlayer::AttachSwapChain(SwapChainPanel const& panel)
    {
        IDXGISwapChain* swapChain = nullptr;
        mpv_get_property(m_mpv, "display-swapchain", MPV_FORMAT_INT64, &swapChain);

        if (swapChain)
        {
            winrt::com_ptr<IDXGISwapChain2> swapChain2{nullptr};
            if (S_OK == swapChain->QueryInterface(swapChain2.put()))
            {
                DXGI_MATRIX_3X2_F inverseScale{};
                inverseScale._11 = 1.0f / panel.CompositionScaleX();
                inverseScale._22 = 1.0f / panel.CompositionScaleY();
                swapChain2->SetMatrixTransform(&inverseScale);
            };
        }

        winrt::com_ptr<ISwapChainPanelNative> nativePanel{nullptr};
        if (panel.try_as(nativePanel))
        {
#pragma warning(suppress : 6387)
            nativePanel->SetSwapChain(swapChain);
        }
    }

    void MpvPlayer::UpdateSwapChainScale(float scaleX, float scaleY)
    {
        IDXGISwapChain* swapChain = nullptr;
        mpv_get_property(m_mpv, "display-swapchain", MPV_FORMAT_INT64, &swapChain);

        if (swapChain && scaleX > 0 && scaleY > 0)
        {
            winrt::com_ptr<IDXGISwapChain2> swapChain2{nullptr};
            if (S_OK == swapChain->QueryInterface(swapChain2.put()))
            {
                DXGI_MATRIX_3X2_F inverseScale{};
                inverseScale._11 = 1.0f / scaleX;
                inverseScale._22 = 1.0f / scaleY;
                swapChain2->SetMatrixTransform(&inverseScale);
            }
        }
    }

    double MpvPlayer::GetDoubleProperty(const char* name)
    {
        if (!m_mpv)
        {
            return 0.0;
        }

        double value = 0.0;
        mpv_get_property(m_mpv, name, MPV_FORMAT_DOUBLE, &value);
        return value;
    }

    int64_t MpvPlayer::GetInt64Property(const char* name)
    {
        if (!m_mpv)
        {
            return 0;
        }

        int64_t value = 0;
        mpv_get_property(m_mpv, name, MPV_FORMAT_INT64, &value);
        return value;
    }

    winrt::hstring MpvPlayer::GetHStringProperty(const char* name)
    {
        if (!m_mpv)
        {
            return L"";
        }

        char* value = nullptr;
        if (mpv_get_property(m_mpv, name, MPV_FORMAT_STRING, &value) >= 0 && value)
        {
            auto hstring = winrt::to_hstring(value);
            mpv_free(value);
            return hstring;
        }
        return L"";
    }

    bool MpvPlayer::GetFlagProperty(const char* name)
    {
        if (!m_mpv)
        {
            return false;
        }

        int flag = 0;
        return mpv_get_property(m_mpv, name, MPV_FORMAT_FLAG, &flag) >= 0 && flag != 0;
    }

    bool MpvPlayer::IsStringPropertyEqual(const char* name, std::string_view expected)
    {
        if (!m_mpv)
        {
            return false;
        }

        char* value = nullptr;
        if (mpv_get_property(m_mpv, name, MPV_FORMAT_STRING, &value) < 0 || !value)
        {
            return false;
        }

        const bool isEqual = std::string_view(value) == expected;
        mpv_free(value);
        return isEqual;
    }

    void MpvPlayer::SetDoubleProperty(const char* name, double value)
    {
        if (!m_mpv)
        {
            return;
        }

        mpv_set_property(m_mpv, name, MPV_FORMAT_DOUBLE, &value);
    }

    void MpvPlayer::SetInt64Property(const char* name, int64_t value)
    {
        if (!m_mpv)
        {
            return;
        }
        mpv_set_property(m_mpv, name, MPV_FORMAT_INT64, &value);
    }

    void MpvPlayer::SetStringProperty(const char* name, const std::string& value)
    {
        if (!m_mpv)
        {
            return;
        }
        const char* str = value.c_str();
        mpv_set_property(m_mpv, name, MPV_FORMAT_STRING, &str);
    }

    void MpvPlayer::SetOption(std::string const& name, std::string const& value)
    {
        mpv_set_option_string(m_mpv, name.c_str(), value.c_str());
    }

    winrt::Windows::Foundation::Collections::IVectorView<winrt::mpv_winrt::MpvPlaylistItem> MpvPlayer::GetPlaylist()
    {
        auto items = winrt::single_threaded_vector<winrt::mpv_winrt::MpvPlaylistItem>();
        if (!m_mpv)
        {
            return items.GetView();
        }

        mpv_node node;
        if (mpv_get_property(m_mpv, "playlist", MPV_FORMAT_NODE, &node) < 0)
        {
            return items.GetView();
        }

        if (node.format == MPV_FORMAT_NODE_ARRAY)
        {
            for (int i = 0; i < node.u.list->num; i++)
            {
                mpv_node* entry = &node.u.list->values[i];
                if (entry->format != MPV_FORMAT_NODE_MAP)
                {
                    continue;
                }

                int32_t id = -1;
                std::string filename, title;
                bool isCurrent = false, isPlaying = false;

                for (int j = 0; j < entry->u.list->num; j++)
                {
                    auto& key = entry->u.list->keys[j];
                    auto& value = entry->u.list->values[j];

                    if (strcmp(key, "id") == 0 && value.format == MPV_FORMAT_INT64)
                    {
                        id = static_cast<int32_t>(value.u.int64);
                    }
                    else if (strcmp(key, "filename") == 0 && value.format == MPV_FORMAT_STRING)
                    {
                        filename = value.u.string ? value.u.string : "";
                    }
                    else if (strcmp(key, "title") == 0 && value.format == MPV_FORMAT_STRING)
                    {
                        title = value.u.string ? value.u.string : "";
                    }
                    else if (strcmp(key, "current") == 0 && value.format == MPV_FORMAT_FLAG)
                    {
                        isCurrent = value.u.flag != 0;
                    }
                    else if (strcmp(key, "playing") == 0 && value.format == MPV_FORMAT_FLAG)
                    {
                        isPlaying = value.u.flag != 0;
                    }
                }

                auto item = winrt::make<implementation::MpvPlaylistItem>(id, i, winrt::to_hstring(filename), winrt::to_hstring(title), isCurrent, isPlaying);
                items.Append(item);
            }
        }

        mpv_free_node_contents(&node);
        return items.GetView();
    }

    mpv_node* MpvPlayer::FindMapField(mpv_node* map, const char* key)
    {
        if (!map || map->format != MPV_FORMAT_NODE_MAP)
        {
            return nullptr;
        }
        for (int i = 0; i < map->u.list->num; i++)
        {
            if (strcmp(map->u.list->keys[i], key) == 0)
            {
                return &map->u.list->values[i];
            }
        }
        return nullptr;
    }

    winrt::Windows::Foundation::Collections::IVectorView<winrt::mpv_winrt::MpvTrack> MpvPlayer::GetTracks(const char* type)
    {
        auto tracks = winrt::single_threaded_vector<winrt::mpv_winrt::MpvTrack>();
        if (!m_mpv)
        {
            return tracks.GetView();
        }

        mpv_node node;
        if (mpv_get_property(m_mpv, "track-list", MPV_FORMAT_NODE, &node) < 0)
        {
            return tracks.GetView();
        }

        if (node.format == MPV_FORMAT_NODE_ARRAY)
        {
            int32_t index = 0;
            for (int i = 0; i < node.u.list->num; i++)
            {
                auto* entry = &node.u.list->values[i];
                if (entry->format != MPV_FORMAT_NODE_MAP)
                {
                    continue;
                }

                auto* typeField = FindMapField(entry, "type");
                if (!typeField || typeField->format != MPV_FORMAT_STRING || !typeField->u.string)
                {
                    continue;
                }
                if (strcmp(typeField->u.string, type) != 0)
                {
                    continue;
                }

                index++;
                int32_t id = -1;
                std::string title, lang, codec;
                bool selected = false, isDefault = false;

                auto* idField = FindMapField(entry, "id");
                if (idField && idField->format == MPV_FORMAT_INT64)
                {
                    id = static_cast<int32_t>(idField->u.int64);
                }

                auto* titleField = FindMapField(entry, "title");
                if (titleField && titleField->format == MPV_FORMAT_STRING && titleField->u.string)
                {
                    title = titleField->u.string;
                }

                auto* langField = FindMapField(entry, "lang");
                if (langField && langField->format == MPV_FORMAT_STRING && langField->u.string)
                {
                    lang = langField->u.string;
                }

                auto* codecField = FindMapField(entry, "codec");
                if (codecField && codecField->format == MPV_FORMAT_STRING && codecField->u.string)
                {
                    codec = codecField->u.string;
                }

                auto* selectedField = FindMapField(entry, "selected");
                if (selectedField && selectedField->format == MPV_FORMAT_FLAG)
                {
                    selected = selectedField->u.flag != 0;
                }

                auto* defaultField = FindMapField(entry, "default");
                if (defaultField && defaultField->format == MPV_FORMAT_FLAG)
                {
                    isDefault = defaultField->u.flag != 0;
                }

                auto hTitle = winrt::to_hstring(title);
                auto hLang = winrt::to_hstring(lang);
                auto hCodec = winrt::to_hstring(codec);

                if (strcmp(type, "audio") == 0)
                {
                    int32_t demuxChannelCount = 0, demuxSamplerate = 0;
                    auto* ccField = FindMapField(entry, "demux-channel-count");
                    if (ccField && ccField->format == MPV_FORMAT_INT64)
                    {
                        demuxChannelCount = static_cast<int32_t>(ccField->u.int64);
                    }
                    auto* srField = FindMapField(entry, "demux-samplerate");
                    if (srField && srField->format == MPV_FORMAT_INT64)
                    {
                        demuxSamplerate = static_cast<int32_t>(srField->u.int64);
                    }

                    tracks.Append(winrt::make<implementation::MpvTrack>(
                        index, id, winrt::mpv_winrt::TrackType::Audio,
                        hTitle, hLang, selected, hCodec, isDefault,
                        demuxChannelCount, demuxSamplerate));
                }
                else if (strcmp(type, "video") == 0)
                {
                    int32_t demuxW = 0, demuxH = 0;
                    double demuxFps = 0;
                    auto* wField = FindMapField(entry, "demux-w");
                    if (wField && wField->format == MPV_FORMAT_INT64)
                    {
                        demuxW = static_cast<int32_t>(wField->u.int64);
                    }
                    auto* hField = FindMapField(entry, "demux-h");
                    if (hField && hField->format == MPV_FORMAT_INT64)
                    {
                        demuxH = static_cast<int32_t>(hField->u.int64);
                    }
                    auto* fpsField = FindMapField(entry, "demux-fps");
                    if (fpsField && fpsField->format == MPV_FORMAT_DOUBLE)
                    {
                        demuxFps = fpsField->u.double_;
                    }

                    tracks.Append(winrt::make<implementation::MpvTrack>(
                        index, id, winrt::mpv_winrt::TrackType::Video,
                        hTitle, hLang, selected, hCodec, isDefault,
                        demuxW, demuxH, demuxFps));
                }
                else if (strcmp(type, "sub") == 0)
                {
                    bool isForced = false, isExternal = false;
                    auto* forcedField = FindMapField(entry, "forced");
                    if (forcedField && forcedField->format == MPV_FORMAT_FLAG)
                    {
                        isForced = forcedField->u.flag != 0;
                    }
                    auto* extField = FindMapField(entry, "external");
                    if (extField && extField->format == MPV_FORMAT_FLAG)
                    {
                        isExternal = extField->u.flag != 0;
                    }

                    tracks.Append(winrt::make<implementation::MpvTrack>(
                        index, id, winrt::mpv_winrt::TrackType::Subtitle,
                        hTitle, hLang, selected, hCodec, isDefault,
                        isForced, isExternal));
                }
            }
        }

        mpv_free_node_contents(&node);
        return tracks.GetView();
    }

    winrt::Windows::Foundation::Collections::IVectorView<winrt::mpv_winrt::MpvTrack> MpvPlayer::GetAudioTracks()
    {
        return GetTracks("audio");
    }

    winrt::Windows::Foundation::Collections::IVectorView<winrt::mpv_winrt::MpvTrack> MpvPlayer::GetVideoTracks()
    {
        return GetTracks("video");
    }

    winrt::Windows::Foundation::Collections::IVectorView<winrt::mpv_winrt::MpvTrack> MpvPlayer::GetSubtitleTracks()
    {
        return GetTracks("sub");
    }

    winrt::Windows::Foundation::Collections::IVectorView<winrt::mpv_winrt::MpvChapter> MpvPlayer::GetChapters()
    {
        auto chapters = winrt::single_threaded_vector<winrt::mpv_winrt::MpvChapter>();
        if (!m_mpv)
        {
            return chapters.GetView();
        }

        mpv_node node;
        if (mpv_get_property(m_mpv, "chapter-list", MPV_FORMAT_NODE, &node) < 0)
        {
            return chapters.GetView();
        }

        if (node.format == MPV_FORMAT_NODE_ARRAY)
        {
            for (int i = 0; i < node.u.list->num; i++)
            {
                mpv_node* entry = &node.u.list->values[i];
                if (entry->format != MPV_FORMAT_NODE_MAP)
                {
                    continue;
                }

                int32_t id = i;
                std::string title;
                double time = 0;

                for (int j = 0; j < entry->u.list->num; j++)
                {
                    auto& key = entry->u.list->keys[j];
                    auto& value = entry->u.list->values[j];

                    if (strcmp(key, "title") == 0 && value.format == MPV_FORMAT_STRING)
                    {
                        title = value.u.string ? value.u.string : "";
                    }
                    else if (strcmp(key, "time") == 0 && value.format == MPV_FORMAT_DOUBLE)
                    {
                        time = value.u.double_;
                    }
                }

                auto chapter = winrt::make<implementation::MpvChapter>(id, winrt::to_hstring(title), time);
                chapters.Append(chapter);
            }
        }

        mpv_free_node_contents(&node);
        return chapters.GetView();
    }

    winrt::Windows::Foundation::Collections::IVectorView<winrt::mpv_winrt::MpvEdition> MpvPlayer::GetEditions()
    {
        auto editions = winrt::single_threaded_vector<winrt::mpv_winrt::MpvEdition>();
        if (!m_mpv)
        {
            return editions.GetView();
        }

        mpv_node node;
        if (mpv_get_property(m_mpv, "edition-list", MPV_FORMAT_NODE, &node) < 0)
        {
            return editions.GetView();
        }

        if (node.format == MPV_FORMAT_NODE_ARRAY)
        {
            for (int i = 0; i < node.u.list->num; i++)
            {
                mpv_node* entry = &node.u.list->values[i];
                if (entry->format != MPV_FORMAT_NODE_MAP)
                {
                    continue;
                }

                int32_t id = i;
                std::string title;

                for (int j = 0; j < entry->u.list->num; j++)
                {
                    auto& key = entry->u.list->keys[j];
                    auto& value = entry->u.list->values[j];

                    if (strcmp(key, "title") == 0 && value.format == MPV_FORMAT_STRING)
                    {
                        title = value.u.string ? value.u.string : "";
                    }
                }

                auto edition = winrt::make<implementation::MpvEdition>(id, winrt::to_hstring(title));
                editions.Append(edition);
            }
        }

        mpv_free_node_contents(&node);
        return editions.GetView();
    }

    winrt::Windows::Foundation::Collections::IVectorView<winrt::mpv_winrt::MpvAudioDevice> MpvPlayer::GetAudioDevices()
    {
        auto devices = winrt::single_threaded_vector<winrt::mpv_winrt::MpvAudioDevice>();
        if (!m_mpv)
        {
            return devices.GetView();
        }

        mpv_node node;
        if (mpv_get_property(m_mpv, "audio-device-list", MPV_FORMAT_NODE, &node) < 0)
        {
            return devices.GetView();
        }

        if (node.format == MPV_FORMAT_NODE_ARRAY)
        {
            for (int i = 0; i < node.u.list->num; i++)
            {
                mpv_node* entry = &node.u.list->values[i];
                if (entry->format != MPV_FORMAT_NODE_MAP)
                {
                    continue;
                }

                std::string name, description;

                for (int j = 0; j < entry->u.list->num; j++)
                {
                    auto& key = entry->u.list->keys[j];
                    auto& value = entry->u.list->values[j];

                    if (strcmp(key, "name") == 0 && value.format == MPV_FORMAT_STRING)
                    {
                        name = value.u.string ? value.u.string : "";
                    }
                    else if (strcmp(key, "description") == 0 && value.format == MPV_FORMAT_STRING)
                    {
                        description = value.u.string ? value.u.string : "";
                    }
                }

                auto device = winrt::make<implementation::MpvAudioDevice>(
                    winrt::to_hstring(name), winrt::to_hstring(description));
                devices.Append(device);
            }
        }

        mpv_free_node_contents(&node);
        return devices.GetView();
    }

    winrt::Windows::Foundation::Collections::IVectorView<winrt::mpv_winrt::MpvProfile> MpvPlayer::GetProfiles()
    {
        auto profiles = winrt::single_threaded_vector<winrt::mpv_winrt::MpvProfile>();
        if (!m_mpv)
        {
            return profiles.GetView();
        }

        mpv_node node;
        if (mpv_get_property(m_mpv, "profile-list", MPV_FORMAT_NODE, &node) < 0)
        {
            return profiles.GetView();
        }

        if (node.format == MPV_FORMAT_NODE_ARRAY)
        {
            for (int i = 0; i < node.u.list->num; i++)
            {
                mpv_node* entry = &node.u.list->values[i];
                if (entry->format != MPV_FORMAT_NODE_MAP)
                {
                    continue;
                }

                std::string name;

                for (int j = 0; j < entry->u.list->num; j++)
                {
                    auto& key = entry->u.list->keys[j];
                    auto& value = entry->u.list->values[j];

                    if (strcmp(key, "name") == 0 && value.format == MPV_FORMAT_STRING)
                    {
                        name = value.u.string ? value.u.string : "";
                    }
                }

                auto profile = winrt::make<implementation::MpvProfile>(winrt::to_hstring(name));
                profiles.Append(profile);
            }
        }

        mpv_free_node_contents(&node);
        return profiles.GetView();
    }

    int32_t MpvPlayer::CurrentChapter()
    {
        return static_cast<int32_t>(GetInt64Property("chapter"));
    }

    int32_t MpvPlayer::CurrentEdition()
    {
        return static_cast<int32_t>(GetInt64Property("edition"));
    }

    static winrt::Windows::Foundation::Collections::IVectorView<winrt::mpv_winrt::MpvMenuItem> ParseMenuNode(mpv_node* node)
    {
        auto items = winrt::single_threaded_vector<winrt::mpv_winrt::MpvMenuItem>();
        if (!node || node->format != MPV_FORMAT_NODE_ARRAY)
        {
            return items.GetView();
        }

        auto* list = node->u.list;
        for (int i = 0; i < list->num; i++)
        {
            auto* itemNode = &list->values[i];
            if (itemNode->format != MPV_FORMAT_NODE_MAP)
            {
                continue;
            }

            winrt::hstring title{L""};
            winrt::hstring command{L""};
            winrt::hstring type{L"command"};
            bool isChecked = false;
            bool isDisabled = false;
            bool isHidden = false;
            mpv_node* subItemsNode = nullptr;

            auto* itemList = itemNode->u.list;
            for (int j = 0; j < itemList->num; j++)
            {
                std::string key = itemList->keys[j];
                mpv_node* val = &itemList->values[j];

                if (key == "title" && val->format == MPV_FORMAT_STRING)
                {
                    title = winrt::to_hstring(val->u.string);
                }
                else if (key == "cmd" && val->format == MPV_FORMAT_STRING)
                {
                    command = winrt::to_hstring(val->u.string);
                }
                else if (key == "type" && val->format == MPV_FORMAT_STRING)
                {
                    type = winrt::to_hstring(val->u.string);
                }
                else if (key == "state" && val->format == MPV_FORMAT_NODE_ARRAY)
                {
                    auto* stateList = val->u.list;
                    for (int k = 0; k < stateList->num; k++)
                    {
                        if (stateList->values[k].format == MPV_FORMAT_STRING)
                        {
                            std::string s = stateList->values[k].u.string;
                            if (s == "checked")
                            {
                                isChecked = true;
                            }
                            if (s == "disabled")
                            {
                                isDisabled = true;
                            }
                            if (s == "hidden")
                            {
                                isHidden = true;
                            }
                        }
                    }
                }
                else if (key == "submenu")
                {
                    subItemsNode = val;
                }
            }

            auto subItems = subItemsNode
                ? ParseMenuNode(subItemsNode)
                : winrt::single_threaded_vector<winrt::mpv_winrt::MpvMenuItem>().GetView();

            items.Append(winrt::make<implementation::MpvMenuItem>(
                title, command, type, isChecked, isDisabled, isHidden, subItems));
        }

        return items.GetView();
    }

    winrt::Windows::Foundation::Collections::IVectorView<winrt::hstring> MpvPlayer::GetSubtitleExtensions()
    {
        auto extensions = winrt::single_threaded_vector<winrt::hstring>();
        if (!m_mpv)
        {
            return extensions.GetView();
        }

        mpv_node node;
        if (mpv_get_property(m_mpv, "sub-auto-exts", MPV_FORMAT_NODE, &node) < 0)
        {
            return extensions.GetView();
        }

        if (node.format == MPV_FORMAT_NODE_ARRAY)
        {
            for (int i = 0; i < node.u.list->num; i++)
            {
                mpv_node& value = node.u.list->values[i];
                if (value.format != MPV_FORMAT_STRING || !value.u.string)
                {
                    continue;
                }

                const char* ext = value.u.string;
                if (ext[0] == '.')
                {
                    ext++;
                }
                extensions.Append(winrt::to_hstring(ext));
            }
        }

        mpv_free_node_contents(&node);
        return extensions.GetView();
    }

    winrt::hstring MpvPlayer::GetVersion()
    {
        return GetHStringProperty("mpv-version");
    }

    winrt::Windows::Foundation::Collections::IVectorView<winrt::mpv_winrt::MpvMenuItem> MpvPlayer::GetMenu()
    {
        if (!m_mpv)
        {
            return winrt::single_threaded_vector<winrt::mpv_winrt::MpvMenuItem>().GetView();
        }

        mpv_node node;
        if (mpv_get_property(m_mpv, "menu-data", MPV_FORMAT_NODE, &node) < 0)
        {
            return winrt::single_threaded_vector<winrt::mpv_winrt::MpvMenuItem>().GetView();
        }

        auto result = ParseMenuNode(&node);
        mpv_free_node_contents(&node);
        return result;
    }

    void MpvPlayer::UpdateDisplayColorInfo(winrt::mpv_winrt::DisplayColorKind colorKind)
    {
        if (!m_mpv)
        {
            return;
        }

        const char* cs;
        switch (colorKind)
        {
            case winrt::mpv_winrt::DisplayColorKind::HDR:
                cs = "HDR";
                break;
            case winrt::mpv_winrt::DisplayColorKind::WCG:
                cs = "WCG";
                break;
            default:
                cs = "SDR";
                break;
        }
        SetStringProperty("user-data/mpvw/color-kind", cs);
    }

    void MpvPlayer::UpdateDisplayRefreshRate(int32_t refreshRate)
    {
        if (!m_mpv)
        {
            return;
        }

        SetOption("override-display-fps", std::to_string(refreshRate));
        SetInt64Property("user-data/mpvw/refresh-rate", refreshRate);
    }
} // namespace winrt::mpv_winrt::implementation
