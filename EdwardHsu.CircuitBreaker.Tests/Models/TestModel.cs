using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdwardHsu.CircuitBreaker.Tests.Models
{
    public class TestModel
    {
        public void Method1(string arg1)
        { 
        }
        public string Method2(string arg1)
        {
            return arg1;
        }
    }
}
