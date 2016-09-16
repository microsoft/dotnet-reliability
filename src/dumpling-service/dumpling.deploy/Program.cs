// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DumplingLib;
using System.IO;

namespace dumplingService
{
    public static class DumplingDeployment
    {
        private delegate void CreationDelegate();

        /// <summary>
        /// This topic is used to route messages to the correct analysis machine for analytics.
        /// </summary>
        /// <returns></returns>
        private static async Task DeployAnalysisTopic(TextWriter writer, bool recreate = false)
        {
            var SubscriptionsAndConditions = new Dictionary<string, SqlFilter>()
            {
                { "ubuntu", new SqlFilter("target_os = 'ubuntu'") },
                { "centos", new SqlFilter("target_os = 'centos' OR target_os = 'rhel' OR target_os = 'redhat'") },
                { "windows", new SqlFilter("target_os = 'windows'") }
            };
            // check if the topic exists, then create it.
            var nsManager = NamespaceManager.CreateFromConnectionString(NearbyConfig.Settings["dumpling-service bus connection string"]);

            var topicName = NearbyConfig.Settings["dumpling-service analysis topic path"];

            CreationDelegate createTopic = async () =>
            {
                var topicDescription = new TopicDescription(topicName);

                await nsManager.CreateTopicAsync(topicDescription);
                writer.WriteLine("topic created");
            };

            var exists = await nsManager.TopicExistsAsync(topicName);
            if (exists && recreate)
            {
                writer.WriteLine($"topic {topicName} deleting it");
                await nsManager.DeleteTopicAsync(topicName);
                createTopic();
            }
            else if (!exists)
            {
                createTopic();
            }
            else
            {
                writer.WriteLine($"'{topicName}' exists and we are not recreating.");
            }

            // Topic is created, now we'll do the subscriptions.
            foreach (var subscription in SubscriptionsAndConditions)
            {
                // I use this to avoid repeating myself in the createSubscription(...) calls below.
                CreationDelegate createSubscription = async () =>
                {
                    SubscriptionDescription description = new SubscriptionDescription(topicName, subscription.Key);
                    // When an item is retrieved from the subscription, a machine owns that lock, and then must clean up after it.
                    // if the lock expires that machine will not be able to delete the message and it is possible for another machine to take it.
                    description.LockDuration = TimeSpan.FromMinutes(5);

                    // after 16 hours, dead letter any messages in the queue. In my experience this is necessary
                    // because sometimes messages can get 'stuck' if they take too long to evaluate. We won't just delete
                    // them though, we'll dead letter them.
                    description.DefaultMessageTimeToLive = TimeSpan.FromHours(16);
                    description.EnableDeadLetteringOnMessageExpiration = true;

                    await nsManager.CreateSubscriptionAsync(description, subscription.Value);
                    writer.WriteLine($"created subscription: {subscription.Key}");
                };

                // next we'll (re)create the subscriptions
                exists = await nsManager.SubscriptionExistsAsync(topicName, subscription.Key);
                if (exists && recreate)
                {
                    writer.WriteLine($"'{subscription.Key}' already exists; deleting");

                    await nsManager.DeleteSubscriptionAsync(topicName, subscription.Key);

                    createSubscription();
                }
                else if (!exists)
                {
                    createSubscription();
                }
                else
                {
                    writer.WriteLine($"'{subscription.Key}' already exists and we're not recreating.");
                }
            }
        }

        private static DumplingStorageAccount s_storage = new DumplingStorageAccount(NearbyConfig.Settings["dumpling-service storage account connection string"]);

        /// <summary>
        /// Deploys the state table that tracks dumps as they move through the dumpling infrastructure and get processed. 
        /// </summary>
        /// <param name="recreate"></param>
        /// <returns></returns>
        private static async Task DeployStateTable(TextWriter writer, bool recreate = false)
        {
            var tableName = NearbyConfig.Settings["dumpling-service state table name"];

            writer.WriteLine($"creating state table {tableName}");
            var table = s_storage.TableClient.GetTableReference(tableName);
            var exists = await table.ExistsAsync();

            if (exists && recreate)
            {
                writer.WriteLine($"state table {tableName} exists, and it was specified that we want to recreate the table; deleting table");
                await table.DeleteAsync();
            }

            await table.CreateIfNotExistsAsync();
            writer.WriteLine($"state table '{tableName}' created or existed already.");
        }

        private static async Task DeployDataWorkerQueue(TextWriter writer, bool recreate = false)
        {
            // "dumpling-service data-worker queue path"
            // "dumpling-service bus connection string"
            var dataworkerQueuePath = NearbyConfig.Settings["dumpling-service data-worker queue path"];
            var servicebusConnString = NearbyConfig.Settings["dumpling-service bus connection string"];

            NamespaceManager manager = NamespaceManager.CreateFromConnectionString(servicebusConnString);

            CreationDelegate createQueue = async () =>
            {
                writer.WriteLine("creating new dataworker queue");
                await manager.CreateQueueAsync(dataworkerQueuePath);
            };

            var exists = await manager.QueueExistsAsync(dataworkerQueuePath);
            if (exists && recreate)
            {
                writer.WriteLine("dataworker queue exists; deleting current instance");
                await manager.DeleteQueueAsync(dataworkerQueuePath);

                createQueue();
            }
            else if (!exists)
            {
                createQueue();
            }
            else
            {
                writer.WriteLine($"'{dataworkerQueuePath}' exists already and we're not recreating");
            }
        }

        /// <summary>
        /// Deploys the event hub that sends messages to stream analytics, and then eventually to PowerBI.
        /// </summary>
        /// <returns></returns>
        private static async Task DeployEventHub(TextWriter writer, bool recreate = false)
        {
            writer.WriteLine("deploying event hub");
            var servicebusConnString = NearbyConfig.Settings["dumpling-service eventhub connection string"];
            var eventhubPath = NearbyConfig.Settings["dumpling-service eventhub path"];

            NamespaceManager manager = NamespaceManager.CreateFromConnectionString(servicebusConnString);
            var exists = await manager.EventHubExistsAsync(eventhubPath);
            if (exists && recreate)
            {
                writer.WriteLine("event hub exists and we want to recreate it; deleting current instance.");
            }

            await manager.CreateEventHubIfNotExistsAsync(eventhubPath);

            writer.WriteLine($"event hub '{eventhubPath}' created or existed already.");
        }


        private static void Main(string[] args)
        {
            using (var writer = Console.Out/*new StreamWriter(@"C:\temp\dumpling_deploy.log")*/)
            {
                writer.WriteLine("deploying dumpling service");
                var deployTasks = new[] {
                    DeployStateTable(writer),
                    DeployAnalysisTopic(writer),
                    DeployDataWorkerQueue(writer),
                    DeployEventHub(writer),
                    // StreamAnalytics is deployed via the Azure UI.
                    // classic webrole - deployed by an azure build process
                    // classic worker role - deployed by an azure build process
                };

                Task.WaitAll(deployTasks);
                writer.WriteLine("deployment completed successfully");
            }
        }
    }
}
