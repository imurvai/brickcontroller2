using static Xamarin.Essentials.Permissions;
using BrickController2.PlatformServices.Permission;

namespace BrickController2.iOS.PlatformServices.Permission
{
    public class ReadWriteExternalStoragePermission : BasePlatformPermission, IReadWriteExternalStoragePermission
    {
    }
}