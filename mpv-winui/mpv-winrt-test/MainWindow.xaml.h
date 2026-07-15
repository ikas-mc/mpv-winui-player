#pragma once

#include "MainWindow.g.h"

namespace winrt::mpv_winrt_test::implementation
{
	struct MainWindow: MainWindowT<MainWindow>
	{
		MainWindow();
	};
}

namespace winrt::mpv_winrt_test::factory_implementation
{
	struct MainWindow: MainWindowT<MainWindow, implementation::MainWindow>
	{
	};
}
