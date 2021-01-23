using System;
using BrickController2.PlatformServices.SharedFileStorage;

namespace BrickController2.iOS.PlatformServices.SharedFileStorage
{
    public class SharedFileStorageService : ISharedFileStorageService
    {
        public bool IsSharedStorageAvailable => IsPermissionGranted && SharedStorageDirectory != null;

        public bool IsPermissionGranted { get ; set; }

        public string SharedStorageDirectory => Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
    }
}