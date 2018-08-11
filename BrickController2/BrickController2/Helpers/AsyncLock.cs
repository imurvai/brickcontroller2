using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BrickController2.Helpers
{
    public class AsyncLock
    {
        private readonly Queue<TaskCompletionSource<Releaser>> _waiters = new Queue<TaskCompletionSource<Releaser>>();
        private readonly object _lock = new object();
        private bool _acquired = false;

        public AsyncLock()
        {
        }

        public Task<Releaser> LockAsync()
        {
            lock (_lock)
            {
                if (_acquired)
                {
                    var waiter = new TaskCompletionSource<Releaser>();
                    _waiters.Enqueue(waiter);
                    return waiter.Task;
                }
                else
                {
                    _acquired = true;
                    return Task.FromResult(new Releaser(this));
                }
            }
        }

        private void Release()
        {
            lock (_lock)
            {
                if (_waiters.Any())
                {
                    var waiter = _waiters.Dequeue();
                    waiter.SetResult(new Releaser(this));
                }
                else
                {
                    _acquired = false;
                }
            }
        }

        public struct Releaser : IDisposable
        {
            private AsyncLock _owner;

            internal Releaser(AsyncLock owner)
            {
                _owner = owner;
            }

            public void Dispose()
            {
                if (_owner != null)
                {
                    _owner.Release();
                    _owner = null;
                }
            }
        }
    }
}
