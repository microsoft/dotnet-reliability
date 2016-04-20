// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace stress.codegen
{
    public class TestAssemblyLoader : MarshalByRefObject
    {
        private TestAssemblyInfo _assembly;


        public string AssemblyPath { get; set; }

        public string LoadError { get; set; }

        public string[] HintPaths { get; set; }

        public IEnumerable<string> SearchPaths
        {
            get
            {
                return new string[] { Path.GetDirectoryName(this.AssemblyPath) }.Concat(this.HintPaths);
            }
        }
        

        public bool Load(string assemblyPath, string[] hintPaths)
        {
            this.AssemblyPath = assemblyPath;

            this.HintPaths = hintPaths;

            this.LoadError = null;

            AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += IsoDomain_ReflectionOnlyAssemblyResolve;

            try
            {
                _assembly = new TestAssemblyInfo() { Assembly = Assembly.ReflectionOnlyLoadFrom(this.AssemblyPath), ReferenceInfo = new TestReferenceInfo(), PackageInfo = LoadPackageRefs(this.AssemblyPath) };
                
                foreach (var refName in _assembly.Assembly.GetReferencedAssemblies())
                {
                    this.AddTestReferenceAssembly(refName);
                }
            }
            catch (Exception e)
            {
                this.LoadError = e.ToString();
            }

            return this.LoadError == null;
        }

        private static ProjectJsonInfo LoadPackageRefs(string assemblyPath)
        {
            var projJsonPath = Path.Combine(Path.GetDirectoryName(assemblyPath), "project.json");

            return ProjectJsonInfo.FromFile(projJsonPath);
        }

        public void AddTestReferenceAssembly(AssemblyName refAssm)
        {
            //if the assembly isn't a banned reference
            if (!s_bannedRefs.Contains(refAssm.Name.ToLowerInvariant()))
            { 
                //try to find the assembly
                string assmPath = FindReferenceAssemblyInPaths(refAssm.Name, this.SearchPaths);

                if (assmPath != null && File.Exists(assmPath))
                {
                    //if the assembly is available in the packages add to the framework reference list for legacy projects
                    //(in new projects these will be added through project.json refs)
                    if (_assembly.PackageInfo.dependencies.ContainsKey(refAssm.Name) && _assembly.PackageInfo.dependencies[refAssm.Name].StartsWith(refAssm.Version.ToString()))
                    {
                        _assembly.ReferenceInfo.FrameworkReferences.Add(new AssemblyReference() { Path = assmPath, Version = refAssm.Version.ToString() });
                    }
                    else
                    {
                        //if the assembly lives next to the test it is a test specific dependancy not a framework dependency            
                        _assembly.ReferenceInfo.ReferencedAssemblies.Add(new AssemblyReference() { Path = assmPath, Version = refAssm.Version.ToString() });
                    }
                }
            }
        }

        private static string FindReferenceAssemblyInPaths(string assmName, IEnumerable<string> paths)
        {
            string assmPath = null;
            string assmDllFile = assmName + ".dll";
            string assmExeFile = assmName + ".exe";

            foreach(var searchPath in paths)
            {
                assmPath = Directory.EnumerateFiles(searchPath, assmDllFile, SearchOption.AllDirectories).FirstOrDefault() ?? Directory.EnumerateFiles(searchPath, assmExeFile, SearchOption.AllDirectories).FirstOrDefault();

                if(assmPath != null)
                {
                    break;
                }
            }

            return assmPath;
        }

        public UnitTestInfo[] GetTests<TDiscoverer>()
            where TDiscoverer : ITestDiscoverer, new()
        {
            try
            {
                var discoverer = new TDiscoverer();

                return discoverer.GetTests(_assembly);
            }
            catch (Exception e)
            {
                this.LoadError = (this.LoadError ?? string.Empty) + e.ToString();
            }

            return new UnitTestInfo[] { };
        }

        private Assembly IsoDomain_ReflectionOnlyAssemblyResolve(object sender, ResolveEventArgs args)
        {
            Assembly assm = null;
            if (!s_loaded.TryGetValue(args.Name, out assm))
            {
                //try to find the assembly
                string assmPath = FindReferenceAssemblyInPaths(new AssemblyName(args.Name).Name, this.SearchPaths);
                
                try
                {
                    assm = Assembly.ReflectionOnlyLoadFrom(assmPath);
                    
                    s_loaded[args.Name] = assm;

                    return assm;
                }
                catch
                {
                }
            }
            return assm;
        }
        
        private void AddTestAssemblyReference(Assembly assembly)
        {
            if (assembly.GetName().Name.ToLowerInvariant() != "mscorlib")
            {
                _assembly.ReferenceInfo.ReferencedAssemblies.Add(new AssemblyReference() { Path = assembly.Location, Version = assembly.GetName().Version.ToString() });
            }
        }
        
        internal static Dictionary<string, string> g_ResolvedAssemblies = new Dictionary<string, string>();
        private static Dictionary<string, Assembly> s_loaded = new Dictionary<string, Assembly>();
        private static HashSet<string> s_knownTestRefs = new HashSet<string>(new string[] { "System.Xml.RW.XmlReaderLib" });

        private readonly static string[] s_bannedRefs = { "mscorelib" };
    }
}
