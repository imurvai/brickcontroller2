﻿using System.ComponentModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls.Compatibility.Platform.iOS;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Graphics;
using UIKit;
using BrickController2.iOS.UI.CustomRenderers;
using BrickController2.UI.Controls;

[assembly:ExportRenderer(typeof(ColorImage), typeof(ColorImageRenderer))]
namespace BrickController2.iOS.UI.CustomRenderers
{
    public class ColorImageRenderer : ImageRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Image> e)
        {
            base.OnElementChanged(e);
            SetColor();
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            if (e.PropertyName == ColorImage.ColorProperty.PropertyName || e.PropertyName == ColorImage.SourceProperty.PropertyName || e.PropertyName == ColorImage.IsLoadingProperty.PropertyName)
            {
                SetColor();
            }
        }

        private void SetColor()
        {
            if (Control == null || Control.Image == null || !(Element is ColorImage colorImage))
            {
                return;
            }

            if (colorImage.Color.Equals(Colors.Transparent))
            {
                Control.Image = Control.Image.ImageWithRenderingMode(UIImageRenderingMode.Automatic);
                Control.TintColor = null;
            }
            else
            {
                Control.Image = Control.Image.ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
                Control.TintColor = colorImage.Color.ToUIColor();
            }
        }
    }
}