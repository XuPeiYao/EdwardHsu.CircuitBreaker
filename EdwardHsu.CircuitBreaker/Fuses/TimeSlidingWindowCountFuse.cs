using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EdwardHsu.CircuitBreaker.Internal;

namespace EdwardHsu.CircuitBreaker.Fuses
{
    public class TimeSlidingWindowCountFuse : IFuse
    {
        private FuseStatus _status;
        private readonly int _threshold;
        private TtlBuffer<byte> _buffer;


        public TimeSlidingWindowCountFuse(int threshold, TimeSpan duration)
        {
            Status = FuseStatus.Normal;

            _threshold = threshold;

            _buffer = new TtlBuffer<byte>(duration);
        }

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

        public event Action<IFuse>? StatusChanged;

        

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
            }
        }

        public void Trip()
        {
            _buffer.Clear();

            Status = FuseStatus.ManuallyTripped;
        }

        public void Reset()
        {
            _buffer.Clear();
            Status = FuseStatus.Normal;
        }
    }
}
