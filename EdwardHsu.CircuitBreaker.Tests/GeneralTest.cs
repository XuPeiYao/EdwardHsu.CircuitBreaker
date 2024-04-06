using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EdwardHsu.CircuitBreaker.Fuses;

namespace EdwardHsu.CircuitBreaker.Tests
{
    public class GeneralTest
    {
        [Fact]
        public void GetFuseFromBreaker()
        {
            var fuse = new ExecutionLimitFuse(5);
            var breaker = new CircuitBreaker(fuse);

            Assert.Equal(fuse, breaker.Fuse);
        }

        [Fact]
        public void ExecutionLimitFuseTripped()
        {
            var fuse = new ExecutionLimitFuse(1);

            Assert.True(fuse.TryPass(null));

            Assert.False(fuse.TryPass(null));

            fuse.Reset();

            Assert.True(fuse.TryPass(null));
        }

        [Fact]
        public void TimeSlidingWindowLimitFuseTripped()
        {
            var fuse = new TimeSlidingWindowLimitFuse(1, TimeSpan.FromSeconds(1));

            Assert.True(fuse.TryPass(null));

            Assert.False(fuse.TryPass(null));

            fuse.Reset();

            Assert.True(fuse.TryPass(null));
        }
    }
}
