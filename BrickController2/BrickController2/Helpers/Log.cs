using System;

namespace BrickController2.Helpers
{
    public static class Log
    {
        public static void Debug(string message)
        {
#if DEBUG
            System.Diagnostics.Debug.WriteLine(message);
#endif
        }

        public static void Error(string message, Exception? ex = null)
        {
#if DEBUG
            System.Diagnostics.Debug.WriteLine(message);
            if (ex is not null)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }
#endif
        }
    }
}
