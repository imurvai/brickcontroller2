using System;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace BrickController2.UI.Services.UIThread
{
    public class UIThreadService : IUIThreadService
    {
        private int _mainThreadId;

        public UIThreadService()
        {
            _mainThreadId = Environment.CurrentManagedThreadId;
        }

        public bool IsOnMainThread => Environment.CurrentManagedThreadId == _mainThreadId;

        public async Task RunOnMainThread(Action action)
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
