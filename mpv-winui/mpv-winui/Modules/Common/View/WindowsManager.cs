using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Graphics;

namespace mpv_winui.Modules.Common.View
{
    public sealed class WindowsManager
    {
        private readonly Dictionary<string, WeakReference<Window>> _children = [];

        public Window Open(string key, Func<Window> creator, Window? parent, double parentSizePercent, double minWidth, double minHeight)
        {
            if (!_children.TryGetValue(key, out var weak) || !weak.TryGetTarget(out var window))
            {
                window = creator();

                var windowRef = new WeakReference<Window>(window);
                _children[key] = windowRef;
                window.Closed += (_, __) =>
                {
                    if (_children.TryGetValue(key, out var value) && ReferenceEquals(value, windowRef))
                    {
                        _children.Remove(key);
                    }
                };
            }

            window.Activate();

            window.SetWindowMinSize(minWidth, minHeight);
            UpdatePositionAndSize(window, parent, parentSizePercent, minWidth, minHeight);

            window.ShowWindow();
            return window;
        }

        public Window Open(string key, Func<Window> creator, double minWidth, double minHeight)
        {
            return Open(key, creator, null, 0, minWidth, minHeight);
        }

        private void UpdatePositionAndSize(Window child, Window? parent, double parentSizePercent, double minWidth, double minHeight)
        {
            if (parent is null || parentSizePercent <= 0)
            {
                return;
            }

            var position = parent.AppWindow.Position;
            var size = parent.AppWindow.Size;

            double width = Math.Max(size.Width * parentSizePercent, minWidth);
            double height = Math.Max(size.Height * parentSizePercent, minHeight);

            double x = position.X + ((size.Width - width) * 0.5);
            double y = position.Y + ((size.Height - height) * 0.5);

            child.AppWindow.MoveAndResize(new RectInt32((int)x, (int)y, (int)width, (int)height));
        }

        public void Close()
        {
            foreach (var weak in _children.Values.ToArray())
            {
                if (weak.TryGetTarget(out var window))
                {
                    window.Close();
                }
            }
            _children.Clear();
        }

        public void UpdateTheme()
        {
            foreach (var weak in _children.Values)
            {
                if (weak.TryGetTarget(out var window) && window is IWindowStyleRefreshSupport s)
                {
                    s.UpdateTheme();
                }
            }
        }

        public void UpdateBackdrop()
        {
            foreach (var weak in _children.Values)
            {
                if (weak.TryGetTarget(out var window) && window is IWindowStyleRefreshSupport s)
                {
                    s.UpdateBackdrop();
                }
            }
        }
    }
}
