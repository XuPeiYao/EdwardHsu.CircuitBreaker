using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EdwardHsu.CircuitBreaker.Fuses;

namespace EdwardHsu.CircuitBreaker
{
    /// <summary>
    /// Fuse interface
    /// </summary>
    public interface IFuse
    {
        /// <summary>
        /// Fuse status
        /// </summary>
        FuseStatus Status { get; }

        /// <summary>
        /// Event for status changed
        /// </summary>
        event Action<IFuse> StatusChanged;

        /// <summary>
        /// Invoke the fuse
        /// </summary>
        /// <param name="arguments">arguments</param>
        void Invoke(object[] arguments);

        /// <summary>
        /// Trip the fuse
        /// </summary>
        void Trip();

        /// <summary>
        /// Reset the fuse
        /// </summary>
        void Reset();
    }


}
