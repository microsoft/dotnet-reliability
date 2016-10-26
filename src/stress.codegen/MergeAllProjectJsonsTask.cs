using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Newtonsoft.Json;
using System.IO;
using System.Diagnostics;
using System.Windows;
using Newtonsoft.Json.Linq;

namespace stress.codegen
{
    
    // This merge class walks all of the directory tree of InPath and produces a merged project json file. Then we 
    // 'trim it down' to the type of run we'd like to support, and write that final project.json to disk at OutPath.
    public class MergeAllProjectJsonsTask : Task
    {
        // assumptions:
        // we assume that anyos test project.jsons (AND ONLY THESE; any other ones will mess up the merge) are 
        // present in the directory or subdirectories of InPath
        [Required]
        public string InPath { get; set; }
        
        // the path to the project.json we are spawning.
        [Required]
        public string OutPath { get; set; }

        // not currently being used, but they exist to allow tests to be 'upgraded' in place.
        public string OldPrerelease { get; set; }
        public string NewPrerelease { get; set; }

        // set this to true when you want to debug. It causes a message box to show with the process id of the executing
        // msbuild instance that you can attach your debugger to. The execution of code will sit until you click ok.
        public bool Debug { get; set; }

        // this method walks the directory tree of inPath and uses the Newtonsoft JObject::Merge to merge together
        // our project jsons.
        // we require there to be at least ONE project.json present somewhere in the directory tree of inPath
        private JObject MergeProjectJsonsIn(string inPath)
        {
            var projectJsons = Directory.EnumerateFiles(InPath, "project.json", SearchOption.AllDirectories).Select(p => JObject.Parse(File.ReadAllText(p)));
            JObject merged = new JObject(projectJsons.First());
            
            foreach (var file in projectJsons)
            {
                // 
                merged.Merge(file);
            }
         
            return merged;   
        }
        
        // this method:
        // strips out test-runtime object if it is present in the dependencies list.
        // removes all frameworks except for the one that is to be targeted for the run
        private void ProduceAnyOsProjectJson(string outPath, JObject merged, string targetFramework)
        {
            // remove test-runtime if it is present.
            (merged["dependencies"] as JObject)?.Property("test-runtime")?.Remove();

            // remove all supports.
            merged["supports"]?.Children<JProperty>().ToList().ForEach(x => x.Remove());
            
            // remove all frameworks except the specified one
            merged["frameworks"]?.Children<JProperty>().ToList().ForEach(x => x.Remove());
            merged["frameworks"][targetFramework] = new JObject();

            // since we are creating an anyos test launcher we should list all known runtimes here.
            merged["runtimes"] = new JObject();
            merged["runtimes"]["win10-x64"] = new JObject();
            merged["runtimes"]["win7-x64"] = new JObject();
            merged["runtimes"]["win7-x86"] = new JObject();
            merged["runtimes"]["ubuntu.14.04-x64"] = new JObject();
            merged["runtimes"]["osx.10.10-x64"] = new JObject();
            merged["runtimes"]["centos.7-x64"] = new JObject();
            merged["runtimes"]["rhel.7-x64"] = new JObject();
            merged["runtimes"]["debian.8-x64"] = new JObject();

            // serialize, then write the project json file to OutPath
            File.WriteAllText(OutPath, JsonConvert.SerializeObject(merged));
        }

        public override bool Execute()
        {
            if (Debug)
            {
                MessageBox.Show($"PID:{Process.GetCurrentProcess().Id} Attach debugger now.", "Debug GenerateStressSuiteTask", MessageBoxButton.OK);
            }
            
            ProduceAnyOsProjectJson(OutPath, MergeProjectJsonsIn(InPath), "netcoreapp1.0");
            
            return true;
        }
    }
}
