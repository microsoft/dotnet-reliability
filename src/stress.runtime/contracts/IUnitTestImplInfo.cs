using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace stress.runtime.contracts
{
    public interface IUnitTestImplInfo
    {
        MethodInfo Method { get; }
        
        object[] Arguments { get; }
    }
}
