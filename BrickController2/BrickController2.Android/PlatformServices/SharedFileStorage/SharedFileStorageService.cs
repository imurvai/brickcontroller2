using Android.OS;
using BrickController2.Helpers;
using BrickController2.PlatformServices.SharedFileStorage;
using Environment = Android.OS.Environment;

namespace BrickController2.Droid.PlatformServices.SharedFileStorage
{
    public class SharedFileStorageService : NotifyPropertyChangedSource, ISharedFileStorageService
    {
        private static string _brickController2SharedDirectory = "BrickController2";

        public bool _isPermissionGranted = false;

        public bool IsSharedStorageAvailable => IsPermissionGranted && SharedStorageDirectory != null;

        public bool IsPermissionGranted
        {
            get { return _isPermissionGranted; }
            set
            {
                _isPermissionGranted = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(IsSharedStorageAvailable));
            }
        }

        public string SharedStorageDirectory
        {
            get
            {
                try
                {
#pragma warning disable CS0618 // Type or member is obsolete
                    var storageDirectory = Environment.ExternalStorageDirectory?.AbsolutePath;
                    var storageState = Environment.ExternalStorageState;
#pragma warning restore CS0618 // Type or member is obsolete

                    if (storageDirectory == null || !Directory.Exists(storageDirectory) || !Environment.MediaMounted.Equals(storageState))
                    {
                        return null;
                    }

                    var bc2StorageDirectory = Path.Combine(storageDirectory, _brickController2SharedDirectory);

                    if (!Directory.Exists(bc2StorageDirectory))
                    {
                        Directory.CreateDirectory(bc2StorageDirectory);
                    }

                    return bc2StorageDirectory;
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }
    }
}