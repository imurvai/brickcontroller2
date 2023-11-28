using BrickController2.Helpers;

namespace BrickController2.UI.MarkupExtensions
{
    [ContentProperty("Key")]
    public class TranslateExtension : IMarkupExtension<string>
    {
        public string Key { get; set; }

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
