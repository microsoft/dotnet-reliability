using Microsoft.ServiceBus.Messaging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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

        public static async Task FireEvent(string json_payload)
        {
            Trace.WriteLine($"event {json_payload}");
            // TODO: Confirm that the string is a json_payload?
            await Client.SendAsync(new EventData(ASCIIEncoding.ASCII.GetBytes(json_payload)));
        }
    }
}
