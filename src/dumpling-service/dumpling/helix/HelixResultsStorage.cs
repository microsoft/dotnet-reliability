// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Linq;

namespace dumpling.helix
{
    /// <summary>
    /// Deprecated. This is here just to help us keep things moving until infrastructure can get setup all the way.
    /// </summary>
    public static class HelixResultsStorage
    {
        private const string _connectionString = "";

        public static CloudStorageAccount StorageAccount { get; set; }
        public static CloudBlobClient BlobClient { get; set; }

        static HelixResultsStorage()
        {
            StorageAccount = CloudStorageAccount.Parse(_connectionString);
            BlobClient = StorageAccount.CreateCloudBlobClient();
        }

        #region public

        public static IEnumerable<string> EnumerateStressContainers()
        {
            return BlobClient.ListContainers("", ContainerListingDetails.All).Where(x => x.Name.Contains("stress")).Select(x => x.Name);
        }

        public static IEnumerable<string> GetCorrelationIdsInContainer(string container)
        {
            var selectedContainer = BlobClient.GetContainerReference(container);
            return selectedContainer.ListBlobs().Select(x => (x as CloudBlobDirectory).Prefix.Substring(0, (x as CloudBlobDirectory).Prefix.IndexOf('/')));
        }

        public static IEnumerable<string> ListZipFiles(string container, string correlationId)
        {
            var selectedContainer = BlobClient.GetContainerReference(container);

            var sasSig = selectedContainer.GetSharedAccessSignature(HelixResultsStorage.s_sasRead);

            return selectedContainer.ListBlobs("/" + correlationId, true).Where(x => (x as CloudBlob).Name.EndsWith(".zip")).Select(x => x.Uri.ToString() + sasSig);
        }
        #endregion

        #region policy
        private static readonly SharedAccessBlobPolicy s_sasRead = new SharedAccessBlobPolicy
        {
            SharedAccessExpiryTime = DateTime.UtcNow.AddYears(1),
            Permissions = SharedAccessBlobPermissions.Read
        };
        #endregion  

    }
}
