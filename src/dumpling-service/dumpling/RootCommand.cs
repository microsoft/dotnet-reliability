// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Odin;
using Odin.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.IO;
using System.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace dumpling
{
    internal class RootCommand : Command
    {
        private const string BASE_URL = "http://dotnetrp.azurewebsites.net";
        public RootCommand(Command helixcmd)
        {
            // helix stuff
            RegisterSubCommand(helixcmd);
        }

        [Action]
        [Description("Sends a message to the server, and the server sends one back to you.")]
        public int Hello(
        [Description("Override who to say hello to. Defaults to 'World'.")]
        [Alias("w")]
        string who = "World")
        {
            this.Logger.Info($"Saying hi on behalf of {who}.\n");

            var response = new HttpClient().GetAsync($"{BASE_URL}/dumpling/test/hi/im/{who}").Result.Content.ReadAsStringAsync().Result;

            Console.WriteLine(response);
            return 0;
        }

        [Action]
        [Description("Uploads a dump file to the dumpling service.")]
        public void Upload(
        [Description("The zipped up dump file and necessary contents.")]
        [Alias("f")]
        string file,
        [Description("The OS the dump was collected on. Support only exists for [ubuntu|centos]")]
        string targetos,
        [Description("Some kind of name to call you by.")]
        string @as)
        {
            const int index = 0;
            var filesize = 0;

            try
            {
                var multipart = new MultipartFormDataContent();
                multipart.Add(new StreamContent(new FileStream(file, FileMode.Open)));
                var response = new HttpClient().PostAsync($"{BASE_URL}/dumpling/store/chunk/{@as}/{targetos}/{index}/{filesize}", multipart);

                Console.WriteLine(response.Result.Content.ReadAsStringAsync().Result.Trim(new[] { '"' }));
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception: {e}.");
            }
        }

        [Action]
        public void Status(string id, string @as)
        {
            Console.WriteLine(new HttpClient().GetStringAsync($"{BASE_URL}/dumpling/status/{@as}/{id}").Result.Trim(new[] { '"' }));
        }

        [Action]
        [Description("Upload ALL the zip files in a directory (does not recurse).")]
        public void Feast(string folder, [Alias("t")] string targetos, string @as)
        {
            if (!Directory.Exists(folder))
            {
                Console.WriteLine($"Folder {folder} does not seem to exist.");
            }
            foreach (var file in Directory.EnumerateFiles(folder))
            {
                Upload(file, targetos, @as);
            }
        }

        [Action]
        [Description("Upload ALL the zip files in a directory (does not recurse).")]
        public void Re(int dumplingid, string originOs, string @as)
        {
            // /dumpling/redumpling/{owner}/{dumplingid}/{os}
            var result = new HttpClient().PutAsync($"{BASE_URL}/dumpling/redumpling/{@as}/{dumplingid}/{originOs}", null);

            if (result.Result.StatusCode == System.Net.HttpStatusCode.NoContent)
            {
                Console.WriteLine($"Re-running {dumplingid}.");
            }
            else
            {
                Console.WriteLine($"Response code: {result.Result.StatusCode}");
            }
        }


        [Action]
        [Description("Given a owner and dumpling id, this will show the results of the analysis in raw json.")]
        public void Results(string @as, string id)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var url = client.GetStringAsync($"{BASE_URL}/dumpling/store/results/{@as}/{id}").Result.Trim(new[] { '\"' });
                    if (url.ToLower() != "null")
                    {
                        var data = client.GetStringAsync(url).Result;

                        Console.WriteLine(JValue.Parse(data).ToString(Formatting.Indented));
                    }
                }
            }
            catch (System.Net.Http.HttpRequestException)
            {
                Console.WriteLine("Couldn't locate information for that particular dumpling id. There may not be results.");
            }
        }

        [Action]
        [Description("Dont touch this.")]
        public void ReRun()
        {
            for (int i = 1080; i <= 1143; i++)
            {
                Re(i, "ubuntu", "bryanar");
            }
        }


        #region deprecated

        [Action]
        [Description("Given a file pointing to a list of dumpling ids, generate an html report.")]
        public void Report(string file, string owner)
        {
            if (!File.Exists(file))
            {
                Console.WriteLine($"File {file} does not seem to exist.");
            }

            var results = new Dictionary<string, Dictionary<string, string>>();

            using (HttpClient client = new HttpClient())
            {
                foreach (var id in File.ReadAllLines(file))
                {
                    // retrieve report url
                    var url = client.GetStringAsync($"{BASE_URL}/dumpling/store/results/{owner}/{id}").Result.Trim(new[] { '\"' });

                    if (url.ToLower() != "null")
                    {
                        var dumpUrl = client.GetStringAsync($"{BASE_URL}/dumpling/store/dump/{owner}/{id}").Result.Trim(new[] { '\"' });

                        var data = client.GetStringAsync(url).Result;

                        var dumplingValues = JsonConvert.DeserializeObject<Dictionary<string, string>>(data);
                        dumplingValues.Add("URL", dumpUrl);

                        results.Add(id, dumplingValues);

                        //Console.WriteLine(data);
                    }
                }
            }

            var organizedData = OrganizeResults(results);

            WriteReport(Path.ChangeExtension(file, "html"), organizedData);
        }

        private string OrganizeResults(Dictionary<string, Dictionary<string, string>> data, string groupKey = "FAILURE_HASH")
        {
            var relevantResults = from x in data
                                  where x.Value.ContainsKey(groupKey)
                                  select new { Id = x.Key, Group = x.Value[groupKey], Store = x.Value }; // Id = x.Key = dumpling id


            var groups = relevantResults.GroupBy(x => x.Group);

            var structuredGroups = groups.Select(x => new { Bucket = x.Key, Data = x });

            foreach (var group in structuredGroups)
            {
                Console.WriteLine($"{group.Bucket} ({group.Data.Count()} results.)");
            }

            return JsonConvert.SerializeObject(structuredGroups);
        }

        private void WriteReport(string filename, string data)
        {
            string form = @"<!DOCTYPE html>

<html>
<head>
    <title>dumpling results</title>
    <link href='https://fonts.googleapis.com/css?family=Open+Sans' rel='stylesheet' type='text/css'>
    <style>
        body {
            font-family: 'Open Sans', sans-serif;
        }

        span {
            font-style: italic;
        }

    </style>
    <script>
var DumplingFocus = (function () {
    function DumplingFocus(element) {
        this.element = element;
    }
    DumplingFocus.prototype.ClearRows = function () {
        while (this.element.rows.length > 0) {
            this.element.deleteRow(0);
        }
    };
    DumplingFocus.prototype.FocusOn = function (data) {
        this.ClearRows();
        for (var field in DumplingBuckets.Index[data.innerText]) {
            var row = this.element.insertRow();
            var leftCell = row.insertCell();
            var font = document.createElement('font');
            font.innerText = field;
            font.color = '#d1ac87';
            leftCell.appendChild(font);
            leftCell.align = 'center';
            leftCell.bgColor = '#333232';
            row.insertCell().innerText = DumplingBuckets.Index[data.innerText][field];
        }
    };
    return DumplingFocus;
})();
var DumplingBuckets = (function () {
    function DumplingBuckets(element, groups, focusInstance) {
        this.element = element;
        for (var bucket in groups) {
            for (var dumpling in groups[bucket].Data) {
                DumplingBuckets.Index[groups[bucket].Data[dumpling].Id] = groups[bucket].Data[dumpling].Store;
            }
        }
        DumplingBuckets.Buckets = groups;
        DumplingBuckets.focus = focusInstance;
        for (var groupRow in groups) {
            var groupTableRowElement = this.element.insertRow();
            var groupCell = groupTableRowElement.insertCell();
            groupCell.innerText = groups[groupRow].Bucket;
            groupCell.rowSpan = groups[groupRow].Data.length;
            groupCell.vAlign = 'top';
            var shareRow = true;
            for (var dumplingRow in groups[groupRow].Data) {
                if (shareRow) {
                    var dumplingsCell = groupTableRowElement.insertCell();
                    dumplingsCell.onmouseover = this.MouseOverDumplingId;
                    dumplingsCell.onmouseleave = this.MouseLeaveDumplingId;
                    dumplingsCell.onmouseup = this.MouseClickDumpling;
                    dumplingsCell.innerText = groups[groupRow].Data[dumplingRow].Id;
                    shareRow = false;
                    continue;
                }
                var dumplingRowElement = this.element.insertRow();
                var dumplingsCell = dumplingRowElement.insertCell();
                dumplingsCell.onmouseover = this.MouseOverDumplingId;
                dumplingsCell.onmouseleave = this.MouseLeaveDumplingId;
                dumplingsCell.onmouseup = this.MouseClickDumpling;
                dumplingsCell.innerText = groups[groupRow].Data[dumplingRow].Id;
            }
        }
    }
    DumplingBuckets.prototype.MouseClickDumpling = function (evt) {
        var target = evt.target;
        if (DumplingBuckets.focusedElement != null) {
            DumplingBuckets.focusedElement.bgColor = '#FFFFFF';
        }
        DumplingBuckets.focusedElement = target;
        target.bgColor = '#FFFCC9';
    };
    DumplingBuckets.prototype.MouseOverDumplingId = function (evt) {
        var target = evt.target;
        if (target == DumplingBuckets.focusedElement) {
            return;
        }
        target.bgColor = '#9090D4';
        DumplingBuckets.focus.FocusOn(target);
    };
    DumplingBuckets.prototype.MouseLeaveDumplingId = function (evt) {
        var target = evt.target;
        if (DumplingBuckets.focusedElement != null && target == DumplingBuckets.focusedElement) {
            return;
        }
        target.bgColor = '#FFFFFF';
        DumplingBuckets.focus.FocusOn(DumplingBuckets.focusedElement);
    };
    return DumplingBuckets;
})();
window.onload = function () {
    var groupsTableElement = document.getElementById('groups');
    var focusTableElement = document.getElementById('focus');
    var bucketedData = " + data + @";
    DumplingBuckets.Index = {};
    var focus = new DumplingFocus(focusTableElement);
    var buckets = new DumplingBuckets(groupsTableElement, bucketedData, focus);
};



    </script>
</head>
<body>
    <h1>dumpling report</h1>

    <table id='groups' border='0'></table>
    <hr />
    <table id='focus' border='0'></table>

</body>
</html>
";
            File.WriteAllText(filename, form);
            Console.WriteLine($"Created {Path.Combine(Environment.CurrentDirectory, filename)}");
        }
        #endregion
    }
}
