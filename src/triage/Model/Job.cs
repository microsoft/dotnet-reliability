using System.Collections.Generic;

namespace HelixMonitoringService.Models
{
    public class Job
    {
            public string Command { get; set; }
            public List<string> CorrelationPayloadUris { get; set; }
            public string PayloadUri { get; set; }
            public string WorkItemId { get; set; }
    }
}
