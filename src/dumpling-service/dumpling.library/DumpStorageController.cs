using Microsoft.WindowsAzure.Storage.Blob;

namespace DumplingLib
{

    public static class DumpStorageController
    {
        public static DumplingStorageAccount Storage { get;  private set; } = new DumplingStorageAccount(NearbyConfig.Settings["dumpling-service storage account connection string"]);

        static DumpStorageController()
        {

        }

        public static CloudBlobContainer GetContainerForOwner(string owner)
        {
            return Storage.BlobClient.GetContainerReference(owner);
        }
        
    }
}
