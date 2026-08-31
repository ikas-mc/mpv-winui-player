#pragma once
#include "MpvPreviewer.g.h"
#include <condition_variable>
#include <mpv/client.h>
#include <mpv/render.h>
#include <mutex>
#include <string>
#include <thread>
#include <winrt/Microsoft.UI.Dispatching.h>
#include <winrt/Microsoft.UI.Xaml.Controls.h>
#include <winrt/Microsoft.UI.Xaml.Media.Imaging.h>
#include <winrt/Windows.Storage.Streams.h>

namespace winrt::mpv_winrt::implementation
{
    struct MpvPreviewer: MpvPreviewerT<MpvPreviewer>
    {
        MpvPreviewer();
        ~MpvPreviewer();

        winrt::Windows::Foundation::IAsyncAction Initialize(winrt::Microsoft::UI::Xaml::Controls::Image const& image, uint32_t width, uint32_t height);
        void Destroy();
        void LoadFile(winrt::hstring const& url);
        void SetPosition(double position);
        void Pause();
        void SetEdition(int32_t edition);
        void SetDiscPath(winrt::mpv_winrt::DiscType type, winrt::hstring const& path);
        void SetVideoTrack(int32_t value);

    private:
        void CreateContext();
        void SetOption(std::string const& name, std::string const& value);
        void CreateRenderContext();
        void RenderLoop();
        void RenderFrame();
        static void SwRenderUpdateCallback(void* cb_ctx);
        void NotifyFrameReady();

        mpv_handle* m_mpv{nullptr};
        mpv_render_context* m_renderContext{nullptr};

        winrt::Microsoft::UI::Xaml::Media::Imaging::WriteableBitmap m_bitmap{nullptr};
        winrt::Microsoft::UI::Xaml::Controls::Image m_image{nullptr};
        winrt::Microsoft::UI::Dispatching::DispatcherQueue m_dispatcher{nullptr};

        uint8_t* m_bitmapData{nullptr};
        uint32_t m_width{0};
        uint32_t m_height{0};
        size_t m_stride{0};
        size_t m_size{0};

        std::thread m_renderThread;
        std::mutex m_renderMutex;
        std::mutex m_lifecycleMutex;
        std::condition_variable m_renderCv;
        bool m_framePending{false};
        bool m_quit{false};
        bool m_initialized{false};
        bool m_destroyed{false};
    };
}

namespace winrt::mpv_winrt::factory_implementation
{
    struct MpvPreviewer: MpvPreviewerT<MpvPreviewer, implementation::MpvPreviewer>
    {
    };
}