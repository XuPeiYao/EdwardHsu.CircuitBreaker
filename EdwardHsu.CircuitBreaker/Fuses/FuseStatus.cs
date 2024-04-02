using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdwardHsu.CircuitBreaker.Fuses
{
    public enum FuseStatus
    {
        Initial,
        Normal,
        ManuallyTripped,
        Tripped
    }
}
