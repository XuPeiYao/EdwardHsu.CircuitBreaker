using HarmonyLib;

using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Metadata;
using System.Security.Cryptography.X509Certificates;
using EdwardHsu.CircuitBreaker.Fuses;
using EdwardHsu.CircuitBreaker.Internal;

namespace EdwardHsu.CircuitBreaker
{
    /// <summary>
    /// Circuit breaker
    /// </summary>
    public class CircuitBreaker : ICircuitBreaker, IDisposable , IAsyncDisposable
    {
        private readonly IFuse _fuse; 
        private CircuitBreakerStatus _status;
        private object _monitorObject;
        private MethodInfo _monitorMethod;
        private Harmony _harmony;

        /// <summary>
        /// Constructor for CircuitBreaker
        /// </summary>
        /// <param name="fuse">Breaker fuse, which is the circuit breaker's internal state</param>
        /// <param name="monitorMethodSelector">Expression selector for method to monitor</param>
        /// <param name="initialStatus">Default status of the circuit breaker</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public CircuitBreaker(
            IFuse fuse,
            Expression<Action> monitorMethodSelector,
            CircuitBreakerStatus initialStatus = CircuitBreakerStatus.On)
        {
            _fuse = fuse;
            InitialFuseHook(initialStatus);

            if (monitorMethodSelector.Body is MethodCallExpression mce)
            {
                MonitorObjectAndMethod(mce);
            }
            else
            {
                throw new ArgumentOutOfRangeException($"Invalid expression type {monitorMethodSelector.Body.NodeType}");
            }
        }

        /// <summary>
        /// Constructor for CircuitBreaker
        /// </summary>
        /// <param name="fuse">Breaker fuse, which is the circuit breaker's internal state</param>
        /// <param name="monitorMethodSelector">Expression selector for method to monitor</param>
        /// <param name="initialStatus">Default status of the circuit breaker</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public CircuitBreaker(
            IFuse fuse,
            Expression<Func<object>> monitorMethodSelector,
            CircuitBreakerStatus initialStatus = CircuitBreakerStatus.On)
        {
            _fuse = fuse;
            InitialFuseHook(initialStatus);

            if (monitorMethodSelector.Body is MethodCallExpression mce)
            {
                MonitorObjectAndMethod(mce);
            }
            else
            {
                throw new ArgumentOutOfRangeException($"Invalid expression type {monitorMethodSelector.Body.NodeType}");
            }
        }

        private void InitialFuseHook(CircuitBreakerStatus initialStatus)
        {
            _fuse.StatusChanged += (f) =>
            {
                if (f.Status == FuseStatus.Tripped)
                {
                    _status = CircuitBreakerStatus.TrippedOff;
                }
                else if (f.Status == FuseStatus.ManuallyTripped)
                {
                    _status = CircuitBreakerStatus.Off;
                }
                else if (f.Status == FuseStatus.Initial ||
                         f.Status == FuseStatus.Normal)
                {
                    _status = CircuitBreakerStatus.On;
                }
                
                StatusChanged?.Invoke(this);
            };

            if (_status == CircuitBreakerStatus.On)
            {
                _fuse.Reset();
            }
            else
            {
                _fuse.Trip();
            }
        }

        private void MonitorObjectAndMethod(MethodCallExpression mce)
        {
            _monitorMethod = mce.Method;

            if (mce.Object is MemberExpression me)
            {
                var variableMemberInfo = me.Member;

                var fieldInfo = variableMemberInfo as FieldInfo;

                if (fieldInfo != null && me.Expression is ConstantExpression ce )
                {
                    _monitorObject = fieldInfo.GetValue(ce.Value);
                }
                else
                {
                    throw new ArgumentOutOfRangeException($"Invalid expression type {mce.Object.NodeType}");
                }
            }
            else if(mce.Object is ConstantExpression ce)
            {
                _monitorObject = ce.Value;
            }
            else if(mce.Object is null) // static method
            {
                _monitorObject = null;
            }
            else
            {
                throw new ArgumentOutOfRangeException($"Invalid expression type {mce.Object.NodeType}");
            }

            PatchFactory.Register(this, _monitorObject, _monitorMethod);

            _harmony = new Harmony($"EdwardHsu.CircuitBreaker.{this.GetHashCode()}");

            var prefix = typeof(PatchFactory).GetMethod(nameof(PatchFactory.Prefix), BindingFlags.Static | BindingFlags.Public);

            var patchMethod = new HarmonyMethod(prefix);

            _harmony.Patch(_monitorMethod, patchMethod);
        }
        
        /// <summary>
        /// Circuit breaker status
        /// </summary>
        public CircuitBreakerStatus Status => _status;
        
        /// <summary>
        /// Circuit breaker fuse
        /// </summary>
        public IFuse Fuse => _fuse;


        /// <summary>
        /// Event for status changed
        /// </summary>
        public event Action<ICircuitBreaker> StatusChanged;

        /// <summary>
        /// Turn on the circuit breaker
        /// </summary>
        public void On()
        {
            if (_status == CircuitBreakerStatus.On)
            {
                return;
            }

            _fuse.Reset();
        }

        /// <summary>
        /// Turn off the circuit breaker
        /// </summary>
        public void Off()
        {
            if (_status == CircuitBreakerStatus.Off ||
                _status == CircuitBreakerStatus.TrippedOff)
            {
                return;
            }

            _fuse.Trip();
        }

        /// <summary>
        /// Dispose the circuit breaker
        /// </summary>
        public void Dispose()
        {
            if (_harmony != null)
            {
                _harmony.UnpatchAll();
                _harmony = null;
            }

            PatchFactory.Unregister(this);
        }

        /// <summary>
        /// Dispose the circuit breaker
        /// </summary>
        /// <returns></returns>
        public async ValueTask DisposeAsync()
        {
            if (_harmony != null)
            {
                _harmony.UnpatchAll();
                _harmony = null;
            }

            PatchFactory.Unregister(this);
        }
    }
}
