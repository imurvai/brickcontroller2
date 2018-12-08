using System.Globalization;

namespace BrickController2.PlatformServices.Localization
{
    public interface ILocalizationService
    {
        CultureInfo CurrentCultureInfo { get; set; }
    }
}
