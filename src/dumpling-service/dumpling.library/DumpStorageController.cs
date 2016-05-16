
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using Microsoft.WindowsAzure.Storage.Blob;

namespace DumplingLib
{
    public static class DumpStorageController
    {
        public static DumplingStorageAccount Storage { get; private set; } = new DumplingStorageAccount(NearbyConfig.Settings["dumpling-service storage account connection string"]);

        static DumpStorageController()
        {
        }

        public static CloudBlobContainer GetContainerForOwner(string owner)
        {
            return Storage.BlobClient.GetContainerReference(owner);
        }

        public static string GetContainerSasUri(CloudBlobContainer container)
        {
            // Set the expiry time and permissions for the container.
            // In this case no start time is specified, so the shared access signature becomes valid immediately.

            SharedAccessBlobPolicy sasConstraints = new SharedAccessBlobPolicy();
            sasConstraints.SharedAccessExpiryTime = DateTime.UtcNow.AddYears(1);
            sasConstraints.Permissions = SharedAccessBlobPermissions.Write | SharedAccessBlobPermissions.List;

            // Generate the shared access signature on the container, setting the constraints directly on the signature.
            string sasContainerToken = container.GetSharedAccessSignature(sasConstraints);

            // Return the URI string for the container, including the SAS token.
            return container.Uri + sasContainerToken;
        }
    }
}
