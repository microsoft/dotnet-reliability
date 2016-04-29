using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace stress.codegen
{
    [Serializable]
    public class ProjectJsonInfo
    {
        public Dictionary<string, string> dependencies;

        public Dictionary<string, Dictionary<string, string>> frameworks;
        public Dictionary<string, Dictionary<string, string>> runtimes;

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

        public static ProjectJsonInfo FromFile(string path)
        {
            ProjectJsonInfo obj = null;

            //if a project.json file exists next to the test binary load it
            if (File.Exists(path))
            {
                var serializer = JsonSerializer.CreateDefault();

                using (var srdr = new StreamReader(File.OpenRead(path)))
                {
                    var jrdr = new JsonTextReader(srdr);

                    obj = serializer.Deserialize<ProjectJsonInfo>(jrdr);
                }
            }

            return obj;
        }

        public static ProjectJsonInfo MergeToLatest(IEnumerable<ProjectJsonInfo> projectJsons)
        {
            var merged = new ProjectJsonInfo();

            merged.dependencies = new Dictionary<string, string>();

            merged.frameworks = new Dictionary<string, Dictionary<string, string>>();
            merged.runtimes = new Dictionary<string, Dictionary<string, string>>();

            foreach (var pjson in projectJsons)
            {
                if (pjson.dependencies != null)
                {
                    foreach (var depend in pjson.dependencies)
                    {
                        //if the dependency is not present in the merged dependencies OR the version in the current pjson is a greater than the one in merged 
                        //if string.Compare returns > 0 then depend.Value is greater than the one in merged, this should mean a later version 
                        if (!merged.dependencies.ContainsKey(depend.Key) || (string.Compare(merged.dependencies[depend.Key], depend.Value, StringComparison.InvariantCultureIgnoreCase) > 0))
                        {
                            merged.dependencies[depend.Key] = depend.Value;
                        }
                    }
                }

                if (pjson.frameworks != null)
                {
                    foreach (var fx in pjson.frameworks)
                    {
                        //if the framework is not in the merged dependencies
                        if (!merged.frameworks.ContainsKey(fx.Key))
                        {
                            merged.frameworks[fx.Key] = fx.Value;
                        }
                    }
                }

                if (pjson.runtimes != null)
                {
                    foreach (var rt in pjson.runtimes)
                    {
                        //if the framework is not in the merged dependencies
                        if (!merged.runtimes.ContainsKey(rt.Key))
                        {
                            merged.runtimes[rt.Key] = rt.Value;
                        }
                    }
                }
            }

            return merged;
        }
    }
}
