using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
using BrickController2.Helpers;

namespace BrickController2.UI.MarkupExtensions
{
    [ContentProperty("Key")]
    public class TranslateExtension : IMarkupExtension<string>
    {
        public required string Key { get; set; }

        public string ProvideValue(IServiceProvider serviceProvider)
        {
            return ProvideValueInternal();
        }

        object IMarkupExtension.ProvideValue(IServiceProvider serviceProvider)
        {
            return ProvideValueInternal();
        }

        private string ProvideValueInternal()
        {
            return TranslationHelper.Translate(Key);
        }
    }
}
