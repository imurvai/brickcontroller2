using System.Globalization;
using BrickController2.PlatformServices.Localization;
using Foundation;

[assembly: Dependency(typeof(BrickController2.iOS.PlatformServices.Localization.LocalizationService))]
namespace BrickController2.iOS.PlatformServices.Localization
{
    public class LocalizationService : ILocalizationService
    {
        private string _netLanguage;
        private CultureInfo _ci;

        public CultureInfo CurrentCultureInfo
        {
            get
            {
                var netLanguage = "en";
                if (NSLocale.PreferredLanguages.Length > 0)
                {
                    var iosPreferredLanguage = NSLocale.PreferredLanguages[0];
                    netLanguage = IOSToDotnetLanguage(iosPreferredLanguage);
                }

                if (_ci == null || _netLanguage != netLanguage)
                {
                    _netLanguage = netLanguage;

                    try
                    {
                        _ci = new CultureInfo(_netLanguage);
                    }
                    catch (CultureNotFoundException)
                    {
                        try
                        {
                            var fallback = ToDotnetFallbackLanguage(new PlatformCulture(_netLanguage));
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

        private string IOSToDotnetLanguage(string iOSLanguage)
        {
            var netLanguage = iOSLanguage;

            //certain languages need to be converted to CultureInfo equivalent
            switch (iOSLanguage)
            {
                case "ms-MY":   // "Malaysian (Malaysia)" not supported .NET culture
                case "ms-SG":    // "Malaysian (Singapore)" not supported .NET culture
                    netLanguage = "ms"; // closest supported
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
                case "pt":
                    netLanguage = "pt-PT"; // fallback to Portuguese (Portugal)
                    break;

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