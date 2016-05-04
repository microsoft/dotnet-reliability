using HelixMonitoringService.Models;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dumps
{
    // Helper class for common CloudBlobStorage tasks
    public class BlobHelper
    {
        private CloudStorageAccount _storageAccount;
        private CloudBlobClient _blobClient;

        static CloudTable _jobs;
        static CloudTableClient _tableClient;

        static BlobHelper()
        {
            Initialize();
        }

        static bool Initialize()
        {
            var storageAccount = CloudStorageAccount.Parse(string.Format("DefaultEndpointsProtocol=https;AccountName={0};AccountKey={1}", Config.ConfigFile.BUILD_OPS_ACCOUNT_NAME, Config.ConfigFile.BUILD_OPS_KEY));
            _tableClient = storageAccount.CreateCloudTableClient();

            _jobs = _tableClient.GetTableReference(Config.ConfigFile.CORRELATIONS_TABLE_NAME);

            if (!_jobs.Exists())
            {
                Console.WriteLine($"Table {Config.ConfigFile.CORRELATIONS_TABLE_NAME} does not exist. Exiting without doing anything.");
                return false; // fail out.
            }

            return true; // continue safely.
        }

        private static List<HelixCorrelationEntry> GetWorkItemEntries(string targetedCorrelationId)
        {
            return (from wi in _jobs.CreateQuery<HelixCorrelationEntry>().Where(
                        wi => wi.RowKey == targetedCorrelationId)
                    select wi).ToList();
        }

        /// <summary>
        /// Searches results for an item with a container name. 
        /// </summary>
        /// <param name="correlationId"></param>
        /// <returns></returns>
        private static string GetContainerForCorrelationId(string correlationId)
        {
            string containerName = String.Empty;

            var item = GetWorkItemEntries(correlationId).FirstOrDefault();
            if (item != null)
            {
                containerName = item.GetResultsContainerName(); // we expect the container name to follow a particular convention.
                if (containerName == null || string.IsNullOrEmpty(containerName))
                {
                    Console.WriteLine("Can't discern the results container name from the correlation-id. Exiting without doing anything.");
                    return string.Empty;
                }
                else
                {
                    //Console.WriteLine($"Reading from container: {containerName}");
                    return containerName;
                }
            }
            else
            {
                Console.WriteLine("I can't find a work item id for this correlation id. I am going to quit without doing anything.");
                return string.Empty;
            }
        }

        public static readonly SharedAccessBlobPolicy SasReadWrite = new SharedAccessBlobPolicy
        {
            SharedAccessExpiryTime = DateTime.UtcNow.AddYears(1),
            Permissions = SharedAccessBlobPermissions.Write | SharedAccessBlobPermissions.Read
        };

        public BlobHelper(string connectionString)
        {
            _storageAccount = CreateStorageAccountFromConnectionString(connectionString);
            _blobClient = _storageAccount.CreateCloudBlobClient();
        }
        
        public async Task<CloudBlobContainer> GetContainerAsync(string containerName)
        {
            CloudBlobContainer container = _blobClient.GetContainerReference(containerName);
            await container.CreateIfNotExistsAsync();
            return container;
        }

        public async Task<CloudBlobContainer> UploadFileAsync(byte[] contents, string containerName, string blobName)
        {
            int originalSize = contents.Length;
            int pages = contents.Length / 512;
            if (contents.Length % 512 > 0)
            {
                pages++;
            }
            Array.Resize(ref contents, pages * 512);

            CloudBlobContainer container = await GetContainerAsync(containerName);

            CloudPageBlob pageBlob = container.GetPageBlobReference(blobName);
            // TODO: AzCopy somehow makes these files the right size, not rounded up to next 512 bytes size.  
            //       Figure out what it does differently.
            await pageBlob.CreateAsync(512 * pages);
            await pageBlob.UploadFromByteArrayAsync(contents, 0, contents.Length);
            return container;
        }

        public async Task<CloudBlobContainer> UploadFileAsync(Stream fileStream, string containerName, string blobPath)
        {
            byte[] file = new byte[fileStream.Length];
            fileStream.Read(file, 0, (int)fileStream.Length);
            return await UploadFileAsync(file, containerName, blobPath);
        }

        public async Task<CloudBlobContainer> UploadFileAsync(string path, string containerName, string blobPath)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException("Please make sure file exists at " + path);
            }

            return await UploadFileAsync(File.ReadAllBytes(path), containerName, blobPath);
        }
        public async Task<CloudBlobContainer> UploadListAsync(string json, string containerName, string blobPath)
        {
            return await UploadFileAsync(Encoding.UTF8.GetBytes(json), containerName, blobPath);
        }

        public async Task<Uri> GetDumpURL(string correlationId, string testName)
        {
            var containerName       = GetContainerForCorrelationId(correlationId);  
            
            if(string.IsNullOrEmpty(containerName))
            {
                return null;
            }
             
            var container           = await GetContainerAsync(containerName);
            var containerSasToken   = container.GetSharedAccessSignature(SasReadWrite);

            var blobRef = container.ListBlobs(useFlatBlobListing: true).Where(x => (x as CloudBlob).Name.Contains(correlationId) && (x as CloudBlob).Name.Contains(testName + ".zip")).FirstOrDefault();

            if (blobRef != null && blobRef is CloudBlob)
            {
                if (await (blobRef as CloudBlob).ExistsAsync())
                {
                    var dumpUri = $"{blobRef.StorageUri.PrimaryUri}{containerSasToken}";

                    return new Uri(dumpUri);
                }
            }

            throw new Exception($"Could not locate a blob for correlation id: {correlationId} and test named {testName + ".zip"}");
        }

        #region Private Methods
        private static CloudStorageAccount CreateStorageAccountFromConnectionString(string storageConnectionString)
        {
            CloudStorageAccount storageAccount;
            try
            {
                storageAccount = CloudStorageAccount.Parse(storageConnectionString);
            }
            catch (FormatException)
            {
                Trace.WriteLine("Invalid storage account information provided. Please confirm the AccountName and AccountKey are valid in the app.config file - then restart the sample.");
                throw;
            }
            catch (ArgumentException)
            {
                Trace.WriteLine("Invalid storage account information provided. Please confirm the AccountName and AccountKey are valid in the app.config file - then restart the sample.");
                throw;
            }

            return storageAccount;
        }
        #endregion

    }
}
