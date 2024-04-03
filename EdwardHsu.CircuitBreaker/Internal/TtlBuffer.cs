using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdwardHsu.CircuitBreaker.Internal
{
    internal class TtlBuffer<T>
    {
        private TimeSpan _ttl;
        private ConcurrentQueue<(DateTime time, T obj)> _queue;
        
        public TtlBuffer(TimeSpan ttl)
        {
            _queue = new ConcurrentQueue<(DateTime, T)>();
            _ttl = ttl;
        }

        public void Add(T obj)
        {
            _queue.Enqueue((DateTime.UtcNow, obj));
        }

        public IEnumerable<T> GetItems()
        {
            var now = DateTime.UtcNow;

            while (_queue.Count > 0)
            {
                if (_queue.TryPeek(out var item))
                {
                    if (now - item.time >= _ttl)
                    {
                        _queue.TryDequeue(out _);
                    }
                    else
                    {
                        break;
                    }
                }
            }

            return _queue.Select(x => x.obj)
                .ToList().AsReadOnly();
        }

        public void Clear()
        {
            _queue.Clear();
        }
    }
}
