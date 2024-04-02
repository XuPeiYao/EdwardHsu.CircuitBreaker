using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdwardHsu.CircuitBreaker.Fuses
{
    public class TimeWindowCountFuse: IFuse, IDisposable, IAsyncDisposable
    {
        private FuseStatus _status;
        private readonly int _threshold;
        private readonly TimeSpan _duration;
        private readonly Timer _timer;
        private long _count;

        public TimeWindowCountFuse(int threshold, TimeSpan duration)
        {
            _status = FuseStatus.Initial;

            _threshold = threshold;
            _duration = duration;

            _timer = new Timer((state) =>
            {
                CheckThreshold(state, true);
            }, null, Timeout.Infinite, Timeout.Infinite);
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

        private void CheckThreshold(object? state, bool reset = true)
        {
            lock (this)
            {
                if (_count >= _threshold)
                {
                    Status = FuseStatus.Tripped;

                    _timer.Change(Timeout.Infinite, Timeout.Infinite);
                }

                if (reset)
                {
                    _count = 0;
                }
            }
        }


        public void Invoke(object[] arguments)
        {
            if (Status == FuseStatus.Tripped || 
                Status == FuseStatus.ManuallyTripped)
            {
                throw new InvalidOperationException($"Fuse is tripped");
            }
            else if (Status == FuseStatus.Initial)
            {
                Status = FuseStatus.Normal;
                _timer.Change(_duration, _duration);
            }
            lock (this)
            {
                _count++;
            }

            CheckThreshold(this, false);
        }

        public void Trip()
        {
            lock (this)
            {
                _count = 0;
            }

            Status = FuseStatus.ManuallyTripped;
            _timer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        public void Reset()
        {
            lock (this)
            {
                _count = 0;
            }

            Status = FuseStatus.Initial;
            _timer.Change(Timeout.Infinite, Timeout.Infinite);
        }


        public void Dispose()
        {
            _timer.Dispose();
        }

        public async ValueTask DisposeAsync()
        {
            await _timer.DisposeAsync();
        }
    }
}
