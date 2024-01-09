using BrickController2.PlatformServices.Permission;
using static Microsoft.Maui.ApplicationModel.Permissions;

namespace BrickController2.Droid.PlatformServices.Permission
{
    internal class CameraPermission : BasePlatformPermission, ICameraPermission
    {
        public override (string androidPermission, bool isRuntime)[] RequiredPermissions
        {
            get
            {
                var permissions = new List<(string androidPermission, bool isRuntime)>
                {
                    (Android.Manifest.Permission.Camera, true)
                };

                return permissions.ToArray();
            }
        }
    }
}