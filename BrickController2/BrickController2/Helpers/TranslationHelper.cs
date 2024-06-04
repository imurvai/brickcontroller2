using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using BrickController2.PlatformServices.Localization;

namespace BrickController2.Helpers
{
    public static class TranslationHelper
    {
        private static ILocalizationService? _localizationService = null;

        public static string Translate(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return string.Empty;
            }

            if (_localizationService == null)
            {
                _localizationService = IPlatformApplication.Current!.Services.GetRequiredService<ILocalizationService>();
            }

            var translation = ResourceHelper.TranslationResourceManager.GetString(key, _localizationService.CurrentCultureInfo);
            return translation ?? key;
        }
    }
}
