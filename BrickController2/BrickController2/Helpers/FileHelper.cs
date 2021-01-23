using System.Collections.Generic;
using System.IO;

namespace BrickController2.Helpers
{
    public static class FileHelper
    {
        public static IEnumerable<string> GetFilesFromDirectory(string directoryPath, string filter)
        {
            return Directory.EnumerateFiles(directoryPath, filter, SearchOption.TopDirectoryOnly);
        }
    }
}
