using System.Collections.Generic;
using Android;
using Android.OS;
using static Microsoft.Maui.ApplicationModel.Permissions;
using BrickController2.PlatformServices.Permission;

namespace BrickController2.Droid.PlatformServices.Permission
{
    internal class BluetoothPermission : BasePlatformPermission, IBluetoothPermission
    {
        public override (string androidPermission, bool isRuntime)[] RequiredPermissions
        {
            get
            {
                var permissions = new List<(string androidPermission, bool isRuntime)>();

                if (Build.VERSION.SdkInt <= BuildVersionCodes.R)
                {
                    permissions.Add((Manifest.Permission.Bluetooth, true));
                    permissions.Add((Manifest.Permission.BluetoothAdmin, true));
                    permissions.Add((Manifest.Permission.AccessFineLocation, true));
                    permissions.Add((Manifest.Permission.AccessCoarseLocation, true));
                }
                else
                {
#pragma warning disable CA1416 // Validate platform compatibility
                    permissions.Add((Manifest.Permission.BluetoothConnect, true));
                    permissions.Add((Manifest.Permission.BluetoothScan, true));
#pragma warning restore CA1416 // Validate platform compatibility
                }

                return permissions.ToArray();
            }
        }
    }
}