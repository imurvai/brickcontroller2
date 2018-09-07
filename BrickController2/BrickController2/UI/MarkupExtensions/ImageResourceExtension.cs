using System;
using BrickController2.Helpers;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace BrickController2.UI.MarkupExtensions
{
    [ContentProperty(nameof(Source))]
    public class ImageResourceExtension : IMarkupExtension<ImageSource>
    {
        public string Source { get; set; }

        public ImageSource ProvideValue(IServiceProvider serviceProvider)
        {
            return ProvideValueInternal(serviceProvider);
        }

        object IMarkupExtension.ProvideValue(IServiceProvider serviceProvider)
        {
            return ProvideValueInternal(serviceProvider);
        }

        private ImageSource ProvideValueInternal(IServiceProvider serviceProvider)
        {
            if (string.IsNullOrEmpty(Source))
            {
                return null;
            }

            return ImageSource.FromResource($"{ImageHelper.ImageResourceRootNameSpace}.{Source}");
        }
    }
}
