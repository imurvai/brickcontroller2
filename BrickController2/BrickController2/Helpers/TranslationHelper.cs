using BrickController2.PlatformServices.Localization;
using Microsoft.Maui;

namespace BrickController2.Helpers
{
    public static class TranslationHelper
    {
        private static ILocalizationService _localizationService = null;

        public static string Translate(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return string.Empty;
            }

            if (_localizationService == null)
            {
                _localizationService = DependencyService.Get<ILocalizationService>();
            }

            var translation = ResourceHelper.TranslationResourceManager.GetString(key, _localizationService?.CurrentCultureInfo);
            return translation ?? key;
        }
    }
}
