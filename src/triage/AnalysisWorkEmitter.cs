using dumps.Model;
using HelixMonitoringService.Models;
using Microsoft.ServiceBus.Messaging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dumps
{
    class AnalysisWorkEmitter
    {
        static JobMetadataHelper _jobMetadata;

        public string StateId { get; private set; }
        private string _targetOS = string.Empty;

        private AnalysisWorkEmitter() {  }
        public AnalysisWorkEmitter(string[] correlationIds, string targetOS)
        {
            // this stateId is going to be used to create a corresponding container entry in our results storage.
            StateId = Guid.NewGuid().ToString();
            _targetOS = targetOS;
            GatherAndSend(correlationIds);
        }

        private void GatherAndSend(string[] correlationIds)
        {
            // creates a container for our results to go in to.
            // additionally, we will store some metadata associated with the container to help track progress.
            _jobMetadata = new JobMetadataHelper(AzureClientInstances.RAPDropBlobHelper, StateId);
            var container = _jobMetadata.BoundContainer;
            
            _jobMetadata.SetStartTimeToNow();

            foreach (var id in correlationIds)
            {
                var containerName = GetResultsContainerNameForCorrelationId(id);

                if (string.IsNullOrEmpty(containerName)) throw new Exception("Can't locate container name.");

                var dumpList = GatherListOfDumpsFromContainer(containerName, id);
                
                if (dumpList.Count == 0)
                {
                    Console.WriteLine($"There are no .zip files in the container {containerName}.");
                    return;
                }
                else
                {
                    SendMessage(dumpList, StateId, dumpList.Count, container);
                    Console.WriteLine($"queued up {dumpList.Count} zips.");
                }
            }

        }

        private List<HelixCorrelationEntry> GetWorkItemEntries(string targetedCorrelationId)
        {
            return (from wi in AzureClientInstances.JobTable.CreateQuery<HelixCorrelationEntry>().Where(
                        wi => wi.RowKey == targetedCorrelationId)
                    select wi).ToList();
        }

        private List<string> GatherListOfDumpsFromContainer(string containerName, string correlationId)
        {
            CloudBlobContainer dropContainer = AzureClientInstances.ResultsDropHelper.GetContainerAsync(containerName).Result;

            // Grab the SAS token for the container
            var containerSasToken = dropContainer.GetSharedAccessSignature(Config.SasReadWrite);
            var dumpList = new List<string>();
            var blobs = dropContainer.ListBlobs(prefix: correlationId, useFlatBlobListing: true);

            // we assume that every zip file contains a core dump inside, so we create a list of the zip file uris.
            foreach (var blob in blobs)
            {
                if (blob.Uri.ToString().EndsWith(".zip"))
                {
                    var dumpUri = $"{blob.StorageUri.PrimaryUri}{containerSasToken}";

                    dumpList.Add(dumpUri);
                }
            }

            return dumpList;
        }

        private void SendMessage(List<string> dumpList, string state, int takeCount, CloudBlobContainer currentContainer)
        {
            // We've got a list, time to queue it up. So we will construct a job message for our queue.
            DumpMessage dumpMessage = new DumpMessage()
            {
                target_os = _targetOS,
                state = state,
                result_payload_uris = dumpList.Take(takeCount).ToArray()
            };

            var serializedDumpMessage = JsonConvert.SerializeObject(dumpMessage, Formatting.None);

            /* Note that we have to use an ASCII stream when serializing, otherwise C# has type information prepended and this will
            mess up any non-C# receivers of the message (namely our Python script) */
            MemoryStream stream = new MemoryStream();

            var data = Encoding.ASCII.GetBytes(serializedDumpMessage);
            var count = Encoding.ASCII.GetByteCount(serializedDumpMessage);

            var message = new BrokeredMessage(stream);

            message.Properties["target_os"] = _targetOS;

            stream.Write(data, 0, count);
            stream.Position = 0; // position needs to be 0 before we can send, otherwise we think that the message has been partially consumed.

            message.ContentType = "application/json";

            var queue = TopicClient.CreateFromConnectionString(@"Endpoint=sb://clr-rap.servicebus.windows.net/;SharedAccessKeyName=RootPolicy;SharedAccessKey=EodAFNj6LHr+GV8+4VySSVeE2+pMCEy6Wz/vjD7Au3k=", "dopplertasktopic");
            Console.WriteLine($"message is {message.Size} bytes (Azure's max message size is 256kb).");

            queue.Send(message);

            _jobMetadata.IncrementJobCount(takeCount);
        }

        private string GetResultsContainerNameForCorrelationId(string correlationId)
        {
            string containerName = String.Empty;

            var item = GetWorkItemEntries(correlationId).FirstOrDefault();
            if (item != null)
            {
                containerName = item.GetResultsContainerName(); // we expect the container name to follow a particular convention.
                if (containerName == null || string.IsNullOrEmpty(containerName))
                {
                    Console.WriteLine("Can't discern the results container name from the correlation-id. Exiting without doing anything.");
                    return string.Empty;
                }
                else
                {
                    Console.WriteLine($"Reading from container: {containerName}");
                    return containerName;
                }
            }
            else
            {
                Console.WriteLine("I can't find a work item id for this correlation id. I am going to quit without doing anything.");
                return string.Empty;
            }
        }


    }
}
