// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace triage.database
{
    public class Dump
    {
        public Dump()
        {
            this.Threads = new HashSet<Thread>();
        }

        public int DumpId { get; set; }

        public string DisplayName { get; set; }

        public string DumpPath { get; set; }

        public string Origin { get; set; }

        public int? BucketId { get; set; }

        [Required]
        [Index]
        public DateTime DumpTime { get; set; }

        public virtual ICollection<Thread> Threads { get; set; }

        public virtual ICollection<Property> Properties { get; set; }

        [ForeignKey("BucketId")]
        public virtual Bucket Bucket { get; set; }

        public void LoadTriageData(IDictionary<string, string> triageData)
        {
            if (triageData == null) throw new ArgumentNullException("triageData");

            //copy into new dictionary so that we can modify
            triageData = new Dictionary<string, string>(triageData);

            string val = null;

            if(triageData.TryGetValue("FAILURE_HASH", out val))
            {
                this.Bucket = new Bucket() { Name = val };

                triageData.Remove("FAILURE_HASH");
            }
            
            if(triageData.TryGetValue("ALL_THREADS", out val))
            {
                var threadData = triageData["ALL_THREADS"];

                DeserializeAllThreads(threadData);

                triageData.Remove("ALL_THREADS");
            }
        }

        private static List<Thread> DeserializeAllThreads(string allThreadsValue)
        {
            List<Thread> allThreads = new List<Thread>();

            var serializer = JsonSerializer.CreateDefault();

            var txtReader = new StringReader(allThreadsValue);

            var jsonReader = new JsonTextReader(txtReader);

            var threadObjs = serializer.Deserialize<List<ThreadDeserializerObj>>(jsonReader);

            for(int i = 0; i < threadObjs.Count; i++)
            {
                Thread t = new Thread()
                {
                    CurrentThread = (i == 0),
                    Number = threadObjs[i].Index,
                    OSId = threadObjs[i].Osid
                };

                for(int j = 0; j < threadObjs[i].Frames.Count; j++)
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
