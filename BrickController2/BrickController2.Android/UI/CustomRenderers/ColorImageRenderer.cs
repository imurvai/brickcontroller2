using Android.Graphics;
using Android.Widget;
using BrickController2.UI.Controls;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Compatibility.Platform.Android;
using Microsoft.Maui.Handlers;

namespace BrickController2.Droid.UI.CustomRenderers
{
    public class ColorImageRenderer : ImageHandler
    {
        public static readonly PropertyMapper<ColorImage, ColorImageRenderer> PropertyMapper = new(ViewHandler.ViewMapper)
        {
            [ColorImage.ColorProperty.PropertyName] = SetColor,
            [ColorImage.SourceProperty.PropertyName] = SetColor
        };

        private static IResourceDictionary ResourceDicrionary => Application.Current.Resources;

        protected override void ConnectHandler(ImageView platformView)
        {
            base.ConnectHandler(platformView);
            SetColor();

            // react on theme change
            ResourceDicrionary.ValuesChanged += (sender, args) => SetColor();
        }

        private void SetColor() => SetColor(this, VirtualView as ColorImage);

        private static void SetColor(ColorImageRenderer handler, ColorImage colorImage)
        {
            if (colorImage is null)
            {
                return;
            }

            if (colorImage.Color.Equals(Colors.Transparent))
            {
                if (handler.PlatformView.ColorFilter != null)
                {
                    handler.PlatformView.ClearColorFilter();
                }
            }
            else
            {
                var colorFilter = new PorterDuffColorFilter(colorImage.Color.ToAndroid(), PorterDuff.Mode.SrcIn);
                handler.PlatformView.SetColorFilter(colorFilter);
            }
        }
    }
}