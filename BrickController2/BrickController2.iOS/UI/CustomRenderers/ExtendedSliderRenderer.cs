using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls.Compatibility.Platform.iOS;
using Microsoft.Maui.Controls.Platform;
using BrickController2.iOS.UI.CustomRenderers;
using BrickController2.UI.Controls;

[assembly: ExportRenderer(typeof(ExtendedSlider), typeof(ExtendedSliderRenderer))]
namespace BrickController2.iOS.UI.CustomRenderers
{
    public class ExtendedSliderRenderer : SliderRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Slider> e)
        {
            base.OnElementChanged(e);

            if (Element is ExtendedSlider extendedSlider && Control != null)
            {
                Control.TouchDown += (sender, args) => extendedSlider.TouchDown();
                Control.TouchUpInside += (sender, args) => extendedSlider.TouchUp();
                Control.TouchUpOutside += (sender, args) => extendedSlider.TouchUp();
            }
        }
    }
}