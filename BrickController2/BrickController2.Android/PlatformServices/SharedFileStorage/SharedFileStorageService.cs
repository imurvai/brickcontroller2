using Android.OS;
using BrickController2.PlatformServices.SharedFileStorage;

namespace BrickController2.Droid.PlatformServices.SharedFileStorage
{
    public class SharedFileStorageService : ISharedFileStorageService
    {
        public string GetSharedStorageDirectory()
        {
#pragma warning disable CS0618 // Type or member is obsolete
            return Environment.GetExternalStoragePublicDirectory(Environment.DirectoryDocuments).AbsolutePath;
#pragma warning restore CS0618 // Type or member is obsolete
        }
    }
}