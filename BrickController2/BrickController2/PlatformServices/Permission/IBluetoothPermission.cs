namespace BrickController2.PlatformServices.Permission
{
    public interface IBluetoothPermission
    {
        Task<PermissionStatus> CheckStatusAsync();
        Task<PermissionStatus> RequestAsync();
    }
}
