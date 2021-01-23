using Android.OS;
using BrickController2.PlatformServices.SharedFileStorage;
using System.IO;

namespace BrickController2.Droid.PlatformServices.SharedFileStorage
{
    public class SharedFileStorageService : ISharedFileStorageService
    {
        private static string _brickController2SharedDirectory = "BrickController2";

        public bool IsSharedStorageAvailable => IsPermissionGranted && SharedStorageDirectory != null;

        public bool IsPermissionGranted { get; set; }

        public string SharedStorageDirectory
        {
            get
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
        }
    }
}