using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdwardHsu.CircuitBreaker
{
    /// <summary>
    /// Circuit breaker status
    /// </summary>
    public enum CircuitBreakerStatus
    {
        /// <summary>
        /// Circuit breaker is on
        /// </summary>
        On,
        /// <summary>
        /// Circuit breaker is tripped off
        /// </summary>
        TrippedOff,
        /// <summary>
        /// Circuit breaker is off
        /// </summary>
        Off
    }
}
