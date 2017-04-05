using System;
using System.Collections.Generic;
using System.Text;

namespace stress.runtime.contracts
{
    public interface ITestPattern
    {
        IUnitTest GetNextTest();
    }
}
