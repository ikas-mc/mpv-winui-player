#include "pch.h"
#include "MpvPreviewer.h"
#include "MpvPreviewer.g.cpp"
#include <winrt/Microsoft.UI.Dispatching.h>
#include <wil/cppwinrt_helpers.h>

namespace
{
    struct __declspec(uuid("905a0fef-bc53-11df-8c49-001e4fc686da")) IBufferByteAccess: ::IUnknown
    {
        virtual HRESULT __stdcall Buffer(BYTE** value) = 0;
    };
}

namespace winrt::mpv_winrt::implementation
{
    MpvPreviewer::MpvPreviewer()
    {
    }

    MpvPreviewer::~MpvPreviewer()
    {
        Destroy();
    }

    void MpvPreviewer::CreateContext()
    {
        m_mpv = mpv_create();
        if (!m_mpv)
        {
            throw hresult_error(E_FAIL, L"Failed to create mpv context");
        }
    }

    void MpvPreviewer::SetOption(std::string const& name, std::string const& value)
    {
        if (mpv_set_option_string(m_mpv, name.c_str(), value.c_str()) < 0)
        {
            throw hresult_error(E_FAIL, L"Failed to set mpv option");
        }
    }

    winrt::Windows::Foundation::IAsyncAction MpvPreviewer::Initialize(winrt::Microsoft::UI::Xaml::Controls::Image const& image, uint32_t width, uint32_t height)
    {
        m_image = image;
        m_dispatcher = m_image.DispatcherQueue();
        m_width = width;
        m_height = height;
        m_stride = static_cast<size_t>(width) * 4;
        m_size = m_stride * height;

        co_await winrt::resume_background();

        {
            std::lock_guard lifecycleLock(m_lifecycleMutex);
            if (m_destroyed)
            {
                co_return;
            }

            try
            {
                CreateContext();

                SetOption("vo", "libmpv");
                SetOption("config", "no");
                SetOption("msg-level", "all=no");
                SetOption("profile", "fast");
                SetOption("osc", "no");
                SetOption("load-scripts", "no");
                SetOption("idle", "yes");
                SetOption("keep-open", "yes");
                SetOption("pause", "yes");
                SetOption("sub", "no");
                SetOption("hr-seek", "no");
                SetOption("audio", "no");
                SetOption("terminal", "no");
                SetOption("cache", "no");
                SetOption("hwdec", "auto");

                if (mpv_initialize(m_mpv) < 0)
                {
                    throw hresult_error(E_FAIL, L"Failed to initialize mpv for preview");
                }

                CreateRenderContext();
            }
            catch (...)
            {
                if (m_renderContext)
                {
                    mpv_render_context_free(m_renderContext);
                    m_renderContext = nullptr;
                }
                if (m_mpv)
                {
                    mpv_terminate_destroy(m_mpv);
                    m_mpv = nullptr;
                }
                throw;
            }
        }

        co_await wil::resume_foreground(m_dispatcher);

        {
            std::lock_guard lifecycleLock(m_lifecycleMutex);
            if (m_destroyed)
            {
                co_return;
            }

            m_bitmap = winrt::Microsoft::UI::Xaml::Media::Imaging::WriteableBitmap(width, height);
            m_image.Source(m_bitmap);

            auto pixelBuffer = m_bitmap.PixelBuffer();
            auto byteAccess = pixelBuffer.as<IBufferByteAccess>();
            byteAccess->Buffer(&m_bitmapData);

            {
                std::lock_guard lock(m_renderMutex);
                m_quit = false;
            }
            m_renderThread = std::thread([this]() { RenderLoop(); });
            m_initialized = true;
        }
    }

    void MpvPreviewer::CreateRenderContext()
    {
        const char* api = "sw";
        mpv_render_param params[] = {
            {MPV_RENDER_PARAM_API_TYPE, const_cast<char*>(api)},
            {MPV_RENDER_PARAM_INVALID, nullptr},
        };

        if (mpv_render_context_create(&m_renderContext, m_mpv, params) < 0)
        {
            throw hresult_error(E_FAIL, L"Failed to create software render context");
        }

        mpv_render_context_set_update_callback(m_renderContext, &MpvPreviewer::SwRenderUpdateCallback, this);
    }

    void MpvPreviewer::SwRenderUpdateCallback(void* cb_ctx)
    {
        static_cast<MpvPreviewer*>(cb_ctx)->NotifyFrameReady();
    }

    void MpvPreviewer::NotifyFrameReady()
    {
        std::lock_guard lock(m_renderMutex);
        m_framePending = true;
        m_renderCv.notify_one();
    }

    void MpvPreviewer::RenderLoop()
    {
        while (true)
        {
            std::unique_lock lock(m_renderMutex);
            m_renderCv.wait(lock, [this]() { return m_framePending || m_quit; });
            m_framePending = false;
            if (m_quit)
            {
                break;
            }
            lock.unlock();

            RenderFrame();
        }
    }

    void MpvPreviewer::RenderFrame()
    {
        if (!m_renderContext || !m_bitmapData)
        {
            return;
        }

        int swSize[2] = {static_cast<int>(m_width), static_cast<int>(m_height)};
        const char* format = "bgr0";

        mpv_render_param params[] = {
            {MPV_RENDER_PARAM_SW_SIZE, swSize},
            {MPV_RENDER_PARAM_SW_FORMAT, const_cast<char*>(format)},
            {MPV_RENDER_PARAM_SW_STRIDE, &m_stride},
            {MPV_RENDER_PARAM_SW_POINTER, m_bitmapData},
            {MPV_RENDER_PARAM_INVALID, nullptr},
        };

        mpv_render_context_update(m_renderContext);
        mpv_render_context_render(m_renderContext, params);

        for (size_t i = 3; i < m_size; i += 4)
        {
            m_bitmapData[i] = 0xFF;
        }

        auto weak_this{get_weak()};
        if (m_dispatcher)
        {
            m_dispatcher.TryEnqueue([weak_this]() {
                if (auto strong_this{weak_this.get()})
                {
                    if (strong_this->m_bitmap)
                    {
                        strong_this->m_bitmap.Invalidate();
                    }
                }
            });
        }
    }

    void MpvPreviewer::LoadFile(winrt::hstring const& url)
    {
        if (!m_mpv)
        {
            return;
        }

        std::string urlStr = winrt::to_string(url);
        const char* cmd[] = {"loadfile", urlStr.c_str(), "replace", nullptr};
        mpv_command(m_mpv, cmd);
    }

    void MpvPreviewer::SetPosition(double position)
    {
        if (!m_mpv)
        {
            return;
        }
        mpv_set_property(m_mpv, "time-pos", MPV_FORMAT_DOUBLE, &position);
    }

    void MpvPreviewer::Pause()
    {
        if (!m_mpv)
        {
            return;
        }
        int paused = 1;
        mpv_set_property(m_mpv, "pause", MPV_FORMAT_FLAG, &paused);
    }

    void MpvPreviewer::Destroy()
    {
        std::lock_guard lifecycleLock(m_lifecycleMutex);
        m_destroyed = true;
        if (m_initialized)
        {
            m_initialized = false;
            {
                std::lock_guard lock(m_renderMutex);
                m_quit = true;
            }
            m_renderCv.notify_all();
            if (m_renderThread.joinable())
            {
                m_renderThread.join();
            }
        }

        if (m_renderContext)
        {
            mpv_render_context_set_update_callback(m_renderContext, nullptr, nullptr);
            mpv_render_context_free(m_renderContext);
            m_renderContext = nullptr;
        }

        if (m_mpv)
        {
            mpv_terminate_destroy(m_mpv);
            m_mpv = nullptr;
        }

        m_bitmapData = nullptr;
        m_bitmap = nullptr;
        m_dispatcher = nullptr;
    }
}