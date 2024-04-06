using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdwardHsu.CircuitBreaker
{
    /// <summary>
    /// Fuse is used to define the triggering conditions of a circuit breaker.
    /// </summary>
    public interface IFuse
    {
        /// <summary>
        /// Gets the status.
        /// </summary>
        FuseStatus Status { get; }

        /// <summary>
        /// Occurs when status changed.
        /// </summary>
        event Action<IFuse> StatusChanged;

        /// <summary>
        /// Try to pass the fuse.
        /// </summary>
        /// <param name="arguments">arguments.</param>
        /// <returns>Is passed.</returns>
        bool TryPass(object[] arguments);

        /// <summary>
        /// Reset the fuse.
        /// </summary>
        void Reset();
    }
}
