using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;

namespace DumplingLib
{
    public class StateTableEntity : TableEntity
    {
        public string State { get; set; } = "uploading";
        public string Symbols_uri { get; set; } = String.Empty;
        public string DumpRelics_uri{ get; set; } = String.Empty;
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
