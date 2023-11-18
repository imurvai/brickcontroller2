using BrickController2.UI.Controls;
using Microsoft.Maui.Handlers;
using UIKit;

namespace BrickController2.iOS.UI.CustomRenderers
{
    public class ExtendedSliderRenderer : SliderHandler
    {
        public static readonly PropertyMapper<ExtendedSlider, ExtendedSliderRenderer> PropertyMapper = new(ViewHandler.ViewMapper);

        private ExtendedSlider Slider => VirtualView as ExtendedSlider;

        protected override void ConnectHandler(UISlider nativeSlider)
        {
            base.ConnectHandler(nativeSlider);

            if (Slider is not null)
            {
                PlatformView.TouchDown += (sender, args) => Slider?.TouchDown();
                PlatformView.TouchUpInside += (sender, args) => Slider?.TouchUp();
                PlatformView.TouchUpOutside += (sender, args) => Slider?.TouchUp();
            }
        }
    }
}