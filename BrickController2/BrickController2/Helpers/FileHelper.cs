using System.Text.RegularExpressions;

namespace BrickController2.Helpers
{
    public static class FileHelper
    {
        private static Regex _filenameWithoutExtensionPattern = new Regex(@"^\w+(\w\s)*");

        public static string CreationFileExtension => "bc2c";
        public static string ControllerProfileFileExtension => "bc2p";
        public static string SequenceFileExtension => "bc2s";

        public static bool FilenameValidator(string filename)
        {
            return _filenameWithoutExtensionPattern.IsMatch(filename);
        }
    }
}
