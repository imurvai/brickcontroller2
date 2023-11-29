using BrickController2.UI.Controls;
using UIKit;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Platform;

namespace BrickController2.iOS.UI.CustomRenderers
{
    public class ColorImageRenderer : ImageHandler
    {
        public static readonly PropertyMapper<ColorImage, ColorImageRenderer> PropertyMapper = new(ViewHandler.ViewMapper)
        {
            [ColorImage.ColorProperty.PropertyName] = SetColor,
            [ColorImage.SourceProperty.PropertyName] = MapSourceAndColor
        };

        public ColorImageRenderer() : base(PropertyMapper)
        {
        }

        private static IResourceDictionary ResourceDictionary => Microsoft.Maui.Controls.Application.Current.Resources;

        protected override void ConnectHandler(UIImageView platformView)
        {
            base.ConnectHandler(platformView);
            SetColor();

            // react on theme change
            ResourceDictionary.ValuesChanged += (sender, args) => SetColor();
        }

        private void SetColor() => SetColor(this, VirtualView as ColorImage);

        private static void MapSourceAndColor(ColorImageRenderer handler, ColorImage colorImage)
        {
            SetColor(handler, colorImage);
            MapSource(handler, colorImage);
        }

        private static void SetColor(ColorImageRenderer handler, ColorImage colorImage)
        {
            if (handler.PlatformView.Image == null || colorImage is null)
            {
                return;
            }

            if (colorImage.Color.Equals(Colors.Transparent))
            {
                handler.PlatformView.Image = handler.PlatformView.Image.ImageWithRenderingMode(UIImageRenderingMode.Automatic);
                handler.PlatformView.TintColor = null;
            }
            else
            {
                handler.PlatformView.Image = handler.PlatformView.Image.ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
                handler.PlatformView.TintColor = colorImage.Color.ToPlatform();
            }
        }
    }
}