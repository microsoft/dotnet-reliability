using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace HelixMonitoringService.Models
{
    class HelixWorkItem : TableEntity
    {
        public DateTime Updated { get; set; }

        public WorkItemState? State { get; set; }

        public string LogUri { get; set; }

        public string LogModule { get; set; }

        public string ResultsXmlUri { get; set; }

        public string ErrorLogUri { get; set; }

        public int? ExitCode { get; set; }

        public string LastMessageType { get; set; }

        public string FriendlyName { get; set; }

        public bool? Passed { get; set; }

        public int? Timeout { get; set; }

        public HelixWorkItem(string correlationId, string workItemId)
        {
            PartitionKey = correlationId;
            RowKey = workItemId;
        }
        public HelixWorkItem() { }
    }

    enum WorkItemState
    {
        Queued,
        Started,
        TimedOut,
        Finished
    }

}
