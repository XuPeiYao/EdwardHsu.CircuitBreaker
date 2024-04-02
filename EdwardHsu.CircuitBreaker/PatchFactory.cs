using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EdwardHsu.CircuitBreaker
{
    internal class PatchFactory
    {
        private static List<(ICircuitBreaker breaker, object monitorObj, MethodInfo monitorMethod)> _patchTargets = new List<(ICircuitBreaker breaker, object monitorObj, MethodInfo monitorMethod)>();

        public static void Register(ICircuitBreaker breaker, object patchTarget, MethodInfo monitorMethod)
        {
            lock (_patchTargets)
            {
#if !DEBUG
                if (_patchTargets.Any(x => x.breaker == breaker))
                {
                    throw new InvalidOperationException("Breaker already registered");
                }
#endif

                _patchTargets.Add((breaker, patchTarget, monitorMethod));
            }
        }

        public static void Unregister(ICircuitBreaker breaker)
        {
            lock (_patchTargets)
            {
#if !DEBUG
                if (_patchTargets.Any(x => x.breaker == breaker) == false)
                {
                    throw new InvalidOperationException("Breaker not registered");
                }
#endif

                var target = _patchTargets.FirstOrDefault(x => x.breaker == breaker);

                _patchTargets.Remove(target);
            }
        }

        public static bool Prefix(object __instance, MethodInfo __originalMethod, object[] __args)
        {
            var breakerMap = _patchTargets.Where(x => x.monitorObj == __instance && x.monitorMethod == __originalMethod).ToList();

            if (breakerMap.Any() == false)
            {
                return true;
            }

            foreach (var breakerPair in breakerMap)
            {
                breakerPair.breaker.Fuse.Invoke(__args);
            }

            return true;
        }


    }
}
