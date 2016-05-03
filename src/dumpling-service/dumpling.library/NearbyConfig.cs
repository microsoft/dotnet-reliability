using Newtonsoft.Json;
using System.Collections.Generic;
using System.Configuration;
using System.IO;

namespace DumplingLib
{
    public static class NearbyConfig
    {
        public static Dictionary<string, string> Settings { get; set; }

        static NearbyConfig()
        {
            //Settings = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText("dumplinglib_config.json"));
            var config = "{\n  \"dumpling-service storage account connection string\": \"DefaultEndpointsProtocol=https;AccountName=rapreqs;AccountKey=1cY5r8ci4OF5Pv3B5xe00e8+CeOjcGKFNC9J2FZ2VhB9Hxo1boESYFCygHBasLiX6KoC29kKxKrFlynkRa3GkQ==;BlobEndpoint=https://rapreqs.blob.core.windows.net/;TableEndpoint=https://rapreqs.table.core.windows.net/;QueueEndpoint=https://rapreqs.queue.core.windows.net/;FileEndpoint=https://rapreqs.file.core.windows.net/\",\n  \"dumpling-service state table name\": \"dumplingstates\",\n  \"dumpling-service bus connection string\": \"Endpoint=sb://clr-rap.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=FBjznuImz/X8JsbHaDrzkT9eO7sf4eVKmgR5TKgka+o=\",\n  \"dumpling-service analysis topic path\": \"dumplingtopic\",\n  \"dumpling-service data-worker queue path\": \"dataworkerqueue\",\n  \"dumpling-service eventhub connection string\": \"Endpoint=sb://rapevents.servicebus.windows.net/;SharedAccessKeyName=dumproot;SharedAccessKey=rwulZ2jC6g2Ler549Y0zDHH1+AQ9c3/CvnPQO+0z8vc=\",\n  \"dumpling-service eventhub path\": \"dumplinghub\"\n}\n";

            Settings = JsonConvert.DeserializeObject<Dictionary<string, string>>(config);
        }
    }
}
