using Android.Widget;
using BrickController2.UI.Controls;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;

namespace BrickController2.Droid.UI.CustomRenderers
{
    public class ExtendedSliderHandler : SliderHandler
    {
        public static readonly PropertyMapper<ExtendedSlider, ExtendedSliderHandler> PropertyMapper = new(SliderHandler.Mapper)
        {
            [ExtendedSlider.StepProperty.PropertyName] = (handler, slider) => slider.SetValue(slider.Value)
        };

        public ExtendedSlider Slider => VirtualView as ExtendedSlider;

        public ExtendedSliderHandler() : base(PropertyMapper)
        {
        }

        protected override void ConnectHandler(SeekBar seekBar)
        {
            base.ConnectHandler(seekBar);

            seekBar.StartTrackingTouch += (sender, args) => Slider?.TouchDown();
            seekBar.StopTrackingTouch += (sender, args) => Slider?.TouchUp();
            seekBar.ProgressChanged += (sender, args) =>
            {
                if (VirtualView is not ExtendedSlider slider || !args.FromUser)
                    return;

                var min = slider.Minimum;
                var max = slider.Maximum;

                // compute raw value from progress
                var value = min + (max - min) * (args.Progress / SliderExtensions.PlatformMaxValue);
                slider.SetValue(value);
            };
        }
    }
}