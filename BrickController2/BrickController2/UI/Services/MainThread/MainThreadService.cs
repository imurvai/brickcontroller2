using System;
using System.Threading.Tasks;

namespace BrickController2.UI.Services.MainThread
{
    public class MainThreadService : IMainThreadService
    {
        public bool IsOnMainThread => Xamarin.Essentials.MainThread.IsMainThread;

        public async Task RunOnMainThread(Action action)
        {
            if (IsOnMainThread)
            {
                action.Invoke();
            }
            else
            {
                var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

                Xamarin.Essentials.MainThread.BeginInvokeOnMainThread(() =>
                {
                    try
                    {
                        action.Invoke();
                        tcs.TrySetResult(true);
                    }
                    catch (Exception e)
                    {
                        tcs.TrySetException(e);
                    }
                });

                await tcs.Task;
            }
        }
    }
}
