using EdwardHsu.CircuitBreaker.Fuses;

namespace EdwardHsu.CircuitBreaker.Tests
{
    public class TimeSlidingWindowLimitCircuitBreakerTest
    {
        [Fact]
        public void TimeSlidingWindowLimit_TrippedOff()
        {
            for (int i = 2; i < 5; i++)
            {
                var fuse = new TimeSlidingWindowLimitFuse(i, TimeSpan.FromSeconds(1));
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
        public void TimeSlidingWindowLimit_NonTrippedOff()
        {
            for (int i = 2; i < 5; i++)
            {
                var fuse = new TimeSlidingWindowLimitFuse(i, TimeSpan.FromSeconds(1));
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
        public void TimeSlidingWindowLimit_AutoReset()
        {
            for (int i = 2; i < 5; i++)
            {
                var fuse = new TimeSlidingWindowLimitFuse(i, TimeSpan.FromSeconds(1));
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
        public void TimeSlidingWindowLimit_TrippedOffThenReset()
        {
            for (int i = 2; i < 5; i++)
            {
                var fuse = new TimeSlidingWindowLimitFuse(i, TimeSpan.FromSeconds(1));
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

        [Fact]
        public async Task TimeSlidingWindowLimit_NonTrippedOffThenCalmdown()
        {
            for (int i = 2; i < 5; i++)
            {
                var fuse = new TimeSlidingWindowLimitFuse(i, TimeSpan.FromSeconds(1));
                using var breaker = new CircuitBreaker(fuse);

                for (int j = 0; j < i - 1; j++)
                {
                    breaker.Execute(null);
                }

                await Task.Delay(TimeSpan.FromSeconds(2));

                for (int j = 0; j < i - 1; j++)
                {
                    breaker.Execute(null);
                }

                Assert.Equal(CircuitBreakerStatus.On, breaker.Status);
            }
        }
    }
}