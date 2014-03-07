using System.Collections.Generic;
using System.Linq;

namespace ApiClient
{
    public class PriorityQueue<T>
    {
        private int _size;
        private List<KeyValuePair<Priority, Queue<T>>> _storage;

        public PriorityQueue()
        {
            _storage = new List<KeyValuePair<Priority, Queue<T>>>();
            _size = 0;
        }

        public bool IsEmpty()
        {
            return _size == 0;
        }

        public T Dequeue()
        {
            if (!IsEmpty())
            {
                foreach (var kvp in _storage)
                {
                    if (kvp.Value != null && kvp.Value.Count > 0)
                    {
                        _size--;
                        return kvp.Value.Dequeue();
                    }
                }
            }

            return default(T);
        }

        public T Peek()
        {
            if (!IsEmpty())
            {
                foreach (var kvp in _storage)
                {
                    if (kvp.Value != null && kvp.Value.Count > 0)
                    {
                        return kvp.Value.Peek();
                    }
                }
            }

            return default(T);
        }

        public T Dequeue(Priority priority)
        {
            var queue = (from kvp in _storage
                         where kvp.Key == priority
                         select kvp.Value).FirstOrDefault();

            if (queue != null && queue.Count > 0)
            {
                _size--;
                return queue.Dequeue();
            }

            return default(T);
        }

        public void Enqueue(T item, Priority priority)
        {
            var queue = (from kvp in _storage
                         where kvp.Key == priority
                         select kvp.Value).FirstOrDefault();

            if (queue == null)
            {
                _storage.Add(new KeyValuePair<Priority, Queue<T>>(priority, new Queue<T>()));
                _storage.Sort((x, y) => { return x.Key.CompareTo(y.Key); });
                Enqueue(item, priority);
            }
            else
            {
                queue.Enqueue(item);
                _size++;
            }
        }
    }
}
