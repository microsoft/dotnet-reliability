// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.ServiceBus.Messaging;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace DumplingLib
{
    public static class DumplingEventHub
    {
        private static EventHubClient Client { get; set; }

        static DumplingEventHub()
        {
            Client = EventHubClient.CreateFromConnectionString(NearbyConfig.Settings["dumpling-service eventhub connection string"], NearbyConfig.Settings["dumpling-service eventhub path"]);
        }

        public static async Task FireEvent(CommonEvent json_payload)
        {
            Trace.WriteLine($"event {json_payload}");

            await Client.SendAsync(new EventData(ASCIIEncoding.ASCII.GetBytes(JsonConvert.SerializeObject(json_payload))));
        }
    }
}
