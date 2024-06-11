using System.Collections.Generic;
using Android;
using static Microsoft.Maui.ApplicationModel.Permissions;
using BrickController2.PlatformServices.Permission;

namespace BrickController2.Droid.PlatformServices.Permission
{
    public class ReadWriteExternalStoragePermission : BasePlatformPermission, IReadWriteExternalStoragePermission
    {
        public override (string androidPermission, bool isRuntime)[] RequiredPermissions => new List<(string androidPermission, bool isRuntime)>
            {
                (Manifest.Permission.ReadExternalStorage, true),
                (Manifest.Permission.WriteExternalStorage, true)
            }.ToArray();
    }
}