using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace stress.execution
{
    public class XunitTestAssemblyEnumerator : UnitTestEnumerator
    {
        private Assembly _assembly;

        public XunitTestAssemblyEnumerator(Assembly assembly)
        {
            _assembly = assembly;
        }

        protected override IEnumerable<UnitTest> GetTests()
        {
            foreach(var typeInfo in _assembly.DefinedTypes)
            {
                var classProvider = new XunitTestClassEnumerator(typeInfo.AsType());

                foreach(var test in classProvider)
                {
                    yield return test;
                }
            }
        }
    }
}
