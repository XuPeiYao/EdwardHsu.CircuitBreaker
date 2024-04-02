using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace EdwardHsu.CircuitBreaker
{
    /// <summary>
    /// Circuit breaker interface
    /// </summary>
    public interface ICircuitBreaker
    {
        /// <summary>
        /// Circuit breaker fuse
        /// </summary>
        IFuse Fuse { get; }

        /// <summary>
        /// Circuit breaker status
        /// </summary>
        CircuitBreakerStatus Status { get; }

        /// <summary>
        /// Event for status changed
        /// </summary>
        event Action<ICircuitBreaker> StatusChanged;

        /// <summary>
        /// Turn on the circuit breaker
        /// </summary>
        void On();

        /// <summary>
        /// Turn off the circuit breaker
        /// </summary>
        void Off();
    }
}
