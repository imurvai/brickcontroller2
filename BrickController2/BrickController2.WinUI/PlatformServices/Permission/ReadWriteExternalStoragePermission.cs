using BrickController2.PlatformServices.Permission;
using static Microsoft.Maui.ApplicationModel.Permissions;

namespace BrickController2.Windows.PlatformServices.Permission;

public class ReadWriteExternalStoragePermission : BasePlatformPermission, IReadWriteExternalStoragePermission
{
    protected override Func<IEnumerable<string>> RequiredDeclarations => () => new[] { "removableStorage" };
}