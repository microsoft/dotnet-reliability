using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;
using stress.execution;
using stress.tests;

namespace stress.run
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Assembly.Load(new AssemblyName("stress.runtime"));
            
        }
    }
}
