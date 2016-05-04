using dumps;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace RetrieveAnalysis
{
    /// <summary>
    /// Really inefficient class for just retrieving/sorting over all of our analysis in blob storage.
    /// Read only class. Meaning that it is mostly private except to 'see' information inside.
    /// </summary>
    class AnalysisStorage
    {
        string _currentContainerName = string.Empty;

        BlobHelper _analysisResults = new BlobHelper(Config.ConfigFile.ANALYSIS_RESULTS_CONNECTION_STRING);
        BlobHelper _runResults = new BlobHelper(Config.ConfigFile.JOB_RESULTS_CONNECTION_STRING);

        Dictionary<string, Dictionary<string, string>> _results = new Dictionary<string, Dictionary<string, string>>();
        public AnalysisStorage(string state)
        {
            _currentContainerName = state;
            GatherAllBlobs().Wait();
        }
        private AnalysisStorage() {  } // NO.
        /// <summary>
        /// Parses data in to key-value pairs and stores them in the dictionary named _results.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="data"></param>
        private void ParseData(string id, string data)
        {
            if(string.IsNullOrWhiteSpace(data))
            {
                return;
            }
            
            var lines = data.Trim().Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            _results.Add(id, new Dictionary<string, string>());

            int currentIndex = 0;
            do
            {
                StringBuilder valueBuilder = new StringBuilder();
                var key = lines[currentIndex++];

                if(!key.EndsWith(":"))
                {
                    Console.WriteLine("WARNING: line expected to end with ':' but does not. This may be a sign of a problem");
                }
                else
                {
                    key = key.TrimEnd(new[] { ':' });
                }

                while(currentIndex < lines.Count() && !string.IsNullOrWhiteSpace(lines[currentIndex]))
                {
                    valueBuilder.AppendLine(lines[currentIndex++]);
                }

                _results[id].Add(key, valueBuilder.ToString());
            } while (++currentIndex < lines.Count());
            
        }

        /// <summary>
        /// Walks *ALL* of the blobs in the container. Not optimized in anyway. It will then analyze the data inside and then parse it and 
        /// the results are stored in our dictionary.
        /// 
        /// </summary>
        /// <returns></returns>
        private async Task GatherAllBlobs()
        {
            var container = await _analysisResults.GetContainerAsync(_currentContainerName);

            using (WebClient client = new WebClient())
            {
                // figure out correlation id, and test exe
                foreach (var blob in container.ListBlobs(useFlatBlobListing:true))
                {
                    // bucket analysis
                    if (blob is CloudBlob)
                    {
                        var cblob = blob as CloudBlob;
                        using (MemoryStream stream = new MemoryStream())
                        {
                            await cblob.DownloadToStreamAsync(stream);
                            stream.Position = 0; // reset stream position so that we can read.
                            using (StreamReader reader = new StreamReader(stream))
                            {
                                ParseData(cblob.Name, await reader.ReadToEndAsync());
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// For sanity
        /// </summary>
        /// <param name="key"></param>
        public void DumpAllKey(string key)
        {
            foreach (var result in _results)
            {
                Console.WriteLine(result.Key);
                foreach (var item in result.Value)
                {
                    if (item.Key == key)
                    {
                        Console.WriteLine($"\t{item.Key} = {item.Value}");
                    }
                }
            }
        }

        /// <summary>
        /// Formats the output by grouping on the provided key. "FAULT_SYMBOL" is the default.
        /// THIS IS VERY FRAGILE. THERE ARE A LOT OF ASSUMPTIONS BEHIND IT. HOWEVER THIS WILL BE REVISITED WHEN WE 
        /// SET UP OUR OWN STORAGE (instead of using helixjobresults to hold some information).
        /// </summary>
        /// <param name="key"></param>
        public async Task<string> GroupOnKey(string key = "FAILURE_HASH")
        {
            var relevantResults = from x in _results
                          where x.Value.ContainsKey(key)
                          select new { Id = x.Key, Key = key, Value = x.Value[key], Store = x.Value };


            var groups = relevantResults.GroupBy(x => x.Value);

            List<object> groupList = new List<object>();

            foreach (var group in groups)
            {
                // ID: {0: analysisId}/{1: helixcorrelationId}/{2: helixjobid}/{3: testname}
                // TODO: so slow

                Console.WriteLine($"{group.Key} ({group.Count()} results.)");

                foreach (var result in group.OrderByDescending(x => x.Id.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries)[3]))
                {
                    var id = result.Id.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                    var correlationId = id[1];

                    var dumpUrl = await _runResults.GetDumpURL(correlationId, id[3]);
                    if (dumpUrl == null)
                        continue;
                    Console.WriteLine($"\tid: {id[3]} download: {dumpUrl}");
                    result.Store.Add("DUMP_URI", dumpUrl.ToString());
                    result.Store.Add("HELIX_CORRELATION_ID", correlationId);
                    groupList.Add(new { groupSymbol = group.Key.Trim(), testName = id[3], store = result.Store });
                }
            }

            var data = JsonConvert.SerializeObject(groupList);

            // the data.json can be copied in to a javascript file for easy viewing.
            //File.WriteAllText(@"data.json", data);
            return data;
        }
    }
}
