using EdwardHsu.CircuitBreaker.Fuses;

namespace EdwardHsu.CircuitBreaker.Tests
{
    public class ExecutionLimitCircuitBreakerTest
    {
        [Fact]
        public void ExecutionLimit_TrippedOff()
        {
            for (int i = 1; i < 5; i++)
            {
                var fuse = new ExecutionLimitFuse(i);
                using var breaker = new CircuitBreaker(fuse);

                bool eventChanged = false;
                breaker.StatusChanged += (sender) =>
                {
                    eventChanged = true;
                };

                for (int j = 0; j < i; j++)
                {
                    breaker.Execute(null);
                }

                Assert.True(eventChanged);

                Assert.Equal( CircuitBreakerStatus.TrippedOff,breaker.Status);
            }
        }

        [Fact]
        public void ExecutionLimit_NonTrippedOff()
        {
            for (int i = 1; i < 5; i++)
            {
                var fuse = new ExecutionLimitFuse(i);
                using var breaker = new CircuitBreaker(fuse);

                bool eventChanged = false;
                breaker.StatusChanged += (sender) =>
                {
                    eventChanged = true;
                };

                for (int j = 0; j < i - 1; j++)
                {
                    breaker.Execute(null);
                }

                Assert.False(eventChanged);

                Assert.Equal(CircuitBreakerStatus.On, breaker.Status);
            }
        }

        [Fact]
        public void ExecutionLimit_AutoReset()
        {
            for (int i = 1; i < 5; i++)
            {
                var fuse = new ExecutionLimitFuse(i);
                using var breaker = new CircuitBreaker(fuse);

                breaker.StatusChanged += (sender) =>
                {
                    if (breaker.Status == CircuitBreakerStatus.TrippedOff)
                    {
                        breaker.On();
                    }
                };

                for (int j = 0; j < i; j++)
                {
                    breaker.Execute(null);
                }

                breaker.Execute(null);

                Assert.Equal(CircuitBreakerStatus.On, breaker.Status);
            }
        }

        [Fact]
        public void ExecutionLimit_TrippedOffThenReset()
        {
            for (int i = 2; i < 5; i++)
            {
                var fuse = new ExecutionLimitFuse(i);
                using var breaker = new CircuitBreaker(fuse);

                for (int j = 0; j < i; j++)
                {
                    breaker.Execute(null);
                }

                Assert.Throws<InvalidOperationException>(() =>
                {
                    breaker.Execute(null);
                });

                breaker.On();

                breaker.Execute(null);
                
                Assert.Equal(CircuitBreakerStatus.On, breaker.Status);
            }
        }
    }
}