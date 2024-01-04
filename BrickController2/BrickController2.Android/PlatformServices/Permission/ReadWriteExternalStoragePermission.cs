using Android.OS;
using BrickController2.PlatformServices.Permission;
using static Microsoft.Maui.ApplicationModel.Permissions;

namespace BrickController2.Droid.PlatformServices.Permission
{
    public class ReadWriteExternalStoragePermission : BasePlatformPermission, IReadWriteExternalStoragePermission
    {
        public override (string androidPermission, bool isRuntime)[] RequiredPermissions => new List<(string androidPermission, bool isRuntime)>
            {
                (Android.Manifest.Permission.ReadExternalStorage, true),
                (Android.Manifest.Permission.WriteExternalStorage, true)
            }.ToArray();
    }
}