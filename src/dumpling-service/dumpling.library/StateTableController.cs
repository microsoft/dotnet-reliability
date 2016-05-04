// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.WindowsAzure.Storage.Table;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DumplingLib
{
    public struct StateTableIdentifier
    {
        public string Owner { get; set; }
        public string DumplingId { get; set; }
    }

    public static class StateTableController
    {
        private static DumplingStorageAccount Storage { get; set; } = new DumplingStorageAccount(NearbyConfig.Settings["dumpling-service storage account connection string"]);
        private static CloudTable StateTable { get; set; }

        static StateTableController()
        {
            StateTable = Storage.TableClient.GetTableReference(NearbyConfig.Settings["dumpling-service state table name"]);
            StateTable.CreateIfNotExists();
        }

        private static async Task<StateTableEntity> GetEntry(StateTableIdentifier identifier)
        {
            // retrieve item with identifier
            var result = await StateTable.ExecuteAsync(TableOperation.Retrieve<StateTableEntity>(identifier.Owner, identifier.DumplingId));
            return (result.Result as StateTableEntity);
        }

        public static async Task SetState(StateTableIdentifier identifier, string state)
        {
            var entry = await GetEntry(identifier);
            // set the state
            entry.State = state;

            // AddOrUpdate
            await AddOrUpdateStateEntry(entry);
        }

        public static async Task<string> GetState(StateTableIdentifier identifier)
        {
            var entry = await GetEntry(identifier);
            return entry.State;
        }

        public static async Task AddOrUpdateStateEntry(StateTableEntity newEntity)
        {
            await StateTable.ExecuteAsync(TableOperation.InsertOrReplace(newEntity));
        }

        public static async Task UpdateLogMessages(StateTableIdentifier identifier, IEnumerable<LogMessage> messages)
        {
            var entry = await GetEntry(identifier);

            entry.Messages = messages;

            await AddOrUpdateStateEntry(entry);
        }

        public static async Task<string> GetResultsUri(StateTableIdentifier id)
        {
            var response = await StateTable.ExecuteAsync(TableOperation.Retrieve<StateTableEntity>(id.Owner, id.DumplingId));
            var entity = ((StateTableEntity)response.Result);

            return entity.Results_uri;
        }

        public static async Task<string> GetDumpUri(StateTableIdentifier id)
        {
            var response = await StateTable.ExecuteAsync(TableOperation.Retrieve<StateTableEntity>(id.Owner, id.DumplingId));
            var entity = ((StateTableEntity)response.Result);

            return entity.DumpRelics_uri;
        }
    }
}
