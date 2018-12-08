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

            return ImageSource.FromResource($"{ResourceHelper.ImageResourceRootNameSpace}.{Source}");
        }
    }
}
