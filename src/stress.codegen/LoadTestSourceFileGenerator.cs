// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace stress.codegen
{
    public class LoadTestSourceFileGenerator : ISourceFileGenerator
    {
        private HashSet<string> _includedAliases = new HashSet<string>();

        private List<string> _testNames = new List<string>();

        public LoadTestInfo LoadTest { get; set; }

        public void GenerateSourceFile(LoadTestInfo testInfo)
        {
            this.LoadTest = testInfo;

            string unitTestsClassContentSnippet = this.BuildUnitTestsClassContentSnippet();

            string unitTestInitSnippet = this.BuildUnitTestInitSnippet();

            string externAliasSnippet = this.BuildExternAliasSnippet();

            string testSnippet = this.BuildTestSnippet();

            string source = $@"
{externAliasSnippet}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using stress.execution;
using Xunit;

namespace stress.generated
{{
    public static class UnitTests
    {{
        {unitTestsClassContentSnippet}

        private static object s_lock = new object();

        private static Random s_rand = new Random({testInfo.Seed});
    }}             

    public static class LoadTestClass
    {{
        {unitTestInitSnippet}

        {testSnippet}
    }}
}}
    ";

            string srcFilePath = Path.Combine(this.LoadTest.SourceDirectory, "LoadTest.cs");

            File.WriteAllText(srcFilePath, source);

            testInfo.SourceFiles.Add(new SourceFileInfo("LoadTest.cs", SourceFileAction.Compile));
        }

        private string BuildExternAliasSnippet()
        {
            StringBuilder snippet = new StringBuilder();

            foreach (var alias in this.LoadTest.AssemblyAliases)
            {
                snippet.Append($"extern alias {alias};{Environment.NewLine}");
            }

            return snippet.ToString();
        }

        private string BuildTestSnippet()
        {
            string testSnippet = $@" 
        [Load(""{ this.LoadTest.Duration.ToString()}"")]
        public static void LoadTestMethod(CancellationToken cancelToken)
        {{
            {this.LoadTest.TestPatternType.Name} testPattern = new {this.LoadTest.TestPatternType.Name}();

            testPattern.Initialize({this.LoadTest.Seed}, g_unitTests);             

            {this.LoadTest.WorkerStrategyType.Name} workerStrategy = new {this.LoadTest.WorkerStrategyType.Name}();

            {this.LoadTest.LoadPatternType.Name} loadPattern = new {this.LoadTest.LoadPatternType.Name}();
            
            loadPattern.WorkerCount = {this.LoadTest.WorkerCount};

            loadPattern.Execute(testPattern, workerStrategy, cancelToken);
        }}";

            return testSnippet;
        }

        private string BuildUnitTestsClassContentSnippet()
        {
            StringBuilder classContentSnippet = new StringBuilder();

            int i = 0;

            foreach (var uTest in this.LoadTest.UnitTests)
            {
                _includedAliases.Add(uTest.AssemblyAlias);
                
                string test = BuildUnitTestMethodSnippet(i++, uTest);

                classContentSnippet.Append(test);
            }

            return classContentSnippet.ToString();
        }

        private string BuildUnitTestMethodSnippet(int index, UnitTestInfo uTest)
        {
            string testName = $"UT{index.ToString("X")}";

            string datasourceName = uTest.IsParameterized ? $"{testName}_DataSource" : null;

            _testNames.Add(testName);

            StringBuilder snippet = new StringBuilder();

            snippet.Append(BuildDataSourceSnippet(datasourceName, uTest));

            snippet.Append($@" 
        [Fact]
        public static void {testName}()
        {{
            {BuildArgLookupSnippet(datasourceName)}{BuildUnitTestMethodCallSnippet(uTest)}
        }}
");
            return snippet.ToString();
        }

        private string BuildDataSourceSnippet(string datasourceName, UnitTestInfo uTest)
        {
            StringBuilder snippet = new StringBuilder();

            if (datasourceName != null)
            { 
                string datafieldName = $"s_{datasourceName.ToLower()}";
                
                snippet.Append($@"
        private static object[][] {datafieldName} = null;

        public static object[][] {datasourceName}
        {{
            get
            {{
                if({datafieldName} == null)
                {{
                    lock(s_lock)
                    {{        
                        if({datafieldName} == null)
                        {{
                            {ReplaceMangledAssembliesWithAliases(BuildDataFieldInitSnippet(datafieldName, uTest))}
                        }}
                    }}
                }}
                
                return {datafieldName};
            }}
        }}
");
            }

            return snippet.ToString();
        }

        private string BuildDataFieldInitSnippet(string datafieldName, UnitTestInfo uTest)
        {
            StringBuilder snippet = new StringBuilder($"{datafieldName} = ((IEnumerable<object[]>)");

            snippet.Append(uTest.ArgumentInfo.DataSources[0]);

            snippet.Append(")");

            for(int i = 1; i< uTest.ArgumentInfo.DataSources.Length; i++)
            {
                snippet.Append(".Concat((IEnumberable<object[]>)");

                snippet.Append(uTest.ArgumentInfo.DataSources[i]);

                snippet.Append(")");
            }

            snippet.Append(".ToArray();");

            return snippet.ToString();
        }

        private string ReplaceMangledAssembliesWithAliases(string str)
        {
            var remaining = str;

            string[] split; 

            HashSet<string> mangledAssemblies = new HashSet<string>();

            while((split = remaining.Split(new string[] { "```[", "]~~~" }, 3, StringSplitOptions.None)).Length == 3)
            {
                mangledAssemblies.Add(split[1]);

                remaining = split[2];
            }

            foreach(var assm in mangledAssemblies)
            {
                str = str.Replace("```[" + assm + "]~~~", UnitTestInfo.GetAssemblyAlias(assm));
            }

            return str;
        }

        private string BuildArgLookupSnippet(string datasourceName)
        {
            StringBuilder snippet = new StringBuilder();

            if(datasourceName != null)
            {
                snippet.Append($"object[] args = {datasourceName}[s_rand.Next({datasourceName}.Length)];");
                snippet.Append(Environment.NewLine);
                snippet.Append(Environment.NewLine);
                snippet.Append("            ");
            }

            return snippet.ToString();
        }

        private string BuildUnitTestMethodCallSnippet(UnitTestInfo uTest)
        {
            StringBuilder snippet = new StringBuilder();
            
            snippet.Append(uTest.Method.IsStatic ? $"{uTest.QualifiedMethodStr}(" : $"new { uTest.QualifiedTypeStr }().{ uTest.QualifiedMethodStr}(");

            for(int i = 0; i < uTest.ParameterCount; i++)
            {
                if(i > 0)
                {
                    snippet.Append(", ");
                }

                snippet.Append($"({ReplaceMangledAssembliesWithAliases(uTest.ArgumentInfo.ArgumentTypes[i])})args[{i}]");
            }

            snippet.Append(")");

            if(uTest.Method.IsTaskReturn)
            {
                snippet.Append(".GetAwaiter().GetResult()");
            }

            snippet.Append(";");

            return snippet.ToString();
        }

        private string BuildUnitTestInitSnippet()
        {
            StringBuilder arrayContentSnippet = new StringBuilder();

            foreach (var testName in _testNames)
            {
                arrayContentSnippet.Append($@"new ActionUnitTest(UnitTests.{testName}),
            ");
            }

            return $@"
        static ActionUnitTest[] g_unitTests = new ActionUnitTest[] 
        {{
            {arrayContentSnippet}
        }};";
        }
    }
}
