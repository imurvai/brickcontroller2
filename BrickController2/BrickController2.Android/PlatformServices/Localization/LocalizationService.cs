using BrickController2.PlatformServices.Localization;
using System.Globalization;

[assembly:Dependency(typeof(BrickController2.Droid.PlatformServices.Localization.LocalizationService))]
namespace BrickController2.Droid.PlatformServices.Localization
{
    public class LocalizationService : ILocalizationService
    {
        private Java.Util.Locale _androidLocale;
        private CultureInfo _ci = null;

        public CultureInfo CurrentCultureInfo
        {
            get
            {
                var netLanguage = "en";
                var androidLocale = Java.Util.Locale.Default;

                if (_ci == null || androidLocale != _androidLocale)
                {
                    _androidLocale = androidLocale;
                    netLanguage = AndroidToDotnetLanguage(_androidLocale.ToString().Replace("_", "-"));

                    try
                    {
                        _ci = new CultureInfo(netLanguage);
                    }
                    catch (CultureNotFoundException)
                    {
                        try
                        {
                            var fallback = ToDotnetFallbackLanguage(new PlatformCulture(netLanguage));
                            _ci = new CultureInfo(fallback);
                        }
                        catch (CultureNotFoundException)
                        {
                            _ci = new CultureInfo("en");
                        }
                    }
                }

                return _ci;
            }

            set
            {
                Thread.CurrentThread.CurrentCulture = value;
                Thread.CurrentThread.CurrentUICulture = value;
            }
        }

        private string AndroidToDotnetLanguage(string androidLanguage)
        {
            var netLanguage = androidLanguage;

            //certain languages need to be converted to CultureInfo equivalent
            switch (androidLanguage)
            {
                case "ms-BN":   // "Malaysian (Brunei)" not supported .NET culture
                case "ms-MY":   // "Malaysian (Malaysia)" not supported .NET culture
                case "ms-SG":   // "Malaysian (Singapore)" not supported .NET culture
                    netLanguage = "ms"; // closest supported
                    break;

                case "in-ID":  // "Indonesian (Indonesia)" has different code in  .NET
                    netLanguage = "id-ID"; // correct code for .NET
                    break;

                case "gsw-CH":  // "Schwiizertüütsch (Swiss German)" not supported .NET culture
                    netLanguage = "de-CH"; // closest supported
                    break;
                    // add more application-specific cases here (if required)
                    // ONLY use cultures that have been tested and known to work
            }

            return netLanguage;
        }

        private string ToDotnetFallbackLanguage(PlatformCulture platCulture)
        {
            var netLanguage = platCulture.LanguageCode; // use the first part of the identifier (two chars, usually);
            switch (platCulture.LanguageCode)
            {
                case "gsw":
                    netLanguage = "de-CH"; // equivalent to German (Switzerland) for this app
                    break;
                    // add more application-specific cases here (if required)
                    // ONLY use cultures that have been tested and known to work
            }

            return netLanguage;
        }
    }
}