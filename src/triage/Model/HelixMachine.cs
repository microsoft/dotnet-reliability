using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace HelixMonitoringService.Models
{
    //    A heartbeat.
    //9/15/2015 11:04:46 AM: { "QueueId": "Windows", "ComputerName": "dnbws1000640034", "Type": "Heartbeat"}

    class HelixMachine : TableEntity
    {
        public DateTime LastHeartbeat
        {
            get; set;
        }
        public string CurrentWorkItemId { get; set; }

        public HelixMachine(string queueId, string machineName) 
        {
            PartitionKey = queueId;
            RowKey = machineName;
        }

        public HelixMachine() 
        {
        }
    }
}
