using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdwardHsu.CircuitBreaker.Fuses
{
    /// <summary>
    /// Execution limit fuse.
    /// </summary>
    public class ExecutionLimitFuse: IFuse
    {
        private readonly int _limit;
        private FuseStatus _status;
        private int _count;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="ExecutionLimitFuse"/> class.
        /// </summary>
        /// <param name="limit">Limit.</param>
        public ExecutionLimitFuse(int limit)
        {
            _limit = limit;
            _status = FuseStatus.Normal;
        }

        /// <summary>
        /// Gets the limit.
        /// </summary>
        public int Limit => _limit;

        /// <summary>
        /// Number of times it has been executed.
        /// </summary>
        public int Count => _count;

        /// <summary>
        /// Gets the status.
        /// </summary>
        public FuseStatus Status
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
        public event Action<IFuse>? StatusChanged;

        /// <summary>
        /// Try to pass the fuse.
        /// </summary>
        /// <param name="arguments">arguments.</param>
        /// <returns>Is passed.</returns>
        public bool TryPass(object[] arguments)
        {
            if (Status == FuseStatus.Tripped)
            {
                return false;
            }

            _count++;

            if (_count >= _limit)
            {
                Status = FuseStatus.Tripped;
            }

            return true;
        }
        
        /// <summary>
        /// Reset the fuse.
        /// </summary>
        public void Reset()
        {
            _count = 0;
            Status = FuseStatus.Normal;
        }
    }
}
