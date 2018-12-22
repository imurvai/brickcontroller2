using BrickController2.Helpers;

namespace BrickController2.UI.Services.Translation
{
    public class TranslationService : ITranslationService
    {
        public string Translate(string key)
        {
            return TranslationHelper.Translate(key);
        }
    }
}
