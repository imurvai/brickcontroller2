using Android.Widget;
using BrickController2.UI.Controls;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;

namespace BrickController2.Droid.UI.CustomRenderers
{
    public class ExtendedSliderRenderer : SliderHandler
    {
        public static readonly PropertyMapper<ExtendedSlider, ExtendedSliderRenderer> PropertyMapper = new(ViewHandler.ViewMapper);

        private ExtendedSlider Slider => VirtualView as ExtendedSlider;

        protected override void ConnectHandler(SeekBar nativeSlider)
        {
            base.ConnectHandler(nativeSlider);

            nativeSlider.StartTrackingTouch += (sender, args) => Slider?.TouchDown();
            nativeSlider.StopTrackingTouch += (sender, args) => Slider?.TouchUp();
            nativeSlider.ProgressChanged += (sender, args) =>
            {
                if (VirtualView == null || !args.FromUser)
                    return;

                var min = VirtualView.Minimum;
                var max = VirtualView.Maximum;

                var value = min + (max - min) * (args.Progress / SliderExtensions.PlatformMaxValue);
                VirtualView.Value = value;
            };
        }
    }
}