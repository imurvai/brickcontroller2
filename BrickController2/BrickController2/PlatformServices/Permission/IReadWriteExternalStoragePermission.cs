namespace BrickController2.PlatformServices.Permission
{
    public interface IReadWriteExternalStoragePermission
    {
        Task<PermissionStatus> CheckStatusAsync();
        Task<PermissionStatus> RequestAsync();
    }
}
