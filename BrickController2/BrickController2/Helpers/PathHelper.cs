using System;
using System.IO;

namespace BrickController2.Helpers
{
    public static class PathHelper
    {
        public static string AddAppDataPathToFilename(string filename)
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), filename);
        }
    }
}
