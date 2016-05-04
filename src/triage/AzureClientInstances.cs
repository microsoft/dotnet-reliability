using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dumps
{
    public static class AzureClientInstances
    {
        public static CloudStorageAccount BuildOpsStorageAccount { get; private set; }
        public static CloudTableClient BuildOpsTableClient { get; private set; }
        public static CloudTable JobTable { get; private set; }

        public static BlobHelper ResultsDropHelper { get; private set; }
        public static BlobHelper RAPDropBlobHelper { get; private set; }


        public static bool Initialize()
        {
            BuildOpsStorageAccount = CloudStorageAccount.Parse(string.Format(Config.CONNECTION_STRING_FORMAT, Config.ConfigFile.BUILD_OPS_ACCOUNT_NAME, Config.ConfigFile.BUILD_OPS_KEY));
            BuildOpsTableClient = BuildOpsStorageAccount.CreateCloudTableClient();
            JobTable = BuildOpsTableClient.GetTableReference(Config.ConfigFile.CORRELATIONS_TABLE_NAME);

            if (!JobTable.Exists())
            {
                Console.WriteLine($"Table {Config.ConfigFile.CORRELATIONS_TABLE_NAME} does not exist. Exiting without doing anything.");
                return false; // fail out.
            }

            ResultsDropHelper = new BlobHelper(Config.ConfigFile.JOB_RESULTS_CONNECTION_STRING);
            RAPDropBlobHelper = new BlobHelper(String.Format(Config.CONNECTION_STRING_FORMAT, Config.ConfigFile.RAP_STORAGE_ACCOUNT_NAME, Config.ConfigFile.RAP_STORAGE_ACCOUNT_KEY));
            return true;
        }
    }
}
