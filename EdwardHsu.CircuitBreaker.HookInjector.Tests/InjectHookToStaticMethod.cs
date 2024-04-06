using EdwardHsu.CircuitBreaker.Fuses;
using EdwardHsu.CircuitBreaker.HookInjector.Tests.Models;

namespace EdwardHsu.CircuitBreaker.HookInjector.Tests
{
    public class InjectHookToStaticMethod
    {
        [Fact]
        public void InjectHookToStaticMethodThenUninject_Action()
        {
            var fuse = new ExecutionLimitFuse(5);
            var breaker = new CircuitBreaker(fuse);

            breaker.Inject(()=>ExampleClass1.Method1_1());

            Assert.True(breaker.IsInjected(() => ExampleClass1.Method1_1()));

            breaker.Uninject(()=>ExampleClass1.Method1_1());

            Assert.False(breaker.IsInjected(() => ExampleClass1.Method1_1()));
        }

        [Fact]
        public void InjectHookToStaticMethodThenUninject_Func()
        {
            var fuse = new ExecutionLimitFuse(5);
            var breaker = new CircuitBreaker(fuse);

            breaker.Inject(() => ExampleClass1.Method2_1(Guid.Empty));

            Assert.True(breaker.IsInjected(() => ExampleClass1.Method2_1(Guid.Empty)));

            breaker.Uninject(() => ExampleClass1.Method2_1(Guid.Empty));

            Assert.False(breaker.IsInjected(() => ExampleClass1.Method2_1(Guid.Empty)));
        }

        [Fact]
        public void InjectHookToStaticMethod_Action()
        {
            var fuse = new ExecutionLimitFuse(5);
            var breaker = new CircuitBreaker(fuse);

            breaker.Inject(() => ExampleClass1.Method1_2());

            Assert.True(breaker.IsInjected(() => ExampleClass1.Method1_2()));

            Assert.Equal(CircuitBreakerStatus.On, breaker.Status);

            for (int i = 0; i < 5; i++)
            {
                ExampleClass1.Method1_2();
            }

            Assert.Equal(CircuitBreakerStatus.TrippedOff, breaker.Status);

            Assert.Throws<InvalidOperationException>(() =>
            {
                ExampleClass1.Method1_2();
            });

            breaker.On();

            Assert.Equal(CircuitBreakerStatus.On, breaker.Status);
        }

        [Fact]
        public void InjectHookToStaticMethod_Func()
        {
            var fuse = new ExecutionLimitFuse(5);
            var breaker = new CircuitBreaker(fuse);

            breaker.Inject(() => ExampleClass1.Method2_2(Guid.Empty));

            Assert.True(breaker.IsInjected(() => ExampleClass1.Method2_2(Guid.Empty)));

            Assert.Equal(CircuitBreakerStatus.On, breaker.Status);

            for (int i = 0; i < 5; i++)
            {
                ExampleClass1.Method2_2(Guid.Empty);
            }

            Assert.Equal(CircuitBreakerStatus.TrippedOff, breaker.Status);

            Assert.Throws<InvalidOperationException>(() =>
            {
                ExampleClass1.Method2_2(Guid.Empty);
            });

            breaker.On();

            Assert.Equal(CircuitBreakerStatus.On, breaker.Status);
        }

        [Fact]
        public void InjectDiffHookToSameStaticMethod_Action()
        {
            var fuse1 = new ExecutionLimitFuse(5);
            var breaker1 = new CircuitBreaker(fuse1);

            var fuse2 = new ExecutionLimitFuse(5);
            var breaker2 = new CircuitBreaker(fuse2);

            breaker1.Inject(() => ExampleClass1.Method1_3());
            breaker2.Inject(() => ExampleClass1.Method1_3());

            Assert.True(breaker1.IsInjected(() => ExampleClass1.Method1_3()));
            Assert.True(breaker2.IsInjected(() => ExampleClass1.Method1_3()));

            Assert.Equal(CircuitBreakerStatus.On, breaker1.Status);

            for (int i = 0; i < 5; i++)
            {
                ExampleClass1.Method1_3();
            }

            Assert.Equal(CircuitBreakerStatus.TrippedOff, breaker1.Status);
            Assert.Equal(CircuitBreakerStatus.TrippedOff, breaker2.Status);

            Assert.Throws<InvalidOperationException>(() =>
            {
                ExampleClass1.Method1_3();
            });

            breaker1.On();
            breaker2.On();

            Assert.Equal(CircuitBreakerStatus.On, breaker1.Status);
            Assert.Equal(CircuitBreakerStatus.On, breaker2.Status);
        }

        [Fact]
        public void InjectDiffHookToSameStaticMethod_Func()
        {
            var fuse1 = new ExecutionLimitFuse(5);
            var breaker1 = new CircuitBreaker(fuse1);

            var fuse2 = new ExecutionLimitFuse(5);
            var breaker2 = new CircuitBreaker(fuse2);

            breaker1.Inject(() => ExampleClass1.Method2_3(Guid.Empty));
            breaker2.Inject(() => ExampleClass1.Method2_3(Guid.Empty));

            Assert.True(breaker1.IsInjected(() => ExampleClass1.Method2_3(Guid.Empty)));
            Assert.True(breaker2.IsInjected(() => ExampleClass1.Method2_3(Guid.Empty)));

            Assert.Equal(CircuitBreakerStatus.On, breaker1.Status);

            for (int i = 0; i < 5; i++)
            {
                ExampleClass1.Method2_3(Guid.Empty);
            }

            Assert.Equal(CircuitBreakerStatus.TrippedOff, breaker1.Status);
            Assert.Equal(CircuitBreakerStatus.TrippedOff, breaker2.Status);

            Assert.Throws<InvalidOperationException>(() =>
            {
                ExampleClass1.Method2_3(Guid.Empty);
            });

            breaker1.On();
            breaker2.On();

            Assert.Equal(CircuitBreakerStatus.On, breaker1.Status);
            Assert.Equal(CircuitBreakerStatus.On, breaker2.Status);
        }
    }
}