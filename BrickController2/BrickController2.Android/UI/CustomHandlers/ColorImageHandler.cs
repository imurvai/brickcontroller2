using System;
using System.Collections.Generic;
using Android.Graphics;
using Android.Widget;
using Microsoft.Maui;
using Microsoft.Maui.Controls.Compatibility.Platform.Android;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;
using BrickController2.UI.Controls;

namespace BrickController2.Android.UI.CustomHandlers
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

        private static IResourceDictionary _ResourceDictionary => Application.Current!.Resources;

        public ColorImageHandler() : base(_PropertyMapper)
        {
        }

        protected override void ConnectHandler(ImageView platformView)
        {
            base.ConnectHandler(platformView);

            ColorChangedHandler(this, new(Array.Empty<KeyValuePair<string, object>>()));

            _ResourceDictionary.ValuesChanged += ColorChangedHandler;
        }

        protected override void DisconnectHandler(ImageView platformView)
        {
            _ResourceDictionary.ValuesChanged -= ColorChangedHandler;

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

        private void ColorChangedHandler(object? sender, ResourcesChangedEventArgs e)
        {
            if (VirtualView is ColorImage colorImage)
            {
                MapColor(this, colorImage);
            }
        }
    }
}
