using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EdwardHsu.CircuitBreaker.Internal;

namespace EdwardHsu.CircuitBreaker.Fuses
{
    /// <summary>
    /// Limits the number of executions within a sliding time window at any given time
    /// </summary>
    public class TimeSlidingWindowCountFuse : IFuse
    {
        private FuseStatus _status;
        private readonly int _threshold;
        private TtlBuffer<byte> _buffer;

        /// <summary>
        /// Constructor for TimeSlidingWindowCountFuse
        /// </summary>
        /// <param name="threshold">Limit of executions</param>
        /// <param name="duration">Time window</param>
        public TimeSlidingWindowCountFuse(int threshold, TimeSpan duration)
        {
            Status = FuseStatus.Normal;

            _threshold = threshold;

            _buffer = new TtlBuffer<byte>(duration);
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

            _buffer.Add(0);

            var count = _buffer.GetItems().Count();

            if (count >= _threshold)
            {
                Status = FuseStatus.Tripped;

                _buffer.Clear();

                throw new InvalidOperationException($"Fuse is tripped");
            }
        }

        /// <summary>
        /// Trip the fuse
        /// </summary>
        public void Trip()
        {
            _buffer.Clear();

            Status = FuseStatus.ManuallyTripped;
        }

        /// <summary>
        /// Reset the fuse
        /// </summary>
        public void Reset()
        {
            _buffer.Clear();
            Status = FuseStatus.Normal;
        }
    }
}
