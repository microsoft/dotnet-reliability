// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using DumplingLib;
using System.Diagnostics;

namespace dumplingDataWorker
{
    public class HelixEventProxy
    {
        private struct CountTable
        {
            public int Initial;
            public int Unscheduled;
            public int Waiting;
            public int Running;
            public int Finished;
        }

        private class CorrelationIdContext
        {
            public DateTimeOffset CreatedAt { get; private set; }
            public CountTable Table;
            public string CorrelationId { get; private set; }

            public CorrelationIdContext(string correlationId)
            {
                CreatedAt = DateTimeOffset.Now;
                CorrelationId = correlationId;
                Table = new CountTable();
            }
        }

        private static LinkedList<CorrelationIdContext> s_tracking = new LinkedList<CorrelationIdContext>();

        public static async Task TrackIfHelixId(string correlationId)
        {
            // query the helix api
            using (var client = new HttpClient())
            {
                if (await IsHelixCorrelationId(client, correlationId))
                {
                    var context = new CorrelationIdContext(correlationId);

                    s_tracking.AddLast(context);
                }
                else return;
            }
        }

        public static async Task PollTrackedHelixWorkItems()
        {
            using (var client = new HttpClient())
            {
                while (true)
                {
                    try
                    {
                        LinkedListNode<CorrelationIdContext> currentNode = s_tracking.First;
                        Trace.WriteLine($"Tracking {s_tracking.Count} nodes.");

                        while (currentNode != null)
                        {
                            var nextNode = currentNode.Next;
                            var context = currentNode.Value;

                            if (DateTimeOffset.Now.Subtract(context.CreatedAt).TotalHours > 36) // track for 36 hours
                            {
                                s_tracking.Remove(currentNode);
                            }
                            else
                            {
                                var details = await client.GetStringAsync($"https://helixview-stage.azurewebsites.net/api/jobs/{context.CorrelationId}/details");

                                var data = JObject.Parse(details);

                                FireHelixWorkItemProxyEventsForPath(data, "InitialWorkItemCount", ref context.Table.Initial);
                                FireHelixWorkItemProxyEventsForPath(data, "WorkItems.Unscheduled", ref context.Table.Unscheduled);
                                FireHelixWorkItemProxyEventsForPath(data, "WorkItems.Waiting", ref context.Table.Waiting);
                                FireHelixWorkItemProxyEventsForPath(data, "WorkItems.Running", ref context.Table.Running);
                                FireHelixWorkItemProxyEventsForPath(data, "WorkItems.Finished", ref context.Table.Finished);
                            }

                            currentNode = nextNode;
                        }

                        await Task.Delay(60000);
                    }
                    catch (Exception e) { Trace.WriteLine($"Exception: {e}"); }
                }
            }
        }

        /// <summary>
        /// Return true if this is a helix correlation id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private static async Task<bool> IsHelixCorrelationId(HttpClient client, string id)
        {
            try
            {
                var details = await client.GetStringAsync($"https://helixview-stage.azurewebsites.net/api/jobs/{id}/details");
                if (details != String.Empty)
                    return true;
            }
            catch { }

            Console.WriteLine("not a correlation id");
            return false;
        }

        private static void FireHelixWorkItemProxyEventsForPath(JObject data, string path, ref int current)
        {
            try
            {
                // fire off initial events
                switch (data.SelectToken(path).Type)
                {
                    case JTokenType.Integer:
                        for (; current < data.SelectToken(path).ToObject<int>(); current++)
                        {
                            DumplingEventHub.FireEvent(new HelixServiceProxyWorkItemsStatusEvent(path));
                        }
                        break;
                    default:
                        Console.WriteLine("count information type needs to be an int.");
                        break;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"could not fire helix work item events: {e}");
            }
        }
    }
}
