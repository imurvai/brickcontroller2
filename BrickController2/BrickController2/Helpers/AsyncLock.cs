using System;
using System.Threading;
using System.Threading.Tasks;

namespace BrickController2.Helpers
{
    public class AsyncLock
    {
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1);

        public Task<IDisposable> LockAsync()
        {
            return LockAsync(CancellationToken.None);
        }

        public async Task<IDisposable> LockAsync(CancellationToken token)
        {
            await _semaphore.WaitAsync(token);
            return new Releaser(_semaphore);
        }

        private struct Releaser : IDisposable
        {
            private SemaphoreSlim? _semaphore;

            internal Releaser(SemaphoreSlim semaphore)
            {
                _semaphore = semaphore;
            }

            public void Dispose()
            {
                if (_semaphore != null)
                {
                    _semaphore.Release();
                    _semaphore = null;
                }
            }
        }
    }
}
