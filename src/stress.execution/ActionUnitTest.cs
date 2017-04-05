using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace stress.execution
{
    public class ActionUnitTest : UnitTest
    {
        public ActionUnitTest(Action action = null, bool trapExceptions = true)
            : base(trapExceptions)
        {
            this.Action = action;
        }

        public Action Action { get; set; }

        protected override void ExecuteTest()
        {
            this.Action();
        }
        

    }
}
