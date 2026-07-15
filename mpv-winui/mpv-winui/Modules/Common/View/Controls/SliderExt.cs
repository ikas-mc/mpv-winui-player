using Microsoft.UI.Xaml.Controls;
using System;

namespace mpv_winui.Modules.Common.View.Controls
{
    public partial class SliderExt : Slider
    {
        private bool _userInteractionActive;

        public Action<Slider, double>? ValueChanged2;

        public SliderExt()
        {
            DefaultStyleKey = typeof(SliderExt);
            _userInteractionActive = true;
        }

        public double Value2
        {
            get => Value;
            set
            {
                _userInteractionActive = false;
                Value = value;
                _userInteractionActive = true;
            }
        }

        protected override void OnValueChanged(double oldValue, double newValue)
        {
            base.OnValueChanged(oldValue, newValue);

            if (_userInteractionActive)
            {
                ValueChanged2?.Invoke(this, newValue);
            }
        }
    }
}