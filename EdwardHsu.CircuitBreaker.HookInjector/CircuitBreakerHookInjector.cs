using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using HarmonyLib;

namespace EdwardHsu.CircuitBreaker.HookInjector
{
    /// <summary>
    /// Circuit breaker hook injector.
    /// This static class will allow circuit breakers to act on existing methods without modifying the code.
    /// </summary>
    public static class CircuitBreakerHookInjector
    {
        private static object _lock = new object();
        private static Harmony _harmony = new Harmony("EdwardHsu.CircuitBreaker.HookInjector");
        private static ConcurrentDictionary<MethodInfo, MethodInfo> _harmonyPatchedMethods = new ConcurrentDictionary<MethodInfo, MethodInfo>();
        private static List<CircuitBreakerInjectedHook> _injectedHooks = new List<CircuitBreakerInjectedHook>();

        /// <summary>
        /// Get all injected methods.
        /// </summary>
        /// <param name="breaker">Circuit breaker.</param>
        /// <param name="method">Method.</param>
        /// <param name="instance">Instance. If static method then instance is null.</param>
        /// <returns>Injected methods</returns>
        public static IEnumerable<CircuitBreakerInjectedHook> GetInjectedMethods(this ICircuitBreaker breaker, MethodInfo method, object instance = null)
        {
            lock (_lock)
            {
                return _injectedHooks.Where(hook =>
                        hook.Breaker == breaker &&
                        hook.Method == method &&
                        hook.Instance == instance)
                    .ToList().AsReadOnly(); 
            }
        }

        /// <summary>
        /// Check if the method has been injected.
        /// </summary>
        /// <param name="breaker">Circuit breaker.</param>
        /// <param name="method">Method.</param>
        /// <param name="instance">Instance. If static method then instance is null.</param>
        /// <returns>Is injected</returns>
        public static bool IsInjected(this ICircuitBreaker breaker, MethodInfo method, object instance = null)
        {
            return breaker.GetInjectedMethods(method, instance).Any();
        }

        /// <summary>
        /// Inject the method.
        /// </summary>
        /// <param name="breaker">Circuit breaker.</param>
        /// <param name="method">Method.</param>
        /// <param name="instance">Instance. If static method then instance is null.</param>
        /// <exception cref="InvalidOperationException"></exception>
        public static void Inject(this ICircuitBreaker breaker, MethodInfo method, object instance = null)
        {
            lock (_lock)
            {
                if (_injectedHooks.Any(hook =>
                        hook.Breaker == breaker && hook.Method == method && hook.Instance == instance))
                {
                    throw new InvalidOperationException("The method has already been injected.");
                }

                _injectedHooks.Add(new CircuitBreakerInjectedHook()
                {
                    Breaker = breaker,
                    Method = method,
                    Instance = instance
                });

                HarmonyPatch(method);
            } 
        }

        /// <summary>
        /// Uninject the method.
        /// </summary>
        /// <param name="breaker">Circuit breaker.</param>
        /// <param name="method">Method.</param>
        /// <param name="instance">Instance. If static method then instance is null.</param>
        public static void Uninject(this ICircuitBreaker breaker, MethodInfo method, object instance = null)
        {
            lock (_lock)
            {
                var hook = _injectedHooks.FirstOrDefault(hook =>
                                       hook.Breaker == breaker && hook.Method == method && hook.Instance == instance);
                if (hook != null)
                {
                    _injectedHooks.Remove(hook);

                    if (_injectedHooks.Any(h => h.Method == method) == false)
                    {
                        HarmonyUnpatch(method);
                    }
                }
            }
        }

        /// <summary>
        /// Check if the method has been injected.
        /// </summary>
        /// <param name="breaker">Circuit breaker.</param>
        /// <param name="selector">Using Expression to pick a Method.</param>
        /// <returns>Is injected.</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static bool IsInjected(this ICircuitBreaker breaker, Expression<Action> selector)
        {
            if (selector.Body is MethodCallExpression mce)
            {
                var (instance, method) = ParseExpression(mce);
                return breaker.IsInjected(method, instance);
            }
            else if (selector.Body is UnaryExpression ue)
            {
                var (instance, method) = ParseExpression(ue.Operand as MethodCallExpression);
                return breaker.IsInjected(method, instance);
            }
            else
            {
                throw new ArgumentOutOfRangeException($"Invalid expression type {selector.Body.NodeType}");
            }
        }

        /// <summary>
        /// Check if the method has been injected.
        /// </summary>
        /// <param name="breaker">Circuit breaker.</param>
        /// <param name="selector">Using Expression to pick a Method.</param>
        /// <returns>Is injected.</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static bool IsInjected(this ICircuitBreaker breaker, Expression<Func<object>> selector)
        {
            if (selector.Body is MethodCallExpression mce)
            {
                var (instance, method) = ParseExpression(mce);
                return breaker.IsInjected(method, instance);
            }
            else if (selector.Body is UnaryExpression ue)
            {
                var (instance, method) = ParseExpression(ue.Operand as MethodCallExpression);
                return breaker.IsInjected(method, instance);
            }
            else
            {
                throw new ArgumentOutOfRangeException($"Invalid expression type {selector.Body.NodeType}");
            }
        }

        /// <summary>
        /// Get all injected methods of breaker.
        /// </summary>
        /// <param name="breaker">Circuit breaker.</param>
        /// <param name="selector">Using Expression to pick a Method.</param>
        /// <returns>Injected methods.</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static IEnumerable<CircuitBreakerInjectedHook> GetInjectedMethods(this ICircuitBreaker breaker, Expression<Action> selector)
        {
            if (selector.Body is MethodCallExpression mce)
            {
                var (instance, method) = ParseExpression(mce);
                return breaker.GetInjectedMethods(method, instance);
            }
            else if (selector.Body is UnaryExpression ue)
            {
                var (instance, method) = ParseExpression(ue.Operand as MethodCallExpression);
                return breaker.GetInjectedMethods(method, instance);
            }
            else
            {
                throw new ArgumentOutOfRangeException($"Invalid expression type {selector.Body.NodeType}");
            }
        }

        /// <summary>
        /// Get all injected methods of breaker.
        /// </summary>
        /// <param name="breaker">Circuit breaker.</param>
        /// <param name="selector">Using Expression to pick a Method.</param>
        /// <returns>Injected methods.</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static IEnumerable<CircuitBreakerInjectedHook> GetInjectedMethods(this ICircuitBreaker breaker, Expression<Func<object>> selector)
        {
            if (selector.Body is MethodCallExpression mce)
            {
                var (instance, method) = ParseExpression(mce);
                return breaker.GetInjectedMethods(method, instance);
            }
            else if (selector.Body is UnaryExpression ue)
            {
                var (instance, method) = ParseExpression(ue.Operand as MethodCallExpression);
                return breaker.GetInjectedMethods(method, instance);
            }
            else
            {
                throw new ArgumentOutOfRangeException($"Invalid expression type {selector.Body.NodeType}");
            }
        }

        /// <summary>
        /// Inject the method.
        /// </summary>
        /// <param name="breaker">Circuit breaker.</param>
        /// <param name="selector">Using Expression to pick a Method.</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static void Inject(this ICircuitBreaker breaker, Expression<Action> selector)
        {
            if (selector.Body is MethodCallExpression mce)
            {
                var (instance, method) = ParseExpression(mce);
                breaker.Inject(method, instance);
            }
            else if (selector.Body is UnaryExpression ue)
            {
                var (instance, method) = ParseExpression(ue.Operand as MethodCallExpression);
                breaker.Inject(method, instance);
            }
            else
            {
                throw new ArgumentOutOfRangeException($"Invalid expression type {selector.Body.NodeType}");
            }
        }

        /// <summary>
        /// Inject the method.
        /// </summary>
        /// <param name="breaker">Circuit breaker.</param>
        /// <param name="selector">Using Expression to pick a Method.</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static void Inject(this ICircuitBreaker breaker, Expression<Func<object>> selector)
        {
            if (selector.Body is MethodCallExpression mce)
            {
                var (instance, method) = ParseExpression(mce);
                breaker.Inject(method, instance);
            }
            else if (selector.Body is UnaryExpression ue)
            {
                if (ue.Operand is MethodCallExpression mce2)
                {
                    var (instance, method) = ParseExpression(mce2);
                    breaker.Inject(method, instance);
                }
                else
                {
                    throw new ArgumentOutOfRangeException($"Invalid expression type {ue.Operand.NodeType}");
                }
            }
            else
            {
                throw new ArgumentOutOfRangeException($"Invalid expression type {selector.Body.NodeType}");
            }
        }

        /// <summary>
        /// Uninject the method.
        /// </summary>
        /// <param name="breaker">Circuit breaker.</param>
        /// <param name="selector">Using Expression to pick a Method.</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static void Uninject(this ICircuitBreaker breaker, Expression<Action> selector)
        {
            if (selector.Body is MethodCallExpression mce)
            {
                var (instance, method) = ParseExpression(mce);
                breaker.Uninject(method, instance);
            }
            else if (selector.Body is UnaryExpression ue)
            {
                if (ue.Operand is MethodCallExpression mce2)
                {
                    var (instance, method) = ParseExpression(mce2);
                    breaker.Inject(method, instance);
                }
                else
                {
                    throw new ArgumentOutOfRangeException($"Invalid expression type {ue.Operand.NodeType}");
                }
            }
            else
            {
                throw new ArgumentOutOfRangeException($"Invalid expression type {selector.Body.NodeType}");
            }
        }

        /// <summary>
        /// Uninject the method.
        /// </summary>
        /// <param name="breaker">Circuit breaker.</param>
        /// <param name="selector">Using Expression to pick a Method.</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static void Uninject(this ICircuitBreaker breaker, Expression<Func<object>> selector)
        {
            if (selector.Body is MethodCallExpression mce)
            {
                var (instance, method) = ParseExpression(mce);
                breaker.Uninject(method, instance);
            }
            else if (selector.Body is UnaryExpression ue)
            {
                var (instance, method) = ParseExpression(ue.Operand as MethodCallExpression);
                breaker.Uninject(method, instance);
            }
            else
            {
                throw new ArgumentOutOfRangeException($"Invalid expression type {selector.Body.NodeType}");
            }
        }



        private static (object instance, MethodInfo method) ParseExpression(MethodCallExpression mce)
        {
            object instance = null;
            var method = mce.Method;

            if (mce.Object is MemberExpression me)
            {
                var variableMemberInfo = me.Member;

                var fieldInfo = variableMemberInfo as FieldInfo;

                if (fieldInfo != null && me.Expression is ConstantExpression ce)
                {
                    instance = fieldInfo.GetValue(ce.Value);
                }
                else
                {
                    throw new ArgumentOutOfRangeException($"Invalid expression type {mce.Object.NodeType}");
                }
            }
            else if (mce.Object is ConstantExpression ce)
            {
                instance = ce.Value;
            }
            else if (mce.Object is null) // static method
            {
                instance = null;
            }
            else
            {
                throw new ArgumentOutOfRangeException($"Invalid expression type {mce.Object.NodeType}");
            }

            return (instance, method);
        }

        private static void HarmonyPatch(MethodInfo method)
        {
            lock (_harmony)
            {
                var _allHarmonyPatchedMethods = _harmony.GetPatchedMethods();

                if (_allHarmonyPatchedMethods.Any(m => m == method))
                {
                    return;
                }

                var prefixMethodInfo = typeof(CircuitBreakerHookInjector).GetMethod(nameof(Prefix), BindingFlags.Static | BindingFlags.NonPublic);

                var harmonyPatchedMethod = _harmony.Patch(method, new HarmonyMethod(prefixMethodInfo));

                _harmonyPatchedMethods.TryAdd(method, harmonyPatchedMethod);
            }
        }

        private static void HarmonyUnpatch(MethodInfo method)
        {
            lock (_harmony)
            {
                var _allHarmonyPatchedMethods = _harmony.GetPatchedMethods();

                if (_allHarmonyPatchedMethods.Any(m => m == method) == false)
                {
                    return;
                }

                if (_harmonyPatchedMethods.TryGetValue(method, out var harmonyPatchedMethod))
                {
                    _harmony.Unpatch(method, harmonyPatchedMethod);
                    _harmonyPatchedMethods.TryRemove(method, out _);
                }
            }
        }

        private static bool Prefix(object __instance, MethodInfo __originalMethod, object[] __args)
        {
            var breakers = _injectedHooks
                .Where(hook => hook.Method == __originalMethod && hook.Instance == __instance)
                .ToList().AsReadOnly();

            if (breakers.Any() == false)
            {
                return true;
            }

            foreach (var hook in breakers)
            {
                hook.Breaker.Execute(__args);
            }

            return true;
        }
    }
}
