﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdwardHsu.CircuitBreaker
{
    /// <summary>
    /// Circuit breaker status.
    /// </summary>
    public enum CircuitBreakerStatus
    {
        /// <summary>
        /// The circuit breaker is on.
        /// </summary>
        On,

        /// <summary>
        /// The circuit breaker is tripped off.
        /// </summary>
        TrippedOff,

        /// <summary>
        /// The circuit breaker is off.
        /// </summary>
        Off
    }
}
