﻿namespace BrickController2.PlatformServices.SharedFileStorage
{
    public interface ISharedFileStorageService
    {
        bool IsSharedStorageAvailable { get; }

        bool IsPermissionGranted { get; set; }

        string SharedStorageDirectory { get; }
    }
}
