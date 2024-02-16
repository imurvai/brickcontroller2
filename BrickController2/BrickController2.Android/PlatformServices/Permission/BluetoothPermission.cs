using BrickController2.PlatformServices.Permission;
using static Microsoft.Maui.ApplicationModel.Permissions;

namespace BrickController2.Droid.PlatformServices.Permission
{
    internal class BluetoothPermission : BasePlatformPermission, IBluetoothPermission
    {
        public override (string androidPermission, bool isRuntime)[] RequiredPermissions
        {
            get
            {
                var permissions = new List<(string androidPermission, bool isRuntime)>();

                if (Android.OS.Build.VERSION.SdkInt <= Android.OS.BuildVersionCodes.R)
                {
                    permissions.Add((Android.Manifest.Permission.Bluetooth, true));
                    permissions.Add((Android.Manifest.Permission.BluetoothAdmin, true));
                    permissions.Add((Android.Manifest.Permission.AccessFineLocation, true));
                    permissions.Add((Android.Manifest.Permission.AccessCoarseLocation, true));
                }
                else
                {
                    permissions.Add((Android.Manifest.Permission.BluetoothConnect, true));
                    permissions.Add((Android.Manifest.Permission.BluetoothScan, true));
                }

                return permissions.ToArray();
            }
        }
    }
}