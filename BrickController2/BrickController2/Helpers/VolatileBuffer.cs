using System.Collections.Generic;
using System.Linq;
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
                Volatile.Write(ref _buffer[i], default(T));
            }
        }

        public VolatileBuffer(IEnumerable<T> buffer)
        {
            var length = buffer.Count();
            _buffer = new object[length];

            var index = 0;
            foreach (var item in buffer)
            {
                Volatile.Write(ref _buffer[index++], item);
            }
        }

        public T this[int index]
        {
            get => (T)Volatile.Read(ref _buffer[index]);
            set => Volatile.Write(ref _buffer[index], value);
        }

        public static explicit operator T[](VolatileBuffer<T> volatileBuffer)
        {
            var length = volatileBuffer._buffer.Length;
            var result = new T[length];

            for (int i = 0; i < length; i++)
            {
                result[i] = volatileBuffer[i];
            }

            return result;
        }
    }
}
