using stress.execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Collections;

namespace stress.tests
{
    public class StressTestsAssemblyTestEnumerator : UnitTestEnumerator
    {
        protected override IEnumerable<UnitTest> GetTests()
        {
            return new XunitTestClassEnumerator(typeof(JaggedArray)).Concat(new XunitTestClassEnumerator(typeof(ArrayTests)));
        }
    }
}
