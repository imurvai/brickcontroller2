using System.Collections.Generic;
using System.Linq;

namespace BrickController2.Helpers
{
    public class VolatileBuffer<T> where T : notnull
    {
        private readonly T[] _buffer;

        public VolatileBuffer(int capacity)
        {
            _buffer = new T[capacity];

            for (int i = 0; i < capacity; i++)
            {
                _buffer[i] = default!;
            }
        }

        public VolatileBuffer(IEnumerable<T> buffer)
        {
            var length = buffer.Count();
            _buffer = new T[length];

            var index = 0;
            foreach (var item in buffer)
            {
                _buffer[index++] = item;
            }
        }

        public T this[int index]
        {
            get
            {
                lock (_buffer)
                {
                    return _buffer[index];
                }
            }
            set
            {
                lock (_buffer)
                {
                    _buffer[index] = value;
                }
            }
        }
    }
}
