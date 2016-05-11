// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace stress.codegen
{
    public class LoadTestInfo
    {
        private string _globalPackageConfigPath = null;

        public LoadTestInfo(string globalPackageConfigPath)
        {
            _globalPackageConfigPath = globalPackageConfigPath;

            this.SourceFiles = new List<SourceFileInfo>();
        }

        public string TestName { get; set; }

        public Type TestPatternType { get; set; }

        public Type LoadPatternType { get; set; }

        public Type WorkerStrategyType { get; set; }

        public TimeSpan Duration { get; set; }

        public int WorkerCount { get; set; }

        public string SourceDirectory { get; set; }

        public IEnumerable<UnitTestInfo> UnitTests { get; set; }

        public IList<SourceFileInfo> SourceFiles { get; private set; }

        public ProjectJsonDependencyInfo PackageInfo
        {
            get
            {
                if (string.IsNullOrEmpty(_globalPackageConfigPath))
                {
                    var jsonRefInfo = this.UnitTests.Select(ut => ut.ReferenceInfo.PackageInfo);

                    return ProjectJsonDependencyInfo.MergeToLatest(jsonRefInfo);
                }
                else
                {
                    return ProjectJsonDependencyInfo.FromFile(_globalPackageConfigPath);
                }
            }
        }

        public IEnumerable<string> AssemblyAliases
        {
            get
            {
                return this.UnitTests.Select(t => t.AssemblyAlias).Distinct();
            }
        }

        public Dictionary<string, string> EnvironmentVariables { get; set; }

        public LoadSuiteConfig SuiteConfig { get; set; }
    }
}
