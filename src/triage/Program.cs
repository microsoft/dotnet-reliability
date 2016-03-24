using dumps.Model;
using HelixMonitoringService.Models;
using Microsoft.ServiceBus.Messaging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.RetryPolicies;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dumps
{

    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("USE:");
                Console.WriteLine("args[0] == semi-colon delimited set of correlation ids.");
                Console.WriteLine("args[1] == target os. { ubuntu | centos }");

                return;
            }
 
            // initialize dependent azure services.
            if(!AzureClientInstances.Initialize())
            {
                Console.WriteLine("Could not initialize important Azure variables. Closing without doing anything.");
                return;
            }

            
            // get correlation ids
            var workIds = args[0].Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

            // emit our work.
            var workEmitter = new AnalysisWorkEmitter(workIds, args[1]);
            
            Console.WriteLine($"Waiting on work for state: {workEmitter.StateId}");
            Console.WriteLine("Sleeping for one minute.");

            Task.Delay(60000).Wait();

            ReportEmitter report = new ReportEmitter();
            // block on completed jobs, then timeouts. If 5 minutes after last container update, then just write a report anyways,
            // as it seems something has happened.
            report.ConditionedConstruct(workEmitter.StateId).Wait();
        }


    }
}
