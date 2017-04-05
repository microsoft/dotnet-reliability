using stress.runtime.contracts;
using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace stress.runtime
{

    public class ReflectionInvokableUnitTest : IUnitTest, IUnitTestImplInfo
    {
        public object[] Arguments
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public MethodInfo Method
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public string Name
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public event Action<ITestExecutionContext> AfterExecute;
        public event Action<ITestExecutionContext> BeforeExecute;

        public void Execute()
        {
            throw new NotImplementedException();
        }
    }
}
