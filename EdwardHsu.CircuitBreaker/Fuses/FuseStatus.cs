using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdwardHsu.CircuitBreaker.Fuses
{
    /// <summary>
    /// Fuse status
    /// </summary>
    public enum FuseStatus
    {
        /// <summary>
        /// Initial status
        /// </summary>
        Initial,

        /// <summary>
        /// Normal status
        /// </summary>
        Normal,

        /// <summary>
        /// Manually tripped
        /// </summary>
        ManuallyTripped,

        /// <summary>
        /// Tripped
        /// </summary>
        Tripped
    }
}
