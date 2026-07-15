#pragma once
#include <wil/cppwinrt.h>
// Undefine GetCurrentTime macro to prevent
// conflict with Storyboard::GetCurrentTime
#undef GetCurrentTime

#include <algorithm>
#include <memory>
#include <string>
#include <string_view>
#include <mutex>
#include <thread>
#include <atomic>
#include <iostream>
#include <sstream>
#include <format>

#include <winrt/Windows.Storage.h>
#include <winrt/Windows.Foundation.h>
#include <winrt/Windows.Foundation.Collections.h>