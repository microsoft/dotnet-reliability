// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace AzureBlobsFileUpload
{
    internal class AzureBlobStorageMultipartProvider : MultipartFileStreamProvider
    {
        private CloudBlobContainer _container;

        public AzureBlobStorageMultipartProvider(CloudBlobContainer container, string rootPath)
            : base(rootPath)
        {
            Initialize(container);
        }

        public AzureBlobStorageMultipartProvider(CloudBlobContainer container, string rootPath, int bufferSize)
            : base(rootPath, bufferSize)
        {
            Initialize(container);
        }

        public Collection<AzureBlobInfo> AzureBlobs { get; private set; }

        public override async Task ExecutePostProcessingAsync()
        {
            // Upload the files asynchronously to azure blob storage and remove them from local disk when done
            foreach (MultipartFileData fileData in FileData)
            {
                // Get the blob name from the Content-Disposition header if present
                string blobName = GetBlobName(fileData);

                // Retrieve reference to a blob
                CloudBlockBlob blob = _container.GetBlockBlobReference(blobName);

                SharedAccessBlobPolicy sasConstraints = new SharedAccessBlobPolicy();
                sasConstraints.SharedAccessStartTime = DateTime.UtcNow;
                sasConstraints.SharedAccessExpiryTime = DateTime.UtcNow.AddYears(1);
                sasConstraints.Permissions = SharedAccessBlobPermissions.Read;

                var blobToken = blob.GetSharedAccessSignature(sasConstraints);

                // Pick content type if present
                blob.Properties.ContentType = fileData.Headers.ContentType != null ?
                    fileData.Headers.ContentType.ToString() : "application/octet-stream";

                // Upload content to blob storage
                using (FileStream fStream = new FileStream(fileData.LocalFileName, FileMode.Open, FileAccess.Read, FileShare.Read, BufferSize, useAsync: true))
                {
                    await blob.UploadFromStreamAsync(fStream);
                }

                // Delete local file
                File.Delete(fileData.LocalFileName);

                AzureBlobs.Add(new AzureBlobInfo
                {
                    ContentType = blob.Properties.ContentType,
                    Name = blob.Name,
                    Size = blob.Properties.Length,
                    Location = blob.Uri.AbsoluteUri + blobToken,
                });
            }

            await base.ExecutePostProcessingAsync();
        }

        private void Initialize(CloudBlobContainer container)
        {
            if (container == null)
            {
                throw new ArgumentNullException("container");
            }

            _container = container;
            AzureBlobs = new Collection<AzureBlobInfo>();
        }

        private static string GetBlobName(MultipartFileData fileData)
        {
            string blobName = null;
            ContentDispositionHeaderValue contentDisposition = fileData.Headers.ContentDisposition;
            if (contentDisposition != null)
            {
                try
                {
                    blobName = Path.GetFileName(contentDisposition.FileName.Trim('"'));
                }
                catch
                { }
            }

            return blobName ?? Path.GetFileName(fileData.LocalFileName);
        }
    }
}