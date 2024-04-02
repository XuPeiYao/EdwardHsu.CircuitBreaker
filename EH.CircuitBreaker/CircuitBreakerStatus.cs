using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EH.CircuitBreaker
{
    public enum CircuitBreakerStatus
    {
        On,
        TrippedOff,
        Off
    }
}
