using stress.runtime.contracts;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace stress.runtime
{
    public class XunitTestDiscoverer : IUnitTestDiscoverer
    {

        public bool ExploreAssembly(Assembly assembly)
        {
            throw new NotImplementedException();
        }

        public bool ExploreType(Type type)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IUnitTestImplInfo> DiscoverTests(MethodInfo method)
        {
            if(method.IsAbstract || method.IsGenericMethodDefinition)
            {
                return null;
            }

            var factAttr = CustomAttributeData.GetCustomAttributes(method).FirstOrDefault(attr => attr.AttributeType.Name == "FactAttribute");

            if(factAttr != null)
            {

            }

        }
    }
}
