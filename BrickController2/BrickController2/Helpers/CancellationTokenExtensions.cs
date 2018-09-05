using System.Threading;
using System.Threading.Tasks;

namespace BrickController2.Helpers
{
    public static class CancellationTokenExtensions
    {
        public static Task WaitAsync(this CancellationToken token)
        {
            var tcs = new TaskCompletionSource<bool>();
            token.Register(() => tcs.SetResult(true));
            return tcs.Task;
        }
    }
}
