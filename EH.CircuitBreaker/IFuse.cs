using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EH.CircuitBreaker.Fuses;

namespace EH.CircuitBreaker
{
    public interface IFuse
    {
        FuseStatus Status { get; }

        event Action<IFuse> StatusChanged;

        void Invoke(object[] arguments);

        void Trip();

        void Reset();
    }


}
