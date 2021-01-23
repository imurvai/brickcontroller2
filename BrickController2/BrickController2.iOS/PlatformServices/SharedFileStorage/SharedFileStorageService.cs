using System;
using BrickController2.PlatformServices.SharedFileStorage;

namespace BrickController2.iOS.PlatformServices.SharedFileStorage
{
    public class SharedFileStorageService : ISharedFileStorageService
    {
        public string GetSharedStorageDirectory()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        }
    }
}