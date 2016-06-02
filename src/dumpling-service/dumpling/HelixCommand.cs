// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Odin;
using Odin.Attributes;
using System;
using System.ComponentModel;
using helix;
using System.IO;
using System.Net;
using System.Net.Http;

namespace dumpling
{
    internal class HelixCommand : Command
    {
        [Action]
        [Description("Shows all stress containers listed in helix 'dotnetjobresults'.")]
        public void Containers()
        {
            foreach (var x in HelixResultsStorage.EnumerateStressContainers())
            {
                Console.WriteLine(x);
            }
        }

        [Action]
        [Description("Shows all correlation ids listed in specified helix container.")]
        public void CorrelationIds(
            [Alias("sc")]
            string selectedContainer)
        {
            foreach (var id in HelixResultsStorage.GetCorrelationIdsInContainer(selectedContainer))
            {
                Console.WriteLine(id);
            }
        }

        [Action]
        [Description("Retrieve all dump files for a container and correlation id pair.")]
        public void ListDumps(
            [Alias("sc")]
            string selectedContainer,
            [Alias("ci")]
            string correlationId)
        {
            foreach (var zip in HelixResultsStorage.ListZipFiles(selectedContainer, correlationId))
            {
                Console.WriteLine(zip);
            }
        }

        [Action]
        [Description("Retrieve all dump files for a container and correlation id pair.")]
        public void Download(
            [Alias("f")]
            [Description("A text file that lists URLs to retrieve zip files from.")]
            string from,
            [Alias("t")]
            [Description("A directory to store the downloaded files in. If it does not exist, we will create one.")]
            string to)
        {
            if (!File.Exists(from))
            {
                Console.WriteLine($"{from} does not appear to exist.");
            }

            if (!Directory.Exists(to))
            {
                Directory.CreateDirectory(to);
            }

            using (var client = new WebClient())
            {
                foreach (var line in File.ReadAllLines(from))
                {
                    try
                    {
                        Uri fileUri = new Uri(line);
                        var fullLength = fileUri.LocalPath.Length;
                        var startFilename = fileUri.LocalPath.LastIndexOf("/") + 1;

                        var filename = fileUri.LocalPath.Substring(startFilename, fullLength - startFilename);
                        client.DownloadFile(line, Path.Combine(to, filename));
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Error downloading from {line}");
                    }
                }
            }
        }
    }
}
