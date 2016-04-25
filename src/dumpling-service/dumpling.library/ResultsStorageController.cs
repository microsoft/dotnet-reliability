using Microsoft.WindowsAzure.Storage.Blob;

namespace DumplingLib
{
    public static class ResultsStorageController
    {
        
        public static DumplingStorageAccount Storage { get; private set; }
        

        static ResultsStorageController()
        {
            Storage = new DumplingStorageAccount(NearbyConfig.Settings["dumpling-service storage account connection string"]);
        }

        public static CloudBlobContainer GetContainerForOwner(string owner)
        {
            return Storage.BlobClient.GetContainerReference(owner);
        }
    }
}
