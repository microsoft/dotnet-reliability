using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Newtonsoft.Json;
using System.IO;
using System.Diagnostics;

namespace stress.codegen
{
    public class MergeAllProjectJsonsTask : Task
    {
        [Required]
        public string InPath { get; set; }

        [Required]
        public string OutPath { get; set; }

        public bool Debug { get; set; }

        public override bool Execute()
        {
            if (Debug)
            {
                this.Log.LogMessageFromText($"PID:{Process.GetCurrentProcess().Id} Attach debugger now.", MessageImportance.High);

                while (Debug) ;
            }

            Dictionary<string, HashSet<string>> assmVersions = new Dictionary<string, HashSet<string>>();

            var projectJsons = Directory.EnumerateFiles(InPath, "project.json", SearchOption.AllDirectories).Select(p => ProjectJsonDependencyInfo.FromFile(p));

            var merged = ProjectJsonDependencyInfo.MergeToLatest(projectJsons);

            merged.ToFile(OutPath);

            return true;
        }
    }
}
