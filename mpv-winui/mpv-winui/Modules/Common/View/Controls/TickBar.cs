using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using System;
using System.Collections.Generic;
using Windows.Foundation;
using Windows.UI;

namespace mpv_winui.Modules.Common.View.Controls
{
    public partial class TickBar : Panel
    {
        public static readonly DependencyProperty ValuesProperty = DependencyProperty.Register(
            nameof(Values),
            typeof(IReadOnlyList<double>),
            typeof(TickBar),
            new PropertyMetadata(null, OnTicksDataChanged));

        public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register(
            nameof(Maximum),
            typeof(double),
            typeof(TickBar),
            new PropertyMetadata(0.0, OnTicksDataChanged));

        public static readonly DependencyProperty TickBrushProperty = DependencyProperty.Register(
            nameof(TickBrush),
            typeof(Brush),
            typeof(TickBar),
            new PropertyMetadata(null, OnTicksDataChanged));

        public static readonly DependencyProperty TickColorProperty = DependencyProperty.Register(
            nameof(TickColor),
            typeof(Color),
            typeof(TickBar),
            new PropertyMetadata(Color.FromArgb(0x99, 0xFF, 0xFF, 0xFF), OnTicksDataChanged));

        public static readonly DependencyProperty TickWidthProperty = DependencyProperty.Register(
            nameof(TickWidth),
            typeof(double),
            typeof(TickBar),
            new PropertyMetadata(1.0, OnTicksLayoutChanged));

        public static readonly DependencyProperty TickHeightProperty = DependencyProperty.Register(
            nameof(TickHeight),
            typeof(double),
            typeof(TickBar),
            new PropertyMetadata(double.NaN, OnTicksLayoutChanged));

        public IReadOnlyList<double>? Values
        {
            get => (IReadOnlyList<double>?)GetValue(ValuesProperty);
            set => SetValue(ValuesProperty, value);
        }

        public double Maximum
        {
            get => (double)GetValue(MaximumProperty);
            set => SetValue(MaximumProperty, value);
        }

        public Brush? TickBrush
        {
            get => (Brush?)GetValue(TickBrushProperty);
            set => SetValue(TickBrushProperty, value);
        }

        public Color TickColor
        {
            get => (Color)GetValue(TickColorProperty);
            set => SetValue(TickColorProperty, value);
        }

        public double TickWidth
        {
            get => (double)GetValue(TickWidthProperty);
            set => SetValue(TickWidthProperty, value);
        }

        public double TickHeight
        {
            get => (double)GetValue(TickHeightProperty);
            set => SetValue(TickHeightProperty, value);
        }

        private static void OnTicksDataChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (TickBar)d;
            control.RebuildChildren();
            control.InvalidateMeasure();
        }

        private static void OnTicksLayoutChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((TickBar)d).InvalidateMeasure();
        }

        private void RebuildChildren()
        {
            Children.Clear();

            if (Values is null)
            {
                return;
            }

            Brush brush = TickBrush ?? new SolidColorBrush(TickColor);

            foreach (double value in Values)
            {
                if (value < 0 || (Maximum > 0 && value > Maximum))
                {
                    continue;
                }

                Children.Add(new Rectangle
                {
                    Width = TickWidth,
                    Fill = brush,
                });
            }
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            double width = double.IsInfinity(availableSize.Width) ? 0 : availableSize.Width;
            double height = double.IsInfinity(availableSize.Height) ? 0 : availableSize.Height;
            double tickHeight = EffectiveTickHeight(height);

            foreach (UIElement child in Children)
            {
                child.Measure(new Size(TickWidth, tickHeight));
            }

            return new Size(width, height);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            if (Maximum <= 0 || Children.Count == 0 || Values is null)
            {
                return finalSize;
            }

            double tickHeight = EffectiveTickHeight(finalSize.Height);

            for (int i = 0; i < Children.Count; i++)
            {
                if (Children[i] is not Rectangle tick || i >= Values.Count)
                {
                    continue;
                }

                double fraction = Math.Clamp(Values[i] / Maximum, 0.0, 1.0);
                double x = fraction * Math.Max(0, finalSize.Width);
                tick.Arrange(new Rect(x, 0, TickWidth, tickHeight));
            }

            return finalSize;
        }

        private double EffectiveTickHeight(double panelHeight)
        {
            return double.IsNaN(TickHeight) ? Math.Max(0, panelHeight) : TickHeight;
        }
    }
}