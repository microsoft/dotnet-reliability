using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace HelixMonitoringService.Models
{
    class HelixCorrelationEntry : TableEntity
    {
        public int StartedWorkItems { get; set; }
        public int QueuedWorkItems { get; set; }
        public int FinishedWorkItems { get; set; }
        public DateTime Created { get; set; }
        public bool Completed { get; set; }
        public int InitialWorkItemCount { get; set; }
        public string Branch { get; set; }
        public string Creator { get; set; }
        public string Product { get; set; }
        public string BuildNumber { get; set; }
        public string Architecture { get; set; }
        public string Configuration { get; set; }
        public string EndTime { get; set; }
        public string JobList { get; set; }

        public HelixCorrelationEntry(string queueId, string correlationId) 
        {
            PartitionKey = queueId;
            RowKey = correlationId;
            StartedWorkItems = 0;
            QueuedWorkItems = 0;
            FinishedWorkItems = 0;
            InitialWorkItemCount = 0;
            Created = DateTime.UtcNow;
        }

        public HelixCorrelationEntry()
        {
        }

        public string GetResultsContainerName()
        {
            Uri jobUri = new Uri(JobList);
            var pq = jobUri?.PathAndQuery?.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

            return pq[0];
        }
    }
}
