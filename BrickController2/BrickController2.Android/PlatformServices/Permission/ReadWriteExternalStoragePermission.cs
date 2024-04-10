using Android.OS;
using System;
using static Xamarin.Essentials.Permissions;
using BrickController2.PlatformServices.Permission;

namespace BrickController2.Droid.PlatformServices.Permission
{
    public class ReadWriteExternalStoragePermission : BasePlatformPermission, IReadWriteExternalStoragePermission
    {
        public override (string androidPermission, bool isRuntime)[] RequiredPermissions => ((int)Build.VERSION.SdkInt <= 32) ?
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