// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.ServiceBus.Messaging;
using Newtonsoft.Json;
using System.Configuration;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace DumplingLib
{
    public static class AnalysisTopicController
    {
        private static TopicClient Client { get; set; }

        static AnalysisTopicController()
        {
            Client = TopicClient.CreateFromConnectionString(NearbyConfig.Settings["dumpling-service bus connection string"], NearbyConfig.Settings["dumpling-service analysis topic path"]);
        }

        public static async Task EnqueueAnalysisWork(string owner, string dumpling_id, string target_os, string dump_uri)
        {
            // using anonymous type
            var serializedMessage = (JsonConvert.SerializeObject(new
            {
                owner = owner,
                dumpling_id = dumpling_id,
                target_os = target_os,
                dump_uri = dump_uri
            }, Formatting.None));

            var ascii_data = Encoding.ASCII.GetBytes(serializedMessage);
            var ascii_data_size = Encoding.ASCII.GetByteCount(serializedMessage);

            /* Note that we have to use an ASCII stream when serializing, otherwise C# has type information prepended and this will
            mess up any non-C# receivers of the message (namely our Python script) */
            using (var stream = new MemoryStream())
            {
                var message = new BrokeredMessage(stream);

                // the topic routes the message based on this property
                message.Properties["target_os"] = target_os;

                await stream.WriteAsync(ascii_data, 0, ascii_data_size);
                stream.Position = 0; // position needs to be rewound to 0 before we can send, otherwise we think that the message has been partially consumed.

                message.ContentType = "application/json";

                Client.Send(message);
            }
        }
    }
}
