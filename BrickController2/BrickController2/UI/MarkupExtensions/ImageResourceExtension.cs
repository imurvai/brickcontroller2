﻿using System;
using BrickController2.Helpers;
using Microsoft.Maui;
using Microsoft.Maui.Controls.Xaml;

namespace BrickController2.UI.MarkupExtensions
{
    [ContentProperty(nameof(Source))]
    public class ImageResourceExtension : IMarkupExtension<ImageSource>
    {
        public string Source { get; set; }

        public ImageSource ProvideValue(IServiceProvider serviceProvider)
        {
            return ProvideValueInternal();
        }

        object IMarkupExtension.ProvideValue(IServiceProvider serviceProvider)
        {
            return ProvideValueInternal();
        }

        private ImageSource ProvideValueInternal()
        {
            if (string.IsNullOrEmpty(Source))
            {
                return null;
            }

            return ResourceHelper.GetImageResource(Source);
        }
    }
}
