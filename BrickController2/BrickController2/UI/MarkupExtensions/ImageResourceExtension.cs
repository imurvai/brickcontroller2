using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace BrickController2.UI.MarkupExtensions
{
    [ContentProperty(nameof(Source))]
    public class ImageResourceExtension : IMarkupExtension
    {
        private const string ImagesNamespacePrefix = "BrickController2.UI.Images.";

        public string Source { get; set; }

        public object ProvideValue(IServiceProvider serviceProvider)
        {
            if (string.IsNullOrEmpty(Source))
            {
                return null;
            }

            return ImageSource.FromResource($"{ImagesNamespacePrefix}{Source}");
        }
    }
}
