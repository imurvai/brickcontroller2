using System;
using BrickController2.Helpers;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace BrickController2.UI.MarkupExtensions
{
    [ContentProperty(nameof(Source))]
    public class ImageResourceExtension : IMarkupExtension
    {
        public string Source { get; set; }

        public object ProvideValue(IServiceProvider serviceProvider)
        {
            if (string.IsNullOrEmpty(Source))
            {
                return null;
            }

            return ImageSource.FromResource($"{ImageHelper.ImageResourceRootNameSpace}.{Source}");
        }
    }
}
