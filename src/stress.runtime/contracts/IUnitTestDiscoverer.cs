using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace stress.runtime.contracts
{
    public interface IUnitTestDiscoverer
    {
        bool ExploreAssembly(Assembly assembly);

        bool ExploreType(Type type);

        IEnumerable<IUnitTestImplInfo> DiscoverTests(MethodInfo member);
    }
}
