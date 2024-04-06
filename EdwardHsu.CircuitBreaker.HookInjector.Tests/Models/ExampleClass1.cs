using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdwardHsu.CircuitBreaker.HookInjector.Tests.Models
{
    public class ExampleClass1
    {
        public int IntValue;
        public string StrValue { get; set; }

        public static void Method1_1()
        {
        }
        public static void Method1_2()
        {
        }
        public static void Method1_3()
        {
        }

        public static Guid Method2_1(Guid value)
        {
            return value;
        }
        public static Guid Method2_2(Guid value)
        {
            return value;
        }
        public static Guid Method2_3(Guid value)
        {
            return value;
        }
    }
}
