using BrickController2.UI.Services.Translation;

namespace BrickController2.Helpers
{
    public static class TranslationHelper
    {
        private static ITranslationService _translationService = null;

        public static string Translate(string key)
        {
            _translationService ??= IPlatformApplication.Current.Services.GetRequiredService<ITranslationService>();

            return _translationService.Translate(key);
        }
    }
}
