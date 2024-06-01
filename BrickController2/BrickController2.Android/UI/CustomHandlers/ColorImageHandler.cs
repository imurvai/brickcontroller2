using Android.Graphics;
using Android.Widget;
using Microsoft.Maui;
using Microsoft.Maui.Controls.Compatibility.Platform.Android;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using BrickController2.UI.Controls;

namespace BrickController2.Android.UI.CustomHandlers
{
    internal class ColorImageHandler : ImageHandler
    {
        public static readonly PropertyMapper<ColorImage, ColorImageHandler> _PropertyMapper = new(ImageHandler.Mapper)
        {
            [nameof(ColorImage.Color)] = MapColor
        };

        public ColorImageHandler() : base(_PropertyMapper)
        {
        }

        protected override void ConnectHandler(ImageView platformView)
        {
            base.ConnectHandler(platformView);
        }

        protected override void DisconnectHandler(ImageView platformView)
        {
            platformView.Dispose();
            base.DisconnectHandler(platformView);
        }

        private static void MapColor(ColorImageHandler handler, ColorImage colorImage)
        {
            if (colorImage.Color == Colors.Transparent)
            {
                if (handler.PlatformView.ColorFilter is not null)
                {
                    handler.PlatformView.ClearColorFilter();
                }
            }
            else
            {
                var colorFilter = new PorterDuffColorFilter(colorImage.Color.ToAndroid(), PorterDuff.Mode.SrcIn!);
                handler.PlatformView.SetColorFilter(colorFilter);
            }
        }
    }
}
