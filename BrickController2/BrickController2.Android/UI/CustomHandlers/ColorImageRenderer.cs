using Android.Graphics;
using Android.Widget;
using BrickController2.UI.Controls;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Platform;
using Microsoft.Maui.Handlers;

namespace BrickController2.Droid.UI.CustomRenderers
{
    public class ColorImageHnadler : ImageHandler
    {
        public static readonly PropertyMapper<ColorImage, ColorImageHnadler> PropertyMapper = new(ImageHandler.Mapper)
        {
            [ColorImage.ColorProperty.PropertyName] = SetColor,
            [ColorImage.SourceProperty.PropertyName] = MapSourceAndColor
        };

        public ColorImageHnadler() : base(PropertyMapper)
        {
        }

        private static IResourceDictionary ResourceDictionary => Application.Current.Resources;

        protected override void ConnectHandler(ImageView platformView)
        {
            base.ConnectHandler(platformView);
            SetColor();

            // react on theme change
            ResourceDictionary.ValuesChanged += (sender, args) => SetColor();
        }

        private void SetColor() => SetColor(this, VirtualView as ColorImage);

        private static void MapSourceAndColor(ColorImageHnadler handler, ColorImage colorImage)
        { 
            SetColor(handler, colorImage);
            MapSource(handler, colorImage);
        }

        private static void SetColor(ColorImageHnadler handler, ColorImage colorImage)
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
                var colorFilter = new PorterDuffColorFilter(colorImage.Color.ToPlatform(), PorterDuff.Mode.SrcIn);
                handler.PlatformView.SetColorFilter(colorFilter);
            }
        }
    }
}