using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EdwardHsu.CircuitBreaker.HookInjector
{
    public class CircuitBreakerInjectedHook
    {
        public ICircuitBreaker Breaker { get; internal set; }
        public MethodInfo Method { get; internal set; }
        public object Instance { get; internal set; }
    }
}
