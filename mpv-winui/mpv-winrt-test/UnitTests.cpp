#include "pch.h"

#include <winrt/mpv_winrt.h>
#include "CppUnitTest.h"
using namespace Microsoft::VisualStudio::CppUnitTestFramework;

namespace mpv_winrt_test
{
	TEST_CLASS(CppUnitTests)
	{
	public:

		TEST_METHOD_INITIALIZE(InitializeWinRT)
		{
		}

		TEST_METHOD_CLEANUP(UninitializeWinRT)
		{
		}

		TEST_METHOD(TestInitialize)
		{
			winrt::mpv_winrt::MpvPlayer palyer{};
			int volume = 30;
			palyer.Initialize(L"", 1, 1, volume);
			Assert::IsTrue(palyer.Volume() == volume);
		}
	};
}
