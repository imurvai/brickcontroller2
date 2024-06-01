using Android.Content;
using Android.Content.PM;
using BrickController2.PlatformServices.Versioning;

namespace BrickController2.Droid.PlatformServices.Versioning
{
    public class VersionService : IVersionService
    {
        private readonly Context _context;

        public VersionService(Context context)
        {
            _context = context;
        }

        public string ApplicationVersion
        {
            get
            {
                try
                {
                    return PackageInfo?.VersionName ?? "Unkonwn version";
                }
                catch (PackageManager.NameNotFoundException)
                {
                    return "Unkonwn version";
                }
            }
        }

        private PackageInfo? PackageInfo => _context.PackageManager!.GetPackageInfo(_context.PackageName!, 0);
    }
}