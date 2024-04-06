using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdwardHsu.CircuitBreaker
{
    /// <summary>
    /// Fuse status.
    /// </summary>
    public enum FuseStatus
    {
        /// <summary>
        /// The fuse is normal.
        /// </summary>
        Normal,

        /// <summary>
        /// The fuse is tripped.
        /// </summary>
        Tripped,
    }
}
