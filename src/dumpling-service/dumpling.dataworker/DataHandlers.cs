// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DumplingLib;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using triage.database;

namespace dumplingDataWorker
{
    public static class DataHandlers
    {
        public static async Task CheckIfHelixId(int dumplingId, string json_payload)
        {
            try
            {
                await HelixEventProxy.TrackIfHelixId(JObject.Parse(json_payload)["owner"].ToString());
            }
            catch (Exception e)
            {
                Trace.WriteLine($"Handler 'CheckIfHelixId' failed: {e}");
            }
        }

        /// <summary>
        /// This will take a dump analysis uri from the json_payload, serialize the analysis and store
        /// it in our SQL database.
        /// </summary>
        /// <param name="triage_payload"></param>
        /// <returns></returns>
        public static async Task SqlIndexTarget(int dumplingId, string triage_payload)
        {
            try
            {
                var triageData = JsonConvert.DeserializeObject<Dictionary<string, string>>(triage_payload);
                await TriageDb.UpdateDumpTriageInfo(dumplingId, triageData);
            }
            catch (Exception e)
            {
                Trace.WriteLine($"Handler 'SqlIndexTargetHandler' failed: {e}");
            }
        }
    }
}
