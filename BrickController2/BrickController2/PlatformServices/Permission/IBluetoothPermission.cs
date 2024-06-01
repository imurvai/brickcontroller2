using Microsoft.Maui.ApplicationModel;
using System.Threading.Tasks;

namespace BrickController2.PlatformServices.Permission
{
    public interface IBluetoothPermission
    {
        Task<PermissionStatus> CheckStatusAsync();
        Task<PermissionStatus> RequestAsync();
    }
}
