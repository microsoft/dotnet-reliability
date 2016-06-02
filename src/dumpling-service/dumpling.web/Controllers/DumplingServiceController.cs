// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using AzureBlobsFileUpload;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using triage.database;
using DumplingLib;

namespace dumplingWeb.Controllers
{
    /// <summary>
    /// This is the entry point for dumpling services.
    /// </summary>
    public class DumplingServiceController : ApiController
    {
        private const string _version = "0.1";

        /// <summary>
        /// returns the current status of a dumpling.
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="dumplingid"></param>
        /// <returns></returns>
        // api/dumpling/get-status
        //[Route(Name = "get-status")]
        [Route("dumpling/status/{owner}/{dumplingid}")]
        [HttpGet]
        public async Task<string> GetStatus(string owner, string dumplingid)
        {
            await DumplingEventHub.FireEvent(new WebAPIGetStatusEvent());

            return await StateTableController.GetState(new StateTableIdentifier() { Owner = owner, DumplingId = dumplingid });
        }

        [Route("dumpling/redumpling/{owner}/{dumplingid}/{os}")]
        [HttpPut]
        public async Task ReDumpling(string owner, int dumplingid, string os)
        {
            var identifier = new StateTableIdentifier()
            {
                Owner = owner,
                DumplingId = dumplingid.ToString()
            };


            var entity = await StateTableController.GetEntry(identifier);

            entity.OriginatingOS = os;
            await StateTableController.AddOrUpdateStateEntry(entity);

            await StateTableController.SetState(identifier, "enqueued");
            await AnalysisTopicController.EnqueueAnalysisWork(owner, dumplingid.ToString(), os, entity.DumpRelics_uri);
        }

        /// <summary>
        /// This is just here to test service availability. 
        /// 
        /// 
        /// curl http://[dotnetrp].net/dumpling/test/hi/im/(yourname)
        /// </summary>
        /// <param name="name"></param>
        /// <returns>"Hi (name). I am the dumpling service."</returns>
        [Route("dumpling/test/hi/im/{name}")]
        [HttpGet]
        public async Task<string> SayHi(string name)
        {
            await DumplingEventHub.FireEvent(new WebAPIGreetingEvent());

            return $"Hi {name}. I am v.{_version} of the dumpling service.";
        }

        /// <summary>
        /// Returns dump url for a particular owner and dumpling id. Throws exception if it doesn't exist.
        /// </summary>
        /// <param name="owner">user identifier</param>
        /// <param name="dumplingid">the dumpling id that was returned from /dumpling/storage/(owner)/(targetos)</param>
        /// <returns></returns>
        [Route("dumpling/store/dump/{owner}/{dumplingid}")]
        [HttpGet]
        public async Task<string> GetDumpUrl(string owner, string dumplingid)
        {
            return await StateTableController.GetDumpUri(new StateTableIdentifier()
            {
                Owner = owner,
                DumplingId = dumplingid
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="owner">user identifier</param>
        /// <param name="dumplingid">the dumpling id that was returned from /dumpling/storage/(owner)/(targetos)</param>
        /// <returns></returns>
        [Route("dumpling/store/results/{owner}/{dumplingid}")]
        [HttpGet]
        public async Task<string> GetResultsUrl(string owner, string dumplingid)
        {
            return await StateTableController.GetResultsUri(new StateTableIdentifier()
            {
                Owner = owner,
                DumplingId = dumplingid
            });
        }

        /// <summary>
        /// index must be == 0 and filesize must be <= int.MaxValue
        /// At the moment a user will need to upload a single chunk, and then after that they can proceed to upload additional chunks to 
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="targetos"></param>
        /// <param name="index"></param>
        /// <param name="filesize"></param>
        /// <returns></returns>
        [Route("dumpling/store/chunk/{owner}/{targetos}/{index}/{filesize}")]
        [HttpPost]
        public async Task<string> PostDumpChunk(string owner, string targetos, int index, ulong filesize, string displayName = "")
        {
            if (index > 0 || filesize > int.MaxValue)
            {
                throw new NotSupportedException("We do not support chunked files yet, and the file must be <= 2GB or more specifically, int.MaxValue");
            }

            if (!Request.Content.IsMimeMultipartContent("form-data"))
            {
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            }


            await DumplingEventHub.FireEvent(new WebAPIStartUploadChunkEvent());

            /* Handle Upload */
            // Get or create the blob container
            var container = DumpStorageController.GetContainerForOwner(owner);
            await container.CreateIfNotExistsAsync();

            // Create a AzureBlobStorageMultipartProvider and process the request
            var path = Path.GetTempPath();
            AzureBlobStorageMultipartProvider streamProvider = new AzureBlobStorageMultipartProvider(container, path);

            await Request.Content.ReadAsMultipartAsync<AzureBlobStorageMultipartProvider>(streamProvider);

            await DumplingEventHub.FireEvent(new WebAPIFinishedUploadChunkEvent());

            /* Meta data handling */
            var dump_uri = streamProvider.AzureBlobs.First().Location;

            if (displayName == string.Empty)
            {
                displayName = Path.GetFileNameWithoutExtension(Path.GetRandomFileName());
            }

            int dumplingid = await TriageDb.AddDumpAsync(new Dump()
            {
                DumpTime = DateTime.Now,
                DumpPath = dump_uri,
                DisplayName = displayName,
                Origin = owner
            });

            StateTableIdentifier id = new StateTableIdentifier()
            {
                Owner = owner,
                DumplingId = dumplingid.ToString(),
            };

            await AnalysisTopicController.EnqueueAnalysisWork(owner, dumplingid.ToString(), targetos, dump_uri);

            // let the dumpling services know about our dumpling
            await StateTableController.AddOrUpdateStateEntry(new StateTableEntity(id)
            {
                DumpRelics_uri = dump_uri,
                OriginatingOS = targetos
            });

            await StateTableController.SetState(id, "enqueued");

            await DumplingDataWorkerQueueController.SendWork("check_if_helix_worker", $"{{ \"owner\": \"{owner}\" }}");

            // Return result from storing content in the blob container
            return dumplingid.ToString();
        }
    }
}
