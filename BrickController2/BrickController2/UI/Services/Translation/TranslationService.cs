using BrickController2.Helpers;
using BrickController2.PlatformServices.Localization;

namespace BrickController2.UI.Services.Translation
{
    public class TranslationService : ITranslationService
    {
        private readonly ILocalizationService _localizationService;

        public TranslationService(ILocalizationService localizationService)
        {
            _localizationService = localizationService;
        }

        public string Translate(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return string.Empty;
            }

            var translation = ResourceHelper.TranslationResourceManager.GetString(key, _localizationService.CurrentCultureInfo);
            return translation ?? key;
        }
    }
}
