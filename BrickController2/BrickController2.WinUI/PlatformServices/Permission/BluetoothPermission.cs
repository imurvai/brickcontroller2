using BrickController2.PlatformServices.Permission;
using System;
using System.Collections.Generic;
using static Microsoft.Maui.ApplicationModel.Permissions;

namespace BrickController2.Windows.PlatformServices.Permission;

public class BluetoothPermission : BasePlatformPermission, IBluetoothPermission
{
    protected override Func<IEnumerable<string>> RequiredDeclarations => () => ["bluetooth"];
}