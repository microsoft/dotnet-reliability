using stress.runtime.contracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace stress.runtime
{
    public class UnitTestBase : IUnitTest
    {
        public string Name { get; set; }

        public event Action<ITestExecutionContext> AfterExecute;

        public event Action<ITestExecutionContext> BeforeExecute;

        public void Execute()
        {
            var context = new TestExecutionContext() { Test = this, Start = DateTime.Now };

            if (BeforeExecute != null)
            {
                BeforeExecute(context);
            }
        }

        protected virtual void Execute(ITestExecutionContext context)
        {

        }

        private class TestExecutionContext : ITestExecutionContext
        {
            public DateTime End { get; set; }

            public DateTime Start { get; set; }

            public IUnitTest Test { get; set; }
        }
    }
}
