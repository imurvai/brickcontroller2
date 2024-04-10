using Android.OS;
using BrickController2.PlatformServices.Permission;
using static Microsoft.Maui.ApplicationModel.Permissions;

namespace BrickController2.Droid.PlatformServices.Permission
{
    public class ReadWriteExternalStoragePermission : BasePlatformPermission, IReadWriteExternalStoragePermission
    {
        public override (string androidPermission, bool isRuntime)[] RequiredPermissions => (Build.VERSION.SdkInt <= BuildVersionCodes.SV2) ?
                // Android API 32 and older - ask for permissions
                new (string androidPermission, bool isRuntime)[]
                {
                    (Android.Manifest.Permission.ReadExternalStorage, true),
                    (Android.Manifest.Permission.WriteExternalStorage, true)
                } :
                // Android API 33+ does not support permissions to external storage
                // Let it be Granted (via empty permission list)
                Array.Empty<(string androidPermission, bool isRuntime)>();
    }
}