using System.Threading;
using System.Threading.Tasks;

namespace BrickController2.Helpers
{
    public static class ManualResetEventSlimExtensions
    {
        public static async Task<bool> WaitAsync(this ManualResetEventSlim resetEvent, CancellationToken token)
        {
            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            using (new DisposableWrapper<RegisteredWaitHandle>(
                ThreadPool.RegisterWaitForSingleObject(
                    resetEvent.WaitHandle,
                    (state, isTimeout) => tcs.TrySetResult(!isTimeout),
                    null,
                    -1,
                    true),
                (handle) => handle?.Unregister(null)))
            using (token.Register(() => tcs.TrySetCanceled()))
            {
                return await tcs.Task;
            }
        }
    }
}
