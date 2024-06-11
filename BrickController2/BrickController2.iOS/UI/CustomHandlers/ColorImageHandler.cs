using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui;
using UIKit;
using BrickController2.UI.Controls;
using Microsoft.Maui.Platform;

namespace BrickController2.iOS.UI.CustomHandlers
{
    internal class ColorImageHandler : ImageHandler
    {
        public static readonly PropertyMapper<ColorImage, ColorImageHandler> _PropertyMapper = new(ImageHandler.Mapper)
        {
            [nameof(ColorImage.Color)] = MapColor,
            [nameof(ColorImage.Source)] = (handler, colorImage) =>
            {
                MapColor(handler, colorImage);
                MapSource(handler, colorImage);
            }
        };

        private static IResourceDictionary _ResourceDictionary => Microsoft.Maui.Controls.Application.Current!.Resources;

        public ColorImageHandler() : base(_PropertyMapper)
        {
        }

        protected override UIImageView CreatePlatformView()
        {
            _ResourceDictionary.ValuesChanged += ColorChangedHandler;

            return base.CreatePlatformView();
        }

        protected override void DisconnectHandler(UIImageView platformView)
        {
            _ResourceDictionary.ValuesChanged -= ColorChangedHandler;

            platformView.Dispose();
            base.DisconnectHandler(platformView);
        }

        private static void MapColor(ColorImageHandler handler, ColorImage colorImage)
        {
            if (colorImage.Color == Colors.Transparent)
            {
                handler.PlatformView.Image = handler.PlatformView.Image?.ImageWithRenderingMode(UIImageRenderingMode.Automatic);
                handler.PlatformView.TintColor = null;
            }
            else
            {
                handler.PlatformView.Image = handler.PlatformView.Image?.ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
                handler.PlatformView.TintColor = colorImage.Color.ToPlatform();
            }
        }

        private void ColorChangedHandler(object? sender, ResourcesChangedEventArgs e)
        {
            if (VirtualView is ColorImage colorImage)
            {
                MapColor(this, colorImage);
            }
        }
    }
}
