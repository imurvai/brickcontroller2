using Android.Widget;
using BrickController2.UI.Controls;
using Microsoft.Maui.Handlers;

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
                if (args.FromUser)
                {
                    VirtualView.Value = VirtualView.Minimum + ((VirtualView.Maximum - VirtualView.Minimum) * args.Progress / int.MaxValue);
                }
            };
        }
    }
}