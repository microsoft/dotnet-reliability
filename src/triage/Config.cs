using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dumps
{
    public class ConfigFile
    {
        public string RAP_STORAGE_ACCOUNT_NAME;
        public string RAP_STORAGE_ACCOUNT_KEY;
        public string SERVICE_BUS_NAMESPACE;
        public string SERVICE_BUS_KEY;
        public string CORRELATIONS_TABLE_NAME;
        public string BUILD_OPS_KEY;
        public string BUILD_OPS_ACCOUNT_NAME;
        public string JOB_RESULTS_CONNECTION_STRING;
        public string ANALYSIS_RESULTS_CONNECTION_STRING;
    }

    /// <summary>
    /// All secrets + keys need to be here for ease of manageability. This will be dealt with in a safe way soon.
    /// </summary>
    static class Config
    {
        public const string CONNECTION_STRING_FORMAT = @"DefaultEndpointsProtocol=https;AccountName={0};AccountKey={1}";
        public static ConfigFile ConfigFile { get; private set; }

        public static readonly SharedAccessBlobPolicy SasReadWrite = new SharedAccessBlobPolicy
        {
            SharedAccessExpiryTime = DateTime.UtcNow.AddYears(1),
            Permissions = SharedAccessBlobPermissions.Write | SharedAccessBlobPermissions.Read
        };

        static Config()
        {
            ConfigFile = JsonConvert.DeserializeObject<ConfigFile>(File.ReadAllText(@"config.json"));
        }
    }
}
