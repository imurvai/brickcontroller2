using Android.Content;
using Android.Graphics;
using BrickController2.Droid.UI.CustomRenderers;
using BrickController2.UI.Controls;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls.Compatibility.Platform.Android;
using Microsoft.Maui.Controls.Platform;
using System.ComponentModel;

[assembly: ExportRenderer(typeof(ColorImage), typeof(ColorImageRenderer))]
namespace BrickController2.Droid.UI.CustomRenderers
{
    public class ColorImageRenderer : ImageRenderer
    {
        public ColorImageRenderer(Context context) : base(context)
        {
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Image> e)
        {
            base.OnElementChanged(e);
            SetColor();
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            if (e.PropertyName == ColorImage.ColorProperty.PropertyName || e.PropertyName == ColorImage.SourceProperty.PropertyName)
            {
                SetColor();
            }
        }

        private void SetColor()
        {
            if (Control == null || !(Element is ColorImage colorImage))
            {
                return;
            }

            if (colorImage.Color.Equals(Colors.Transparent))
            {
                if (Control.ColorFilter != null)
                {
                    Control.ClearColorFilter();
                }
            }
            else
            {
                var colorFilter = new PorterDuffColorFilter(colorImage.Color.ToAndroid(), PorterDuff.Mode.SrcIn);
                Control.SetColorFilter(colorFilter);
            }
        }
    }
}