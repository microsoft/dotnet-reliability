// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace stress.codegen
{
    public class XUnitTestDiscoverer : ITestDiscoverer
    {
        private TestAssemblyInfo _assemblyInfo;

        public UnitTestInfo[] GetTests(TestAssemblyInfo assemblyInfo)
        {
            _assemblyInfo = assemblyInfo;

            List<string> assmRefs = new List<string>();

            foreach (var refAssm in assemblyInfo.Assembly.GetReferencedAssemblies().Select(name => name.Name).Where(name => !name.StartsWith("System")))
            {
                assmRefs.Add(refAssm);
            }

            List<UnitTestInfo> tests = new List<UnitTestInfo>();

            if (assemblyInfo != null)
            {
                try
                {
                    var explExcluded = assemblyInfo.Assembly.GetCustomAttributesData().FirstOrDefault(attrData => attrData.AttributeType.Name == "AssemblyTraitAttribute"
                                                                                                        && attrData.ConstructorArguments.Count == 2
                                                                                                        && attrData.ConstructorArguments[0].Value as string == "StressCategory"
                                                                                                        && attrData.ConstructorArguments[1].Value as string == "Excluded") != null;
                    foreach (var assmClass in assemblyInfo.Assembly.GetTypes())
                    {
                        foreach (var method in assmClass.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance))
                        {
                            var attributes = method.GetCustomAttributesData();

                            var testAttr = attributes.FirstOrDefault(attr => attr.AttributeType.Name == "FactAttribute" || attr.AttributeType.Name == "TheoryAttribute");

                            if (testAttr != null)
                            {
                                UnitTestInfo test = new UnitTestInfo()
                                {
                                    AssemblyPath = assemblyInfo.Assembly.Location,
                                    AssemblyLastModified = File.GetLastWriteTime(assemblyInfo.Assembly.Location),
                                    ReferenceInfo = assemblyInfo.ReferenceInfo,
                                    Class = new TestClassInfo
                                    {
                                        FullName = assmClass.FullName,
                                        IsAbstract = assmClass.IsAbstract,
                                        IsGenericType = assmClass.IsGenericType || assmClass.IsGenericTypeDefinition,
                                        IsPublic = assmClass.IsPublic,
                                        HasDefaultCtor = assmClass.GetConstructor(Type.EmptyTypes) != null
                                    },
                                    Method = new TestMethodInfo
                                    {
                                        Name = method.Name,
                                        IsAbstract = method.IsAbstract,
                                        IsGenericMethodDefinition = method.IsGenericMethodDefinition,
                                        IsPublic = method.IsPublic,
                                        IsStatic = method.IsStatic,
                                        IsTaskReturn = method.ReturnType.FullName.StartsWith(typeof(Task).FullName),
                                        ExplicitlyExcluded = explExcluded,
                                    },
                                    ArgumentInfo = GetTestArgumentInfo(method, attributes),
                                };

                                tests.Add(test);
                            }
                        }
                    }
                }
                catch(ReflectionTypeLoadException typeException)
                {
                    Console.WriteLine($"Type loading exception: {typeException.Message}\r\nLoader Exceptions:\r\n\t{string.Join("\r\n", typeException.LoaderExceptions.Select(x => x.Message))}");
                }
            }

            return tests.ToArray();
        }

        public TestArgumentInfo GetTestArgumentInfo(MethodInfo method, IList<CustomAttributeData> attributes)
        {
            var dataSources = new List<string>();

            //get the inline data attributes
            var attrDataStrs = attributes.Where(attrData => attrData.AttributeType.Name == "InlineDataAttribute")
                                         .Select(attrData => attrData.ConstructorArguments[0])
                                         .Select(ctorArg => GetAttributeArgumentSnippet(ctorArg))
                                         .ToArray();

            //if there are inline data attributes add them to the datasources
            if(attrDataStrs != null && attrDataStrs.Length > 0)
            {
                dataSources.Add("new object[][] { " + string.Join(",", attrDataStrs) + " }");
            }

            TestArgumentInfo argInfo = new TestArgumentInfo()
            {
                ArgumentTypes = method.GetParameters().Select(pInfo => GetMangledReplacementTypeString(pInfo.ParameterType)).ToArray(),
                DataSources = dataSources.Count != 0 ? dataSources.ToArray() : (string[])null
            };

            return argInfo;
        }
        
        private string GetAttributeArgumentSnippet(CustomAttributeTypedArgument arg)
        {
            string snippet = string.Empty;

            string argType = GetMangledReplacementTypeString(arg.ArgumentType);

            if (arg.ArgumentType.FullName.EndsWith("[]"))
            {
                if (arg.Value == null)
                {
                    snippet = $"({argType})null";
                }
                else
                {
                    snippet += $"new {argType} {{ ";

                    snippet += string.Join(", ", ((ReadOnlyCollection<CustomAttributeTypedArgument>)arg.Value).Select(cata => GetAttributeArgumentSnippet(cata)));

                    snippet += " }";
                }
            }
            else
            {
                if (arg.Value == null)
                {
                    snippet = $"({argType})null";
                }
                else
                {
                    if (arg.ArgumentType.FullName == typeof(Type).FullName)
                    {
                        snippet = "typeof(" + GetMangledReplacementTypeString((Type)arg.Value) + ")";
                    }
                    else
                    {
                        snippet = GetCodeLiteralExpression(arg.Value);
                        
                        snippet = $"({argType})({snippet})";
                    }
                }
            }
            return snippet;
        }

        private string GetMangledReplacementTypeString(Type type)
        {
            string mangled = GetCodeLiteralExpression(type);

            if(type.Assembly == _assemblyInfo.Assembly)
            {
                mangled = $"```[{Path.GetFileName(type.Assembly.Location)}]~~~::" + mangled;
            }

            return mangled;
        }

        private string GetCodeLiteralExpression(object str)
        {
            using (var writer = new StringWriter())
            {
                using (var provider = CodeDomProvider.CreateProvider("CSharp"))
                {
                    provider.GenerateCodeFromExpression(new CodePrimitiveExpression(str), writer, null);
                    return writer.ToString();
                }
            }
        }

        private string GetCodeLiteralExpression(Type type)
        {

            using (var writer = new StringWriter())
            {
                using (var provider = CodeDomProvider.CreateProvider("CSharp"))
                {
                    provider.GenerateCodeFromExpression(new CodeTypeReferenceExpression(type), writer, null);
                    return writer.ToString();
                }
            }
        }
    }
}
