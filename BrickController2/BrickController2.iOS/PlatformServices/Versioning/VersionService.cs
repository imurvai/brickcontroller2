using BrickController2.PlatformServices.Versioning;
using Foundation;

namespace BrickController2.iOS.PlatformServices.Versioning
{
    public class VersionService : IVersionService
    {
        public string ApplicationVersion => NSBundle.MainBundle.ObjectForInfoDictionary("CFBundleShortVersionString").ToString();
    }
}