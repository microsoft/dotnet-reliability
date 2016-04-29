// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using stress.execution;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace stress.codegen
{
    public class LoadTestProjectFileGenerator : ISourceFileGenerator
    {
        public void GenerateSourceFile(LoadTestInfo loadTest)
        {
            string refSnippet = GenerateTestReferencesSnippet(loadTest);
            
            string itemSnippet = GenerateSourceFileItemsSnippet(loadTest);

            string propertySnippet = GenerateTestPropertiesSnippet(loadTest);

            //format the project template {0} source files, {1} test references, {2} test properties
            string projFileContent = string.Format(PROJECT_TEMPLATE, itemSnippet, refSnippet, propertySnippet);

            File.WriteAllText(Path.Combine(loadTest.SourceDirectory, loadTest.TestName + ".csproj"), projFileContent);
        }

        private static string GenerateTestPropertiesSnippet(LoadTestInfo loadTest)
        {
            //timeout = test duration + 5 minutes for dump processing ect.
            string timeoutInSeconds = Convert.ToInt32((loadTest.Duration + TimeSpan.FromMinutes(5)).TotalSeconds).ToString();

            string propertyString = $@"
    <TimeoutInSeconds>{timeoutInSeconds}</TimeoutInSeconds>";

            return propertyString;
        }

        private static string GenerateSourceFileItemsSnippet(LoadTestInfo loadTest)
        {
            StringBuilder snippet = new StringBuilder();

            foreach (var file in loadTest.SourceFiles)
            {
                string itemSnippet = string.Empty;
                if (file.SourceFileAction == SourceFileAction.Compile)
                {
                    itemSnippet = $@"
    <Compile Include='{Path.GetFileName(file.FileName)}'/>";
                }
                else if (file.SourceFileAction == SourceFileAction.Binplace)
                {
                    itemSnippet = $@"
    <None Include='{Path.GetFileName(file.FileName)}'>
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </None> ";
                }

                snippet.Append(itemSnippet);
            }

            return snippet.ToString();
        }

        private static string GenerateTestReferencesSnippet(LoadTestInfo loadTest)
        {
            HashSet<string> uniqueAssemblies = new HashSet<string>();

            var packageInfo = loadTest.PackageInfo;

            StringBuilder snippet = new StringBuilder();
            foreach (var test in loadTest.UnitTests)
            {
                if (uniqueAssemblies.Add(test.AssemblyName))
                {
                    string refSnippet = $@"
    <Reference Include='{test.AssemblyName}'>
      <HintPath>$(MSBuildThisFileDirectory)\refs\{test.AssemblyName}</HintPath>
      <Aliases>{UnitTestInfo.GetAssemblyAlias(test.AssemblyName)}</Aliases>
      <NotForTests>true</NotForTests>
    </Reference>";

                    snippet.Append(refSnippet);
                }

                foreach (var assmref in test.ReferenceInfo.ReferencedAssemblies)
                {
                    if (uniqueAssemblies.Add(assmref.Name) && !packageInfo.dependencies.ContainsKey(assmref.Name))
                    {
                        string refSnippet = $@"
    <Reference Include='{assmref.Name}'>
      <HintPath>$(MSBuildThisFileDirectory)\refs\{assmref.Name}</HintPath>
      <Aliases>{UnitTestInfo.GetAssemblyAlias(assmref.Name)}</Aliases>
      <NotForTests>true</NotForTests>
    </Reference>";

                        snippet.Append(refSnippet);
                    }
                }
            }

            return snippet.ToString();
        }

        private const string PROJECT_TEMPLATE = @"<?xml version='1.0' encoding='utf-8'?>
<Project ToolsVersion = '4.0' DefaultTargets='Build' xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
  <Import Project = '$([MSBuild]::GetDirectoryNameOfFileAbove($(MSBuildThisFileDirectory), dir.props))\dir.props' />
  <PropertyGroup>
    <SignAssembly>false</SignAssembly>
  </PropertyGroup>
  <PropertyGroup>
    <IsTestProject>true</IsTestProject>
  </PropertyGroup>  
  <!-- Test Properties -->
  <PropertyGroup>{2}
  </PropertyGroup>
  <!-- Source Code Files -->
  <ItemGroup>{0}
  </ItemGroup>
  <!-- Test Assembly References -->
  <ItemGroup>{1}
  </ItemGroup>
  <Import Project = '$([MSBuild]::GetDirectoryNameOfFileAbove($(MSBuildThisFileDirectory), dir.targets))\dir.targets' />
</Project>";
        
    }
}
