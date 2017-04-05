using System;
using System.Collections.Generic;
using System.Text;

namespace stress.runtime.contracts
{
    public interface ITestExecutionContext
    {
        IUnitTest Test { get; }

        DateTime Start { get; }

        DateTime End { get; }
    }
}
