using System;
using Xamarin.Forms;

namespace BrickController2.Helpers
{
    public static class ThreadHelper
    {
        private static int _mainThreadId;

        public static void Init()
        {
            _mainThreadId = Environment.CurrentManagedThreadId;
        }

        public static bool IsOnMainThread => Environment.CurrentManagedThreadId == _mainThreadId;

        public static void RunOnMainThread(Action action)
        {
            if (IsOnMainThread)
            {
                action.Invoke();
            }
            else
            {
                Device.BeginInvokeOnMainThread(action);
            }
        }
    }
}
