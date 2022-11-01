using System;

namespace BrickController2.Helpers
{
    public class DisposableWrapper<T> : IDisposable where T : class
    {
        private T _objectToDispose;
        private readonly Action<T> _disposeAction;

        public DisposableWrapper(T objectToDispose, Action<T> disposeAction)
        {
            _objectToDispose = objectToDispose;
            _disposeAction = disposeAction;
        }

        public void Dispose()
        {
            if (_objectToDispose != null)
            {
                _disposeAction?.Invoke(_objectToDispose);
                _objectToDispose = null;
            }
        }
    }
}
