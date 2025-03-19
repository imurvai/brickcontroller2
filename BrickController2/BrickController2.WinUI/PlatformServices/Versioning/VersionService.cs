using BrickController2.PlatformServices.Versioning;
using Windows.ApplicationModel;

namespace BrickController2.Windows.PlatformServices.Versioning;

public class VersionService : IVersionService
{
    public string ApplicationVersion
    {
        get
        {
            try
            {
                var info = Package.Current.Id.Version;
                return $"{info.Major}.{info.Minor}.{info.Revision}";
            }
            catch
            {
                return "Unknown version";
            }
        }
    }
}