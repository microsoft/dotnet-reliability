using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace stress.runtime.contracts
{
    public interface IUnitTestAssemblyExplorer
    {
        IEnumerable<IUnitTestImplInfo> DiscoverTests(Assembly testAssembly, params IUnitTestDiscoverer[] discoverers);
    }
}
