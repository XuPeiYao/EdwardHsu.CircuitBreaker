using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdwardHsu.CircuitBreaker.Fuses
{
    /// <summary>
    /// Time sliding window limit fuse.
    /// </summary>
    public class TimeSlidingWindowLimitFuse: IFuse
    {
        private readonly int _limit;
        private FuseStatus _status;
        private readonly TimeSpan _window;
        private readonly ConcurrentQueue<DateTime> _queue;

        /// <summary>
        /// Initializes a new instance of the <see cref="TimeSlidingWindowLimitFuse"/> class.
        /// </summary>
        /// <param name="limit">Limit.</param>
        /// <param name="window">Window.</param>
        public TimeSlidingWindowLimitFuse(int limit, TimeSpan window)
        {
            _limit = limit;
            _window = window;
            _status = FuseStatus.Normal;
            _queue = new ConcurrentQueue<DateTime>();
        }

        /// <summary>
        /// Gets the limit.
        /// </summary>
        public FuseStatus Status
        {
            get => _status;
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
        /// Occurs when status changed.
        /// </summary>
        public event Action<IFuse>? StatusChanged;

        /// <summary>
        /// Try to pass the fuse.
        /// </summary>
        /// <param name="arguments">arguments.</param>
        /// <returns>Is passed.</returns>
        public bool TryPass(object[] arguments)
        {
            if (Status == FuseStatus.Tripped)
            {
                return false;
            }

            while (_queue.TryPeek(out var time) && DateTime.Now - time > _window)
            {
                _queue.TryDequeue(out _);
            }

            _queue.Enqueue(DateTime.Now);

            if (_queue.Count >= _limit)
            {
                Status = FuseStatus.Tripped;
            }

            return true;
        }

        /// <summary>
        /// Reset the fuse.
        /// </summary>
        public void Reset()
        {
            _queue.Clear();
            Status = FuseStatus.Normal;
        }
    }
}
