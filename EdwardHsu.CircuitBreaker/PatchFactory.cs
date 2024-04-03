using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EdwardHsu.CircuitBreaker
{
    public class PatchFactory
    {
        private static ConcurrentDictionary<ICircuitBreaker, (object monitorObj, MethodInfo monitorMethod)> _patchTargets;

        static PatchFactory()
        {
            _patchTargets = new ConcurrentDictionary<ICircuitBreaker, (object monitorObj, MethodInfo monitorMethod)>();
        }

        public static void Register(ICircuitBreaker breaker, object monitorObj, MethodInfo monitorMethod)
        {
            lock (_patchTargets)
            {
#if !DEBUG
                if (_patchTargets.TryGetValue(breaker, out _))
                {
                    throw new InvalidOperationException("Breaker already registered");
                }
#endif

                if (_patchTargets.TryAdd(breaker, (monitorObj, monitorMethod)) == false)
                {
                    throw new InvalidOperationException("Failed to register breaker");
                }
            }
        }

        public static void Unregister(ICircuitBreaker breaker)
        {
            lock (_patchTargets)
            {
#if !DEBUG
                if (_patchTargets.TryGetValue(breaker, out _) == false)
                {
                    throw new InvalidOperationException("Breaker not registered");
                }
#endif

                _patchTargets.Remove(breaker, out _);
            }
        }

        public static bool Prefix(object __instance, MethodInfo __originalMethod, object[] __args)
        {
            var registedMonitors = _patchTargets.Where(x =>
                    x.Value.monitorObj == __instance && x.Value.monitorMethod == __originalMethod)
                .ToList();

            if (registedMonitors.Any() == false) return true;

            foreach (var breakerPair in registedMonitors)
            {
                breakerPair.Key.Fuse.Invoke(__args);
            }

            return true;
        }


    }
}
