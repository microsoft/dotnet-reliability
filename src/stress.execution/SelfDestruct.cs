using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace stress.execution
{
    public class SelfDestructException : Exception
    {
        public SelfDestructException() : base("The operation self destructed.") { }
    }

    public static class SelfDestruct
    {
        public static void WithException()
        {
            throw new SelfDestructException();
        }
    }
}
