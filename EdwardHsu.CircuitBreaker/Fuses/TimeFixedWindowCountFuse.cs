using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EdwardHsu.CircuitBreaker.Internal;

namespace EdwardHsu.CircuitBreaker.Fuses
{
    /// <summary>
    /// Limits the number of executions within a fixed time window
    /// </summary>
    public class TimeFixedWindowCountFuse : IFuse
    {
        private FuseStatus _status;
        private readonly int _threshold;
        private readonly TimeSpan _duration;
        private DateTime _currentWindowStartTime;
        private ConcurrentQueue<DateTime> _queue;

        /// <summary>
        /// Constructor for TimeFixedWindowCountFuse
        /// </summary>
        /// <param name="threshold">Limit of executions</param>
        /// <param name="duration">Time window</param>
        /// <param name="firstWindowStartTime">Start time of the first window</param>
        public TimeFixedWindowCountFuse(int threshold, TimeSpan duration, DateTime? firstWindowStartTime = null)
        {
            Status = FuseStatus.Normal;

            _duration = duration;

            _threshold = threshold;

            _currentWindowStartTime = firstWindowStartTime ?? DateTime.UtcNow;

            _queue = new ConcurrentQueue<DateTime>();
        }
        
        /// <summary>
        /// Fuse status
        /// </summary>
        public FuseStatus Status
        {
            get
            {
                return _status;
            }
            private set
            {
                if (_status != value)
                {
                    _status = value;
                    StatusChanged?.Invoke(this);
                }
            }
        }

        /// <summary>
        /// Event for status changed
        /// </summary>
        public event Action<IFuse>? StatusChanged;

        /// <summary>
        /// Invoke the fuse
        /// </summary>
        /// <param name="arguments">arguments</param>
        /// <exception cref="InvalidOperationException"></exception>
        public void Invoke(object[] arguments)
        {
            if (Status == FuseStatus.Tripped || 
                Status == FuseStatus.ManuallyTripped)
            {
                throw new InvalidOperationException($"Fuse is tripped");
            }

            var now = DateTime.UtcNow;

            _queue.Enqueue(now);

            if (now - _currentWindowStartTime >= _duration)
            {
                _currentWindowStartTime = _currentWindowStartTime + _duration;
            }

            while (_queue.Count > 0)
            {
                if (_queue.TryPeek(out var item))
                {
                    if (item < _currentWindowStartTime)
                    {
                        _queue.TryDequeue(out _);
                    }
                    else
                    {
                        break;
                    }
                }
            }

            if (_queue.Count >= _threshold)
            {
                Status = FuseStatus.Tripped;

                _queue.Clear();

                throw new InvalidOperationException($"Fuse is tripped");
            }
        }

        /// <summary>
        /// Trip the fuse
        /// </summary>
        public void Trip()
        {
            _queue.Clear();

            Status = FuseStatus.ManuallyTripped;
        }

        /// <summary>
        /// Reset the fuse
        /// </summary>
        public void Reset()
        {
            _queue.Clear();

            Status = FuseStatus.Normal;
        }
    }
}
