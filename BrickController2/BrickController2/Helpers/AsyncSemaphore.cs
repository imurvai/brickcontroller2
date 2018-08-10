using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BrickController2.Helpers
{
    /// <summary>
    /// Async semaphore.
    /// http://blogs.msdn.com/b/pfxteam/archive/2012/02/12/10266983.aspx
    /// </summary>
    public class AsyncSemaphore
    {
        private readonly static Task _completed = Task.FromResult(true);
        private readonly Queue<TaskCompletionSource<bool>> _waiters = new Queue<TaskCompletionSource<bool>>();
        private int _currentCount;

        public AsyncSemaphore(int initialCount)
        {
            if (initialCount < 0) throw new ArgumentOutOfRangeException("initialCount");
            _currentCount = initialCount;
        }

        public Task WaitAsync()
        {
            lock (_waiters)
            {
                if (_currentCount > 0)
                {
                    --_currentCount;
                    return _completed;
                }
                else
                {
                    var waiter = new TaskCompletionSource<bool>();
                    _waiters.Enqueue(waiter);
                    return waiter.Task;
                }
            }
        }

        public void Release()
        {
            TaskCompletionSource<bool> toRelease = null;
            lock (_waiters)
            {
                if (_waiters.Count > 0)
                    toRelease = _waiters.Dequeue();
                else
                    ++_currentCount;
            }

            if (toRelease != null)
                toRelease.SetResult(true);
        }
    }
}
