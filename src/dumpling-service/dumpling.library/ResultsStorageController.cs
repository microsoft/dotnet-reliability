// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

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
