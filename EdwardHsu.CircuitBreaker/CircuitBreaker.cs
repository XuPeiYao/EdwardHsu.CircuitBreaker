using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdwardHsu.CircuitBreaker
{
    /// <summary>
    /// Circuit breaker.
    /// </summary>
    public class CircuitBreaker: ICircuitBreaker, IDisposable, IAsyncDisposable
    {
        private IFuse _fuse;
        private CircuitBreakerStatus _status;

        /// <summary>
        /// Initializes a new instance of the <see cref="CircuitBreaker"/> class.
        /// </summary>
        /// <param name="fuse">Fuse.</param>
        public CircuitBreaker(IFuse fuse)
        {
            _status = CircuitBreakerStatus.On;
            _fuse = fuse;
            _fuse.StatusChanged += OnFuseStatusChanged;
        }
        
        /// <summary>
        /// Gets the fuse.
        /// </summary>
        public IFuse Fuse => _fuse;

        /// <summary>
        /// Gets the status.
        /// </summary>
        public CircuitBreakerStatus Status
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
        public event Action<ICircuitBreaker>? StatusChanged;

        private void OnFuseStatusChanged(IFuse fuse)
        {
            Status = fuse.Status switch
            {
                FuseStatus.Normal => CircuitBreakerStatus.On,
                FuseStatus.Tripped => CircuitBreakerStatus.TrippedOff,
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        /// <summary>
        /// Try to pass the circuit breaker.
        /// </summary>
        /// <param name="arguments"></param>
        /// <exception cref="InvalidOperationException"></exception>
        public void Execute(object[] arguments)
        {
            if (Status == CircuitBreakerStatus.Off)
            {
                throw new InvalidOperationException("CircuitBreaker is off.");
            } 
            else if (Status == CircuitBreakerStatus.TrippedOff)
            {
                throw new InvalidOperationException("CircuitBreaker is tripped off.");
            }

            if (_fuse.TryPass(arguments) == false)
            {
                throw new InvalidOperationException("CircuitBreaker is tripped off.");
            }
        }

        /// <summary>
        /// Turn on the circuit breaker.
        /// </summary>
        public void On()
        {
            // If the circuit breaker is already on, do nothing
            if (Status == CircuitBreakerStatus.On)
            {
                return;
            }

            _fuse.Reset();
            Status = CircuitBreakerStatus.On;
        }

        /// <summary>
        /// Turn off the circuit breaker.
        /// </summary>
        public void Off()
        {
            Status = CircuitBreakerStatus.Off;
        }

        /// <summary>
        /// Dispose the circuit breaker.
        /// </summary>
        public void Dispose()
        {
            if (_fuse is IDisposable disposable)
            {
                disposable.Dispose();
            }

            _fuse = null;
        }

        /// <summary>
        /// Dispose the circuit breaker.
        /// </summary>
        /// <returns></returns>
        public async ValueTask DisposeAsync()
        {
            if (_fuse is IAsyncDisposable asyncDisposable)
            {
                await asyncDisposable.DisposeAsync();
            }
            else if (_fuse is IDisposable disposable)
            {
                disposable.Dispose();
            }

            _fuse = null;
        }
    }
}
