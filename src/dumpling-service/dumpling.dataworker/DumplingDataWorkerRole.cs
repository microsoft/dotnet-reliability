// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics;
using System.Net;
using System.Threading;
using Microsoft.WindowsAzure.ServiceRuntime;
using DumplingLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;


namespace dumplingDataWorker
{
    public class DumplingDataWorkerRole : RoleEntryPoint
    {
        public DumplingDataWorkerRole()
        {
            _dataHandler = new Dictionary<string, DataHandler>()
            {
                { "check_if_helix_worker", DataHandlers.CheckIfHelixId },
                { "sql_index_target", DataHandlers.SqlIndexTarget }
            };
        }

        public override void Run()
        {
            DumplingEventHub.FireEvent(new DataWorkerRunEvent()).Wait();
            Trace.WriteLine("Starting processing of messages");


            // start a background task that queries Helix for data.
            Task.Run(HelixEventProxy.PollTrackedHelixWorkItems);

            // initiates the message pump and callback is invoked for each message that is received, calling close on the client will stop the pump.
            DumplingDataWorkerQueueController.Queue.OnMessageAsync(async (receivedMessage) =>
                {
                    Trace.WriteLine("processing service bus message: " + receivedMessage.SequenceNumber.ToString());
                    try
                    {
                        // Process the message
                        if (receivedMessage.Properties.ContainsKey("data_type") && (receivedMessage.Properties["data_type"] is string))
                        {
                            var key = receivedMessage.Properties["data_type"] as string;
                            var dumplingId = receivedMessage.Properties["dumpling_id"] as string;
                            var body = receivedMessage.GetBody<Stream>();

                            await receivedMessage.CompleteAsync();

                            // TODO: Debug/Understand: the received message is disposed of after the await call returns. Is this a framework/runtime bug?
                            await DumplingEventHub.FireEvent(new DataWorkerMessageReceivedEvent());
                            using (var reader = new StreamReader(body))
                            {
                                await _dataHandler[key](int.Parse(dumplingId), await reader.ReadToEndAsync());
                            }

                            await DumplingEventHub.FireEvent(new DataWorkerCompletedMessageEvent());
                        }
                        else
                        {
                            Trace.WriteLine("message does not contain a 'data_type' property, or the 'data_type' property is not a string, and so we do not know how to route it. this message will be deadlettered.");
                            await receivedMessage.DeadLetterAsync();
                            await DumplingEventHub.FireEvent(new DataWorkerDeadLetterEvent());
                        }
                    }
                    catch (Exception e)
                    {
                        await DumplingEventHub.FireEvent(new DataWorkerExceptionEvent());
                        // Handle any message processing specific exceptions here
                        Trace.WriteLine($"all catching exception of doom.\n{e}");
                    }
                });

            _completedEvent.WaitOne();
        }

        public override bool OnStart()
        {
            // Before events or anything else can happen, we have to do this.
            NearbyConfig.RetrieveSecrets().Wait();
            DumplingEventHub.FireEvent(new DataWorkerStartEvent()).Wait();
            // Set the maximum number of concurrent connections 
            ServicePointManager.DefaultConnectionLimit = 12;

            return base.OnStart();
        }

        public override void OnStop()
        {
            DumplingEventHub.FireEvent(new DataWorkerStopEvent()).Wait();

            // Close the connection to Service Bus Queue
            DumplingDataWorkerQueueController.Queue.Close();
            _completedEvent.Set();
            base.OnStop();
        }

        #region privates

        private ManualResetEvent _completedEvent = new ManualResetEvent(false);

        private delegate Task DataHandler(int dumplingId, string json_payload);
        private Dictionary<string, DataHandler> _dataHandler;

        #endregion // privates
    }
}
