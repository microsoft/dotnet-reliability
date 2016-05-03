using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DumplingLib
{
    public static class DumplingDataWorkerQueueController
    {
        public static QueueClient Queue { get; private set; }

        static DumplingDataWorkerQueueController()
        {
            Queue = QueueClient.CreateFromConnectionString(NearbyConfig.Settings["dumpling-service bus connection string"], NearbyConfig.Settings["dumpling-service data-worker queue path"]);
        }


        /// <summary>
        /// submit a json event to our data worker queue. 
        /// </summary>
        /// <param name="json_payload"></param>
        /// <returns></returns>
        public static async Task SendWork(string payload_type, string json_payload)
        {
            var msg = new BrokeredMessage(json_payload);
            msg.Properties.Add("data_type", payload_type);
            

            await Queue.SendAsync(msg);
        }
    }
}
