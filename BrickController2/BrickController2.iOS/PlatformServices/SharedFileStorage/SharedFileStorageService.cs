using System;
using BrickController2.Helpers;
using BrickController2.PlatformServices.SharedFileStorage;

namespace BrickController2.iOS.PlatformServices.SharedFileStorage
{
    public class SharedFileStorageService : NotifyPropertyChangedSource, ISharedFileStorageService
    {
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

        public string SharedStorageDirectory => SharedStorageBaseDirectory;

        public string SharedStorageBaseDirectory
        {
            get
            {
                try
                {
                    return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }
    }
}