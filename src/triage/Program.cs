// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using triage.database;

namespace triage
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            TriageDb.Init("TriageDbContext");

            List<Dump> dumps = new List<Dump>();
            foreach (var triagePath in Directory.EnumerateFiles(@"h:\temp\triagedata"))
            {

                Dump d = new Dump() { DisplayName = Path.GetFileNameWithoutExtension(triagePath), DumpPath = triagePath, Origin = "TestData", DumpTime = DateTime.Now };

                dumps.Add(d);

                d.DumpId = TriageDb.AddDumpAsync(d).GetAwaiter().GetResult();
            }


            foreach(var d in dumps)
            {
                try
                {
                    Dictionary<string, string> dict = DeserializeTriageJson(d.DumpPath);

                    var triageData = TriageData.FromDictionary(dict);

                    TriageDb.UpdateDumpTriageInfo(d.DumpId, triageData).GetAwaiter().GetResult();
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Dump {d.DisplayName} failed to update triage info.");
                    Console.WriteLine(e);
                }
            }
        }


        public static Dictionary<string,string> DeserializeTriageJson(string path)
        {
            Dictionary<string, string> config = null;

            // Deserialize the RunConfiguration
            JsonSerializer serializer = JsonSerializer.CreateDefault();

            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                using (StreamReader reader = new StreamReader(fs))
                {
                    JsonTextReader jReader = new JsonTextReader(reader);

                    // Call the Deserialize method to restore the object's state.
                    config = serializer.Deserialize<Dictionary<string, string>>(jReader);
                }
            }

            return config;
        }
    }
}
