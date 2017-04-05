using System;
using System.Collections.Generic;
using System.Text;

namespace stress.runtime.contracts
{
    public interface IUnitTest
    {
        string Name { get; }

        event Action<ITestExecutionContext> BeforeExecute;

        event Action<ITestExecutionContext> AfterExecute;

        void Execute();
    }
}
