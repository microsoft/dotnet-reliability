using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Newtonsoft.Json;
using System.IO;

namespace stress.codegen
{
    public class MergeAllProjectJsonsTask : Task
    {
        [Required]
        public string InPath { get; set; }

        [Required]
        public string OutPath { get; set; }

        public override bool Execute()
        {
            Dictionary<string, HashSet<string>> assmVersions = new Dictionary<string, HashSet<string>>();

            var projectJsons = Directory.EnumerateFiles(InPath, "project.json", SearchOption.AllDirectories).Select(p => ProjectJsonInfo.FromFile(p));

            var merged = ProjectJsonInfo.MergeToLatest(projectJsons);

            merged.ToFile(OutPath);

            return true;
        }
    }
}
