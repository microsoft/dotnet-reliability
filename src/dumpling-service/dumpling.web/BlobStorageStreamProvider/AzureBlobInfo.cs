// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureBlobsFileUpload
{
    /// <summary>
    /// The <see cref="AzureBlobInfo"/> class contains information about the blobs found in
    /// the blob store.
    /// </summary>
    internal class AzureBlobInfo
    {
        /// <summary>
        /// The name without path of the file stored in the blob store.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The size of the blob.
        /// </summary>
        public long Size { get; set; }

        /// <summary>
        /// The content type of the blob.
        /// </summary>
        public string ContentType { get; set; }

        /// <summary>
        /// The absolute URI by which the blob can be retrieved from the store.
        /// </summary>
        public string Location { get; set; }
    }
}
