using BrickController2.PlatformServices.SharedFileStorage;
using Windows.Storage;

namespace BrickController2.Windows.PlatformServices.SharedFileStorage;

public class SharedFileStorageService : ISharedFileStorageService
{
    public bool IsSharedStorageAvailable => true;

    public bool IsPermissionGranted { get; set; }

    public string SharedStorageBaseDirectory => ApplicationData.Current.RoamingFolder.Path;

    public string SharedStorageDirectory => SharedStorageBaseDirectory;

}
