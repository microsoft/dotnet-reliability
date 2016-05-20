using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace triage.database
{
    public class TriageData
    {
        public int? BucketId { get { return this.Bucket?.BucketId; } }
        public Bucket Bucket { get; set; }

        public IList<Thread> Threads { get; set; } = new List<Thread>();

        public IList<Property> Properties { get; set; } = new List<Property>();

        public static TriageData FromDictionary(Dictionary<string, string> triageData)
        {
            if (triageData == null) throw new ArgumentNullException("triageData");

            var data = new TriageData();

            data.LoadFromStringDictionary(triageData);

            return data;
        }

        private void LoadFromStringDictionary(Dictionary<string, string> triageData)
        {
            //copy into new dictionary so that we can modify
            triageData = new Dictionary<string, string>(triageData);

            string val = null;

            //FAILURE_HASH corresponds to the bucket the dump belongs in
            if (triageData.TryGetValue("FAILURE_HASH", out val))
            {
                this.Bucket = new Bucket() { Name = val };

                triageData.Remove("FAILURE_HASH");
            }

            //ALL_THREADS corresponds to the json searialized thread data in the triage data
            if (triageData.TryGetValue("ALL_THREADS", out val))
            {
                var threadData = triageData["ALL_THREADS"];

                var threads = DeserializeAllThreads(threadData);
                
                foreach (var t in threads)
                {
                    this.Threads.Add(t);
                }

                triageData.Remove("ALL_THREADS");
            }
            
            //store the remaining properties
            foreach (var kvp in triageData)
            {
                this.Properties.Add(new Property() { Name = kvp.Key, Value = kvp.Value });
            }
        }


        private static List<Thread> DeserializeAllThreads(string allThreadsValue)
        {
            List<Thread> allThreads = new List<Thread>();

            var serializer = JsonSerializer.CreateDefault();

            var txtReader = new StringReader(allThreadsValue);

            var jsonReader = new JsonTextReader(txtReader);

            var threadObjs = serializer.Deserialize<List<ThreadDeserializerObj>>(jsonReader);

            for (int i = 0; i < threadObjs.Count; i++)
            {
                Thread t = new Thread()
                {
                    CurrentThread = (i == 0),
                    Number = threadObjs[i].Index,
                    OSId = threadObjs[i].Osid
                };

                for (int j = 0; j < threadObjs[i].Frames.Count; j++)
                {
                    var splitFrame = threadObjs[i].Frames[j].Split(new char[] { '!' }, 2);

                    if (splitFrame.Length == 2)
                    {
                        t.Frames.Add(new Frame() { Index = j, Module = new Module() { Name = splitFrame[0] }, Routine = new Routine() { Name = splitFrame[1] } });
                    }
                }

                allThreads.Add(t);
            }

            return allThreads;
        }

        private class ThreadDeserializerObj
        {
            public string Osid = null;
            public int Index = 0;
            public List<string> Frames = null;
        }
    }
}
