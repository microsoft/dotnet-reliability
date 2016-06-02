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
    [Serializable]
    public class ProjectJsonDependencyInfo
    {
        public Dictionary<string, string> dependencies;

        public Dictionary<string, Dictionary<string, string[]>> frameworks;

        public Dictionary<string, Dictionary<string, string>> runtimes;

        public ProjectJsonDependencyInfo()
        {
            dependencies = new Dictionary<string, string>();

            frameworks = new Dictionary<string, Dictionary<string, string[]>>();
            frameworks["netcoreapp1.0"] = new Dictionary<string, string[]>() { { "imports", new string[] { "dnxcore50", "portable-net45+win8" }  } };

            runtimes = new Dictionary<string, Dictionary<string, string>>()
            {
                { "win", new Dictionary<string, string>() },
                { "win7-x64", new Dictionary<string, string>() },
                { "win7-x86", new Dictionary<string, string>() },
                { "ubuntu.14.04-x64", new Dictionary<string, string>() },
                { "osx.10.10-x64", new Dictionary<string, string>() },
                { "centos.7-x64", new Dictionary<string, string>() },
                { "rhel.7-x64", new Dictionary<string, string>() },
                { "debian.8.2-x64", new Dictionary<string, string>() },
            };
        }

        public void ToFile(string path)
        {
            // Serialize the RunConfiguration
            JsonSerializer serializer = JsonSerializer.CreateDefault();

            using (FileStream fs = new FileStream(path, FileMode.Create))
            {
                using (StreamWriter writer = new StreamWriter(fs))
                {
                    serializer.Serialize(writer, this);
                }
            }
        }

        public static ProjectJsonDependencyInfo FromFile(string path)
        {
            ProjectJsonDependencyInfo dependInfo = new ProjectJsonDependencyInfo();
            
            //if a project.json file exists next to the test binary load it
            if (File.Exists(path))
            {
                var serializer = JsonSerializer.CreateDefault();

                using (var srdr = new StreamReader(File.OpenRead(path)))
                {
                    var jrdr = new JsonTextReader(srdr);

                    var obj = serializer.Deserialize<JObject>(jrdr);
                    
                    foreach(var prop in FindAllDependencyProperties(obj))
                    {
                        dependInfo.dependencies[prop.Key] = prop.Value;
                    }
                }
            }

            return dependInfo;
        }

        public static IEnumerable<KeyValuePair<string, string>> FindAllDependencyProperties(JObject projectJsonObj)
        {
            return projectJsonObj
                .Descendants()
                .OfType<JProperty>()
                .Where(property => property.Name == "dependencies")
                .Select(property => property.Value)
                .SelectMany(o => o.Children<JProperty>())
                .Where(p => !p.Name.Contains("TargetingPack"))
                .Select(p => GetDependencyPair(p))
                .Where(p => !string.IsNullOrEmpty(p.Value));
        }

        public static KeyValuePair<string, string> GetDependencyPair(JProperty dependProp)
        {
            string dependencyVersion;

            if (dependProp.Value is JObject)
            {
                dependencyVersion = dependProp.Value["version"]?.Value<string>();
            }
            else if (dependProp.Value is JValue)
            {
                dependencyVersion = dependProp.Value.ToObject<string>();
            }
            else
            {
                throw new ArgumentException("Unrecognized dependency element");
            }

            return new KeyValuePair<string, string>(dependProp.Name, dependencyVersion);
        }

        public static ProjectJsonDependencyInfo MergeToLatest(IEnumerable<ProjectJsonDependencyInfo> projectJsons, string oldprerelease = null, string newprerelease = null)
        {
            var merged = new ProjectJsonDependencyInfo();

            merged.dependencies = new Dictionary<string, string>();
            
            foreach (var pjson in projectJsons)
            {
                if (pjson.dependencies != null)
                {
                    foreach (var depend in pjson.dependencies)
                    {
                        var depVer = ComplexVersion.Parse(depend.Value);
                        
                        if(newprerelease != null && depVer.Prerelease == oldprerelease)
                        {
                            depVer.Prerelease = newprerelease;
                        }

                        //if the dependency is not present in the merged dependencies OR the version in the current pjson is a greater than the one in merged 
                        //if string.Compare returns > 0 then depend.Value is greater than the one in merged, this should mean a later version 
                        if (!merged.dependencies.ContainsKey(depend.Key) || (ComplexVersion.Parse(merged.dependencies[depend.Key]) < depVer))
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
