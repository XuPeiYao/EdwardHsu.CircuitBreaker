using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdwardHsu.CircuitBreaker.Tests.Models
{
    public class DisposableFuse : IFuse, IDisposable
    {
        public void Dispose()
        {
        }

        public FuseStatus Status { get; }
        public event Action<IFuse>? StatusChanged;
        public bool TryPass(object[] arguments)
        {
            throw new NotImplementedException();
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }
    }
}
