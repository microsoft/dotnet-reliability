using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace DumplingLib
{
    public static class NearbyConfig
    {
        public static Dictionary<string, string> Settings { get; set; }

        static NearbyConfig()
        {
            Settings = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText("dumplinglib_config.json"));
        }
    }
}
