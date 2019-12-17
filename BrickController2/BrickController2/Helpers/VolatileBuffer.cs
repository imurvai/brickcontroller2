using System.Threading;

namespace BrickController2.Helpers
{
    public class VolatileBuffer<T>
    {
        private readonly object[] _buffer;

        public VolatileBuffer(int capacity)
        {
            _buffer = new object[capacity];

            for (int i = 0; i < capacity; i++)
            {
                _buffer[i] = default(T);
            }
        }

        public T this[int index]
        {
            get => (T)Volatile.Read(ref _buffer[index]);
            set => Volatile.Write(ref _buffer[index], value);
        }
    }
}
