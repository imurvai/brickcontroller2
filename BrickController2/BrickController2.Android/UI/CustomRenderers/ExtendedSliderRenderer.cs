using Android.Content;
using BrickController2.Droid.UI.CustomRenderers;
using BrickController2.UI.Controls;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls.Compatibility.Platform.Android;

[assembly: ExportRenderer(typeof(ExtendedSlider), typeof(ExtendedSliderRenderer))]
namespace BrickController2.Droid.UI.CustomRenderers
{
    public class ExtendedSliderRenderer : SliderRenderer
    {
        public ExtendedSliderRenderer(Context context) : base(context)
        {
        }

        protected override void OnLayout(bool changed, int l, int t, int r, int b)
        {
            base.OnLayout(changed, l, t, r, b);

            if (Element is ExtendedSlider extendedSlider && Control != null)
            {
                Control.StartTrackingTouch += (sender, args) => extendedSlider.TouchDown();
                Control.StopTrackingTouch += (sender, args) => extendedSlider.TouchUp();
                Control.ProgressChanged += (sender, args) =>
                {
                    if (args.FromUser)
                    {
                        extendedSlider.Value = extendedSlider.Minimum + ((extendedSlider.Maximum - extendedSlider.Minimum) * args.Progress / 1000);
                    }
                };
            }
        }
    }
}