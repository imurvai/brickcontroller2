using BrickController2.iOS.UI.CustomRenderers;
using BrickController2.UI.Controls;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

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