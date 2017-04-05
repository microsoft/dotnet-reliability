using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace stress.execution
{
    public class XunitTestClassEnumerator : UnitTestEnumerator
    {
        private Type _classType;

        public XunitTestClassEnumerator(Type classType)
        {
            if (classType == null) throw new ArgumentNullException("classType");

            _classType = classType;
            
        }

        protected override IEnumerable<UnitTest> GetTests()
        {
            var typeInfo = _classType.GetTypeInfo();
            var dfltCtor = typeInfo.DeclaredConstructors.FirstOrDefault(c => c.GetParameters().Length == 0);

            //only provide tests if the class implemented
            if (!typeInfo.IsInterface && !typeInfo.IsAbstract)
            {
                foreach (var method in typeInfo.DeclaredMethods)
                {
                    //if the method is instance based ignore if the type is not instantiable
                    if (method.IsStatic || dfltCtor != null)
                    {
                        var attributes = method.CustomAttributes;

                        var factAttr = attributes.FirstOrDefault(attr => attr.AttributeType.Name == "FactAttribute");

                        var theoryAttr = attributes.FirstOrDefault(attr => attr.AttributeType.Name == "TheoryAttribute");

                        if (factAttr != null || theoryAttr != null)
                        {
                            object[][] paramSets = theoryAttr != null ? GetTheoryParameterSets(attributes, typeInfo, dfltCtor).ToArray() : null;

                            yield return new XunitUnitTest(method, dfltCtor, paramSets);
                        }
                    }
                }
            }
        }

        private IEnumerable<object[]> GetTheoryParameterSets(IEnumerable<CustomAttributeData> attributes, TypeInfo typeInfo, ConstructorInfo dfltCtor)
        {
            foreach(var attr in attributes)
            {
                if(attr.AttributeType.Name == "InlineDataAttribute")
                {
                    yield return attr.ConstructorArguments[0].Value as object[];
                }
                else if(attr.AttributeType.Name == "MemberDataAttribute")
                {
                    string memberName = attr.ConstructorArguments[0].Value as string;

                    var member = typeInfo.DeclaredMembers.FirstOrDefault(m => m.Name == memberName);

                    PropertyInfo prop;
                    FieldInfo fld;
                    MethodInfo meth;

                    IEnumerable<object[]> paramSets = null;

                    if((prop = member as PropertyInfo) != null)
                    {
                        var inst = prop.GetMethod.IsStatic ? null : dfltCtor.Invoke(null);

                        paramSets = (IEnumerable<object[]>)prop.GetValue(inst);
                    }
                    if((fld = member as FieldInfo) != null)
                    {
                        var inst = dfltCtor.Invoke(null);

                        paramSets = (IEnumerable<object[]>)prop.GetValue(inst);
                    }
                    if((meth = member as MethodInfo) != null)
                    {
                        var inst = meth.IsStatic ? null : dfltCtor.Invoke(null);

                        paramSets = (IEnumerable<object[]>)prop.GetValue(inst);
                    }

                    if (paramSets != null)
                    {
                        foreach (var set in paramSets)
                        {
                            yield return set;
                        }
                    }
                }
            }
        }
    }
}
