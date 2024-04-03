using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using EdwardHsu.CircuitBreaker.Fuses;
using EdwardHsu.CircuitBreaker.Internal;
using EdwardHsu.CircuitBreaker.Tests.Models;
using HarmonyLib;

using static System.Net.Mime.MediaTypeNames;

namespace EdwardHsu.CircuitBreaker.Tests
{
    public class TimeSlidingWindowCountFuseTest
    {
        public static string ExampleStaticMethod1()
        {
            return "";
        }

        [Fact]
        public async Task TimeSlidingWindowCount_StaticMethod1()
        {
            var fuse = new TimeSlidingWindowCountFuse(10, TimeSpan.FromSeconds(1));
            var breaker = new CircuitBreaker(fuse, () => ExampleStaticMethod1());
            
            var startTime = DateTime.UtcNow;
            await Assert.ThrowsAsync<InvalidOperationException>(async() =>
            {
                foreach (var i in Enumerable.Range(0, 1000))
                {
                    ExampleStaticMethod1();
                    
                    await Task.Delay(TimeSpan.FromMilliseconds(5));
                }
            });
            
            Assert.True(DateTime.UtcNow - startTime < TimeSpan.FromSeconds(1));
            
            Assert.Equal(CircuitBreakerStatus.TrippedOff, breaker.Status);

            breaker.On();
            breaker.On();

            Assert.Equal(CircuitBreakerStatus.On, breaker.Status);

            ExampleStaticMethod1();

            breaker.Off();
            breaker.Off();

            Assert.Throws<InvalidOperationException>(() =>
            {
                ExampleStaticMethod1();
            });

            await breaker.DisposeAsync();
        }



        public static void ExampleStaticMethod2()
        {
        }

        [Fact]
        public void TimeSlidingWindowCount_StaticMethod2()
        {
            var fuse = new TimeSlidingWindowCountFuse(10, TimeSpan.FromSeconds(1));
            using var breaker = new CircuitBreaker(fuse, () => ExampleStaticMethod2());

            var startTime = DateTime.UtcNow;
            Assert.Throws<InvalidOperationException>(() =>
            {
                foreach (var i in Enumerable.Range(0, 1000))
                {
                    ExampleStaticMethod2();
                    Task.Delay(TimeSpan.FromMilliseconds(5)).Wait();
                }
            });

            Assert.True(DateTime.UtcNow - startTime < TimeSpan.FromSeconds(1));

            Assert.Equal(CircuitBreakerStatus.TrippedOff, breaker.Status);

            breaker.On();

            Assert.Equal(CircuitBreakerStatus.On, breaker.Status);

            ExampleStaticMethod2();

            breaker.Off();

            Assert.Throws<InvalidOperationException>(() =>
            {
                ExampleStaticMethod2();
            });
        }



        [Fact]
        public void TimeSlidingWindowCount_Method1()
        {
            var testInstance = new TestModel();
            var fuse = new TimeSlidingWindowCountFuse(10, TimeSpan.FromSeconds(1));
            using var breaker = new CircuitBreaker(fuse, () => testInstance.Method1(""));

            var startTime = DateTime.UtcNow;
            int count = 0;
            Assert.Throws<InvalidOperationException>(() =>
            {
                foreach (var i in Enumerable.Range(0, 1000))
                {
                    testInstance.Method1("");
                    count++;
                    Task.Delay(TimeSpan.FromMilliseconds(5)).Wait();
                }
            });
            
            Assert.True(DateTime.UtcNow - startTime < TimeSpan.FromSeconds(1));

            Assert.Equal(CircuitBreakerStatus.TrippedOff, breaker.Status);
            
            breaker.On();

            Assert.Equal(CircuitBreakerStatus.On, breaker.Status);

            testInstance.Method1("");

            breaker.Off();

            Assert.Throws<InvalidOperationException>(() =>
            {
                testInstance.Method1("");
            });
        }

        [Fact]
        public void TimeSlidingWindowCount_Method2()
        {
            var testInstance = new TestModel();
            var fuse = new TimeSlidingWindowCountFuse(10, TimeSpan.FromSeconds(1));
            using var breaker = new CircuitBreaker(fuse, () => testInstance.Method2(""));

            var startTime = DateTime.UtcNow;
            Assert.Throws<InvalidOperationException>(() =>
            {
                foreach (var i in Enumerable.Range(0, 1000))
                {
                    testInstance.Method2("");
                    Task.Delay(TimeSpan.FromMilliseconds(5)).Wait();
                }
            });

            Assert.True(DateTime.UtcNow - startTime < TimeSpan.FromSeconds(1));

            Assert.Equal(CircuitBreakerStatus.TrippedOff, breaker.Status);
            
            breaker.On();

            Assert.Equal(CircuitBreakerStatus.On, breaker.Status);
            
            testInstance.Method2("");

            breaker.Off();

            Assert.Throws<InvalidOperationException>(() =>
            {
                testInstance.Method2("");
            });
        }
    }
}