using Android.Content;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls.Compatibility.Platform.Android;
using BrickController2.Droid.UI.CustomRenderers;
using BrickController2.UI.Controls;

[assembly: ExportRenderer(typeof(ExtendedSlider), typeof(ExtendedSliderRenderer))]
namespace BrickController2.Droid.UI.CustomRenderers
{
#pragma warning disable CS0618 // Type or member is obsolete
    public class ExtendedSliderRenderer : SliderRenderer
#pragma warning restore CS0618 // Type or member is obsolete
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