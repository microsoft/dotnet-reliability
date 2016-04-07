using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.File;
using Microsoft.WindowsAzure.Storage.Table;

namespace DumplingLib
{
    public class DumplingStorageAccount
    {
        public CloudStorageAccount StorageAccount { get; set; }
        public CloudBlobClient BlobClient { get; set; }
        public CloudTableClient TableClient { get; set; }

        public CloudFileClient FileClient { get; set; }

        public DumplingStorageAccount(string connection_string)
        {
            StorageAccount = CloudStorageAccount.Parse(connection_string);
           
            BlobClient = StorageAccount.CreateCloudBlobClient();
            TableClient = StorageAccount.CreateCloudTableClient();
            FileClient = StorageAccount.CreateCloudFileClient();
        }

    }
}
