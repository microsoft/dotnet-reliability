using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace stress.codegen
{
    public class ProjectJsonDependencyInfo
    {
        public JObject dependencies;

        public JObject frameworks;

        public JObject runtimes;

        public JObject supports;

        public ProjectJsonDependencyInfo()
        {
            dependencies = new JObject();

            frameworks = new JObject();

            runtimes = new JObject();

            supports = new JObject();

            supports.Add("coreFx.Test.netcoreapp1.0", new JObject());
        }

        public void ToFile(string path)
        {
            File.WriteAllText(path, JsonConvert.SerializeObject(this));
        }

        public static ProjectJsonDependencyInfo FromFile(string path)
        {
            ProjectJsonDependencyInfo dependInfo = null;
            //if a project.json file exists next to the test binary load it
            if (File.Exists(path))
            {
                var data = JObject.Parse(File.ReadAllText(path));

                (data["dependencies"] as JObject)?.Property("test-runtime")?.Remove();

                dependInfo = data.ToObject<ProjectJsonDependencyInfo>();
            }

            return dependInfo;
        }

        public static JEnumerable<JProperty> FetchPrimaryDependenciesProperties(JObject projectJsonObj)
        {
            return projectJsonObj["dependencies"].Children<JProperty>();
        }



        public static ProjectJsonDependencyInfo MergeToLatest(IEnumerable<ProjectJsonDependencyInfo> projectJsons, string oldprerelease = null, string newprerelease = null)
        {
            var merged = new ProjectJsonDependencyInfo();

            merged.dependencies = new JObject();

            foreach (var pjson in projectJsons)
            {
                if (pjson.dependencies != null)
                {
                    foreach (var depend in pjson.dependencies)
                    {
                        var depVer = ComplexVersion.Parse(depend.Value.Value<string>());

                        if (newprerelease != null && depVer.Prerelease != null && depVer.Prerelease == oldprerelease)
                        {
                            depVer.Prerelease = newprerelease;
                        }

                        //if the dependency is not present in the merged dependencies OR the version in the current pjson is a greater than the one in merged 
                        //if string.Compare returns > 0 then depend.Value is greater than the one in merged, this should mean a later version 
                        if (merged.dependencies[depend.Key] == null || (ComplexVersion.Parse(merged.dependencies[depend.Key].Value<string>()) < depVer))
                        {
                            merged.dependencies[depend.Key] = depVer.ToString();
                        }
                    }
                }
            }

            return merged;
        }
        
        private class ComplexVersion
        {
            public Version StrongVersion;
            public string Prerelease;

            public static ComplexVersion Parse(string verStr)
            {
                ComplexVersion ver = null;

                if(verStr != null)
                {
                    var splitVerStr = verStr.Split(new char[] { '-' }, 2);

                    var strongVerStr = splitVerStr[0];

                    var prerelVerStr = splitVerStr.Length > 1 ? splitVerStr[1] : null;

                    Version strongVer;

                    if(Version.TryParse(strongVerStr, out strongVer))
                    {
                        ver = new ComplexVersion() { StrongVersion = strongVer, Prerelease = prerelVerStr };
                    }
                }

                return ver;
            }

            public static bool operator >(ComplexVersion x, ComplexVersion y)
            {
                return (x.StrongVersion > y.StrongVersion) || (y.Prerelease == null || ((x.Prerelease != null) && (string.Compare(x.Prerelease, y.Prerelease, StringComparison.InvariantCultureIgnoreCase) > 0)));
            }

            public static bool operator <(ComplexVersion x, ComplexVersion y)
            {
                return (x.StrongVersion < y.StrongVersion) || (x.Prerelease == null && y.Prerelease != null) || ((y.Prerelease != null) && (string.Compare(x.Prerelease, y.Prerelease, StringComparison.InvariantCultureIgnoreCase) < 0));
            }

            public override string ToString()
            {
                string verStr = this.StrongVersion.ToString();

                if (!string.IsNullOrEmpty(this.Prerelease))
                {
                    verStr += "-" + this.Prerelease;
                }

                return verStr;
            }
        }
    }
}
