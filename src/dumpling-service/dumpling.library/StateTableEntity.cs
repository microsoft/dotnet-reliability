// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;

namespace DumplingLib
{
    public class StateTableEntity : TableEntity
    {
        public string State { get; set; } = "uploading";
        public string OriginatingOS { get; set; } = String.Empty;
        public string Symbols_uri { get; set; } = String.Empty;
        public string DumpRelics_uri { get; set; } = String.Empty;
        public string Results_uri { get; set; } = String.Empty;
        public IEnumerable<LogMessage> Messages { get; set; } = new List<LogMessage>();

        public StateTableEntity()
        {
            // needed or else it throws.
        }

        public StateTableEntity(StateTableIdentifier id)
        {
            this.PartitionKey = id.Owner;
            this.RowKey = id.DumplingId;
        }

        public StateTableEntity(string owner, string dumpling_id)
        {
            this.PartitionKey = owner;
            this.RowKey = dumpling_id;
        }
    }
}
