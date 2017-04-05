using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace stress.execution
{
    public abstract class UnitTestEnumerator : IUnitTestEnumerator
    {
        public IEnumerator<UnitTest> GetEnumerator()
        {
            return (IEnumerator<UnitTest>)GetTests();
        }

        protected abstract IEnumerable<UnitTest> GetTests();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((UnitTestEnumerator)this).GetEnumerator();
        }
    }
}
