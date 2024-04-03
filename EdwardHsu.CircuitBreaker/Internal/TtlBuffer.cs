using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdwardHsu.CircuitBreaker.Internal
{
    public class TtlBuffer<T>
    {
        private TimeSpan _ttl;

        private ConcurrentBag<(DateTime time, T obj)> _list = new ConcurrentBag<(DateTime time, T obj)>();
        
        public TtlBuffer(TimeSpan ttl)
        {
            _list = new ConcurrentBag<(DateTime, T)>();
            _ttl = ttl;
        }

        public void Add(T obj)
        {
            _list.Add((DateTime.UtcNow, obj));
        }

        public IEnumerable<T> GetItems()
        {
            var now = DateTime.UtcNow;
            var result = _list.Where(x => now - x.time < _ttl).Select(x => x.obj);

            while(_list.Count > 0)
            {
                _list.TryPeek(out var item);

                if (now - item.time >= _ttl)
                {
                    _list.TryTake(out _);
                }
                else
                {
                    break;
                }
            }

            return result.ToList().AsReadOnly();
        }

        public void Clear()
        {
            _list = new ConcurrentBag<(DateTime, T)>();
        }
    }
}
