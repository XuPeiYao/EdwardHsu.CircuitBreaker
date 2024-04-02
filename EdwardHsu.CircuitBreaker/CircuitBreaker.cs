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
    public class CircuitBreaker : ICircuitBreaker, IDisposable , IAsyncDisposable
    {

        private readonly IFuse _fuse; 
        private CircuitBreakerStatus _status;
        private object _monitorObject;
        private MethodInfo _monitorMethod;
        private Harmony _harmony;

        public CircuitBreaker(
            IFuse fuse,
            Expression<Action> monitorMethodSelector,
            CircuitBreakerStatus defaultStatus = CircuitBreakerStatus.On)
        {
            _fuse = fuse;
            InitialFuseHook(defaultStatus);

            if (monitorMethodSelector.Body is MethodCallExpression mce)
            {
                MonitorObjectAndMethod(mce);
            }
            else
            {
                throw new ArgumentOutOfRangeException($"Invalid expression type {monitorMethodSelector.Body.NodeType}");
            }
        }

        public CircuitBreaker(
            IFuse fuse,
            Expression<Func<object>> monitorMethodSelector,
            CircuitBreakerStatus defaultStatus = CircuitBreakerStatus.On)
        {
            _fuse = fuse;
            InitialFuseHook(defaultStatus);

            if (monitorMethodSelector.Body is MethodCallExpression mce)
            {
                MonitorObjectAndMethod(mce);
            }
            else
            {
                throw new ArgumentOutOfRangeException($"Invalid expression type {monitorMethodSelector.Body.NodeType}");
            }
        }

        private void InitialFuseHook(CircuitBreakerStatus defaultStatus)
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
        
        public CircuitBreakerStatus Status => _status;

        public IFuse Fuse => _fuse;

        public event Action<ICircuitBreaker> StatusChanged;

        public void On()
        {
            if (_status == CircuitBreakerStatus.On)
            {
                return;
            }

            _fuse.Reset();
        }

        public void Off()
        {
            if (_status == CircuitBreakerStatus.Off ||
                _status == CircuitBreakerStatus.TrippedOff)
            {
                return;
            }

            _fuse.Trip();
        }

        public void Dispose()
        {
            if (_harmony != null)
            {
                _harmony.UnpatchAll();
                _harmony = null;
            }

            PatchFactory.Unregister(this);
        }

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
