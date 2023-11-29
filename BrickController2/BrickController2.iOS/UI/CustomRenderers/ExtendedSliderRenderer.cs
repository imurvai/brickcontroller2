using BrickController2.UI.Controls;
using Microsoft.Maui.Handlers;
using UIKit;

namespace BrickController2.iOS.UI.CustomRenderers
{
    public class ExtendedSliderRenderer : SliderHandler
    {
        protected override void ConnectHandler(UISlider nativeSlider)
        {
            base.ConnectHandler(nativeSlider);

            if (VirtualView is ExtendedSlider slider)
            {
                nativeSlider.TouchDown += (sender, args) => slider.TouchDown();
                nativeSlider.TouchUpInside += (sender, args) => slider.TouchUp();
                nativeSlider.TouchUpOutside += (sender, args) => slider.TouchUp();
            }
        }
    }
}