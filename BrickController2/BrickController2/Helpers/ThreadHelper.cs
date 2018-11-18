using System;
using System.Threading.Tasks;
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

        public static async Task RunOnMainThread(Action action)
        {
            if (IsOnMainThread)
            {
                action.Invoke();
            }
            else
            {
                var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

                Device.BeginInvokeOnMainThread(() =>
                {
                    try
                    {
                        action.Invoke();
                        tcs.SetResult(true);
                    }
                    catch (Exception e)
                    {
                        tcs.SetException(e);
                    }
                });

                await tcs.Task;
            }
        }
    }
}
