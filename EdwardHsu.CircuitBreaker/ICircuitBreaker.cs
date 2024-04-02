using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace EdwardHsu.CircuitBreaker
{
    public interface ICircuitBreaker
    {
        IFuse Fuse { get; }

        CircuitBreakerStatus Status { get; }


        event Action<ICircuitBreaker> StatusChanged;

        void On();

        void Off();
    }
}
