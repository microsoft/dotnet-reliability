using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace stress.execution
{
    public class XunitUnitTest : UnitTest
    {
        private MethodInfo _methodInfo;
        private ConstructorInfo _dfltCtor;
        private object[][] _paramSets;

        public XunitUnitTest(MethodInfo methodInfo, ConstructorInfo dfltCtor, object[][] paramSets)
        {
            if (methodInfo == null) throw new ArgumentNullException("methodInfo");

            _methodInfo = methodInfo;
            _dfltCtor = dfltCtor;

            if(paramSets == null || paramSets.Length == 0)
            {
                _paramSets = new object[][] { null };
            }
        }

        protected override void ExecuteTest()
        {
            object inst = null;

            if (!_methodInfo.IsStatic)
            {
                inst = _dfltCtor.Invoke(null);
            }

            foreach (var parameters in _paramSets)
            {
                _methodInfo.Invoke(inst, parameters);
            }
        }
    }
}
