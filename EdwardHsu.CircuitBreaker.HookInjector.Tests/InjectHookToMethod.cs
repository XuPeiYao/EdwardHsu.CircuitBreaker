using EdwardHsu.CircuitBreaker.Fuses;
using EdwardHsu.CircuitBreaker.HookInjector.Tests.Models;

namespace EdwardHsu.CircuitBreaker.HookInjector.Tests
{
    public class InjectHookToMethod
    {
        [Fact]
        public void InjectHookToMethodThenUninject_Action()
        {
            var instance1 = new ExampleClass2();
            var instance2 = new ExampleClass2();
            var fuse = new ExecutionLimitFuse(5);
            var breaker = new CircuitBreaker(fuse);

            breaker.Inject(()=> instance1.ExampleMethod1());

            Assert.True(breaker.IsInjected(() => instance1.ExampleMethod1()));
            Assert.False(breaker.IsInjected(() => instance2.ExampleMethod1()));

            breaker.Uninject(()=> instance1.ExampleMethod1());

            Assert.False(breaker.IsInjected(() => instance1.ExampleMethod1()));
        }

        [Fact]
        public void InjectHookToMethodThenUninject_Func()
        {
            var instance1 = new ExampleClass2();
            var instance2 = new ExampleClass2();
            var fuse = new ExecutionLimitFuse(5);
            var breaker = new CircuitBreaker(fuse);

            breaker.Inject(() => instance1.ExampleMethod2(Guid.Empty));

            Assert.True(breaker.IsInjected(() => instance1.ExampleMethod2(Guid.Empty)));
            Assert.False(breaker.IsInjected(() => instance2.ExampleMethod2(Guid.Empty)));

            breaker.Uninject(() => instance1.ExampleMethod2(Guid.Empty));

            Assert.False(breaker.IsInjected(() => instance1.ExampleMethod2(Guid.Empty)));
        }

        [Fact]
        public void InjectHookToMethod_Action()
        {
            var instance1 = new ExampleClass2();

            var fuse = new ExecutionLimitFuse(5);
            var breaker = new CircuitBreaker(fuse);

            breaker.Inject(() => instance1.ExampleMethod1());

            Assert.True(breaker.IsInjected(() => instance1.ExampleMethod1()));

            Assert.Equal(CircuitBreakerStatus.On, breaker.Status);

            for (int i = 0; i < 5; i++)
            {
                instance1.ExampleMethod1();
            }

            Assert.Equal(CircuitBreakerStatus.TrippedOff, breaker.Status);

            Assert.Throws<InvalidOperationException>(() =>
            {
                instance1.ExampleMethod1();
            });

            breaker.On();

            Assert.Equal(CircuitBreakerStatus.On, breaker.Status);
        }

        [Fact]
        public void InjectHookToMethod_Func()
        {
            var instance1 = new ExampleClass2();

            var fuse = new ExecutionLimitFuse(5);
            var breaker = new CircuitBreaker(fuse);

            breaker.Inject(() => instance1.ExampleMethod2(Guid.Empty));

            Assert.True(breaker.IsInjected(() => instance1.ExampleMethod2(Guid.Empty)));

            Assert.Equal(CircuitBreakerStatus.On, breaker.Status);

            for (int i = 0; i < 5; i++)
            {
                instance1.ExampleMethod2(Guid.Empty);
            }

            Assert.Equal(CircuitBreakerStatus.TrippedOff, breaker.Status);

            Assert.Throws<InvalidOperationException>(() =>
            {
                instance1.ExampleMethod2(Guid.Empty);
            });

            breaker.On();

            Assert.Equal(CircuitBreakerStatus.On, breaker.Status);
        }

        [Fact]
        public void InjectDiffHookToSameMethod_Action()
        {
            var instance1 = new ExampleClass2();
            var fuse1 = new ExecutionLimitFuse(5);
            var breaker1 = new CircuitBreaker(fuse1);

            var instance2 = new ExampleClass2();
            var fuse2 = new ExecutionLimitFuse(5);
            var breaker2 = new CircuitBreaker(fuse2);

            breaker1.Inject(() => instance1.ExampleMethod1());
            breaker2.Inject(() => instance1.ExampleMethod1());

            Assert.True(breaker1.IsInjected(() => instance1.ExampleMethod1()));
            Assert.True(breaker2.IsInjected(() => instance1.ExampleMethod1()));

            Assert.Equal(CircuitBreakerStatus.On, breaker1.Status);

            for (int i = 0; i < 5; i++)
            {
                instance1.ExampleMethod1();
            }

            Assert.Equal(CircuitBreakerStatus.TrippedOff, breaker1.Status);
            Assert.Equal(CircuitBreakerStatus.TrippedOff, breaker2.Status);

            Assert.Throws<InvalidOperationException>(() =>
            {
                instance1.ExampleMethod1();
            });

            breaker1.On();
            breaker2.On();

            Assert.Equal(CircuitBreakerStatus.On, breaker1.Status);
            Assert.Equal(CircuitBreakerStatus.On, breaker2.Status);
        }

        [Fact]
        public void InjectDiffHookToSameMethod_Func()
        {
            var instance1 = new ExampleClass2();
            var fuse1 = new ExecutionLimitFuse(5);
            var breaker1 = new CircuitBreaker(fuse1);

            var instance2 = new ExampleClass2();
            var fuse2 = new ExecutionLimitFuse(5);
            var breaker2 = new CircuitBreaker(fuse2);

            breaker1.Inject(() => instance1.ExampleMethod2(Guid.Empty));
            breaker2.Inject(() => instance1.ExampleMethod2(Guid.Empty));

            Assert.True(breaker1.IsInjected(() => instance1.ExampleMethod2(Guid.Empty)));
            Assert.True(breaker2.IsInjected(() => instance1.ExampleMethod2(Guid.Empty)));

            Assert.Equal(CircuitBreakerStatus.On, breaker1.Status);

            for (int i = 0; i < 5; i++)
            {
                instance1.ExampleMethod2(Guid.Empty);
            }

            Assert.Equal(CircuitBreakerStatus.TrippedOff, breaker1.Status);
            Assert.Equal(CircuitBreakerStatus.TrippedOff, breaker2.Status);

            Assert.Throws<InvalidOperationException>(() =>
            {
                instance1.ExampleMethod2(Guid.Empty);
            });

            breaker1.On();
            breaker2.On();

            Assert.Equal(CircuitBreakerStatus.On, breaker1.Status);
            Assert.Equal(CircuitBreakerStatus.On, breaker2.Status);
        }

        [Fact]
        public void GetInjectedMethods_1()
        {
            var instance1 = new ExampleClass2();
            var fuse1 = new ExecutionLimitFuse(5);
            var breaker1 = new CircuitBreaker(fuse1);
            var fuse2 = new ExecutionLimitFuse(5);
            var breaker2 = new CircuitBreaker(fuse2);

            breaker1.Inject(() => instance1.ExampleMethod1());
            breaker2.Inject(() => instance1.ExampleMethod1());

            var methods1 = breaker1.GetInjectedMethods(()=>instance1.ExampleMethod1());
            var methods2 = breaker2.GetInjectedMethods(()=>instance1.ExampleMethod1());

            Assert.Equal(1, methods1.Count());
            Assert.Equal(1, methods2.Count());
        }

        [Fact]
        public void GetInjectedMethods_2()
        {
            var instance1 = new ExampleClass2();
            var fuse1 = new ExecutionLimitFuse(5);
            var breaker1 = new CircuitBreaker(fuse1);
            var fuse2 = new ExecutionLimitFuse(5);
            var breaker2 = new CircuitBreaker(fuse2);

            breaker1.Inject(() => instance1.ExampleMethod2(Guid.Empty));
            breaker2.Inject(() => instance1.ExampleMethod2(Guid.Empty));

            var methods1 = breaker1.GetInjectedMethods(() => instance1.ExampleMethod2(Guid.Empty));
            var methods2 = breaker2.GetInjectedMethods(() => instance1.ExampleMethod2(Guid.Empty));

            Assert.Equal(1, methods1.Count());
            Assert.Equal(1, methods2.Count());
        }


        [Fact]
        public void InjectHookToNonSupportExpression()
        {
            var instance1 = new ExampleClass1();

            var fuse = new ExecutionLimitFuse(5);
            var breaker = new CircuitBreaker(fuse);

            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                breaker.Inject(() => instance1.IntValue);
            });

            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                breaker.Inject(() => instance1.StrValue);
            });
        }
    }
}