using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;

namespace stress.codegen
{
    public class RetreiveBuildContainerInformation : Task
    {
        private BuildMaps buildMap;
        private bool SeparateTestStep;

        [Required]
        public string BuildPAT { get; set; }
        [Required]
        public string HelixAPIKey { get; set; }
        [Required]
        public string Repo { get; set; }
        [Required]
        public string Branch { get; set; }
        [Required]
        public string OperatingSystem{ get; set; }
        public string ConfigurationGroup { get; set; }
        public string Platform { get; set; }
        public string SubType { get; set; }
        public string BuildNumber{ get; set; }
        public string TestStorageAccountKey { get; set; }
        [Output]
        public string TestPattern { get; set; }
        [Output]
        public string ProductContainerName { get; set; }
        [Output]
        public string ProductStorageAccount { get; set; }
        [Output]
        public string TestStorageAccount { get; set; }
        [Output]
        public string TestContainerName { get; set; }

        const string containerRegex = @"Creating container named '(.*?)' in storage account (.*?)\.";
        const string buildDefnUrlRegex = @"https:\/\/(.*?)\.visualstudio\.com\/DefaultCollection\/(.*?)\/_build\?_a=summary&buildId=(.*)";

        public override bool Execute()
        {
            Debugger.Launch();
            buildMap = new BuildMaps();
            GetLatestBuilds();
            return !Log.HasLoggedErrors;
        }

        private void GetLatestBuilds()
        {
            if (string.IsNullOrEmpty(BuildNumber))
            {
                GetLatestBuildNumber();
                buildMap.BuildNumber = BuildNumber;
            }
            //Product build
            string dotnetMCUrl = $"https://helix.dot.net/api/2017-01-20/aggregate/analysisdetail?analysisName=Visual+Studio+Build+Information&analysisType=external&build={ BuildNumber }&groupBy=job.properties.operatingSystem&groupBy=job.properties.subType&groupBy=job.properties.configurationGroup&groupBy=job.properties.platform&source=official%2F{Repo}%2F{Branch}%2F&type=build%2Fproduct%2F&workitem=Orchestration";
            try
            {
                string value = string.Empty;
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.Add("Authentication", string.Format("{0} {1}", "token", HelixAPIKey));
                    using (HttpResponseMessage response = client.GetAsync(dotnetMCUrl).Result)
                    {
                        response.EnsureSuccessStatusCode();
                        value = response.Content.ReadAsStringAsync().Result;
                    }

                }

                JArray buildDefintionsArray = (JArray)JsonConvert.DeserializeObject(value);
                string type = string.Empty;
                foreach (var buildDefinition in buildDefintionsArray)
                {
                    //https://devdiv.visualstudio.com/DefaultCollection/DevDiv/_build?_a=summary&buildId=570846
                    
                    string url = buildDefinition["Analysis"]["Uri"].ToString();
                    string operatingSystem = buildDefinition["Key"]["job.properties.operatingSystem"].ToString().Replace(" ", string.Empty);
                    string configurationGroup = buildDefinition["Key"]["job.properties.configurationGroup"].ToString();
                    string platform = buildDefinition["Key"]["job.properties.platform"].ToString();
                    string subType = buildDefinition["Key"]["job.properties.subType"].ToString();
                    //coreclr has a testBuild type
                    type =string.Join("-", operatingSystem, configurationGroup, platform, subType);
                    Match m = Regex.Match(url, buildDefnUrlRegex);
                    if (m.Groups == null)
                    {
                        continue;
                    }
                    //hack until coreclr labels it's builds correctly
                    if (buildMap.buildmap.ContainsKey(type))
                    {
                        continue;
                    }
                    BuildContainerInformation bContainerInformation = new BuildContainerInformation();
                    VSOBuild build = new VSOBuild(m.Groups[1].Value, m.Groups[2].Value, m.Groups[3].Value);
                    bContainerInformation.ProductBuild = build;
                    //coreclr
                    if (subType == "testBuild")
                    {
                        SeparateTestStep = true;
                        GetTestContainer(bContainerInformation);
                    }
                    else
                    {
                        GetProductContainer(bContainerInformation);
                    }
                    buildMap.buildmap.Add(type, bContainerInformation);
                }
            }
            catch (Exception ex)
            {
                Log.LogError(ex.ToString());
            }

            //Grab Test zips for OperatingSystem
            List<string> potentialTestZips = new List<string>();
            BuildContainerInformation bc;
            BuildContainerInformation bcProduct;
            //coreclr
            if (SeparateTestStep)
            {
                bcProduct = buildMap.buildmap[string.Join("-", OperatingSystem, ConfigurationGroup, Platform, "testBuild")];
                bc = buildMap.buildmap[string.Join("-", OperatingSystem, ConfigurationGroup, Platform, "testBuild")];
                potentialTestZips = RetreiveBlobNames.GetBlobs(bc.TestDropStorageAccount, TestStorageAccountKey, bc.TestDropContainer);
            }
            else
            {
                bcProduct = null;
                bc = buildMap.buildmap[string.Join("-", OperatingSystem, ConfigurationGroup, Platform, SubType)];
                potentialTestZips = RetreiveBlobNames.GetBlobs(bc.TestDropStorageAccount, TestStorageAccountKey, bc.TestDropContainer);
            }

            string testPattern = string.Empty;

            //Get path selection for stress
            if (potentialTestZips.Count > 0) {
                for (int counter = 0; counter < potentialTestZips.Count; counter++)
                {
                    int lastIndexOf = potentialTestZips[counter].LastIndexOf('/');  
                    if (lastIndexOf > 0)
                    {
                        testPattern = potentialTestZips[counter].Substring(0, lastIndexOf);
                        Log.LogMessage("Test zip pattern: {0}", testPattern);
                        break;
                    }
                }
            }
            
            //Setup output parameters
            TestPattern = testPattern;
            ProductStorageAccount = bc.ProductDropStorageAccount == null ? bcProduct.TestDropStorageAccount : bc.TestDropStorageAccount;
            ProductContainerName = bc.ProductDropContainer == null ? bcProduct.TestDropStorageAccount : bc.TestDropStorageAccount;
            TestStorageAccount = bc.TestDropStorageAccount==null ? string.Empty : bc.TestDropStorageAccount;
            TestContainerName = bc.TestDropContainer==null ? string.Empty : bc.TestDropContainer;
        }

        private void GetLatestBuildNumber()
        {
            string GetBuildNumberUrl = $"https://helix.dot.net/api/2016-08-25/aggregate/jobs?groupBy=job.build&maxResultSets=6&source=official%2F{ Repo }%2F{ Branch }%2F&type=build%2Fproduct%2F";
            try
            {
                string value = string.Empty;
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.Add("Authentication", string.Format("{0} {1}", "token", HelixAPIKey));
                    using (HttpResponseMessage response = client.GetAsync(GetBuildNumberUrl).Result)
                    {
                        response.EnsureSuccessStatusCode();
                        value = response.Content.ReadAsStringAsync().Result;
                    }

                    JArray buildapiObject = (JArray)JsonConvert.DeserializeObject(value);
                    //Grab latest one
                    //TODO: Grab latest passing one?
                    BuildNumber = buildapiObject[0]["Key"]["job.build"].ToString();
                }
            }
            catch (Exception ex)
            {
                Log.LogMessage(ex.ToString());
            }
        }

        private void GetProductContainer(BuildContainerInformation b)
        {
            string logData = GetBuildLog(b.ProductBuild);
            int i = 0;
            
            MatchCollection mCollection = Regex.Matches(logData, containerRegex, RegexOptions.None);
            if (mCollection.Count == 3)
            {
                foreach (Match m in mCollection)
                {
                    if (i == 0)
                    {
                        b.TestDropContainer = m.Groups[1].Value;
                        b.TestDropStorageAccount = m.Groups[2].Value;
                    }
                    //Assumption that SendToHelix api order printed out doesn't change. 
                    if (i == 1)
                    {
                        b.TestResultsContainer = m.Groups[1].Value;
                        b.TestResultsStorageAccount = m.Groups[2].Value;
                    }
                    if (i == 2)
                    {
                        b.ProductDropContainer = m.Groups[1].Value;
                        b.ProductDropStorageAccount = m.Groups[2].Value;
                    }
                    i++;
                }
            }
            else 
            {
                //hack until order for each repo is sorted out.
                foreach (Match m in mCollection)
                {
                    if (i == 0)
                    {
                        b.ProductDropContainer = m.Groups[1].Value;
                        b.ProductDropStorageAccount = m.Groups[2].Value;
                    }
                    i++;
                }
            }
        }

        //In case you would like to get TestContainer for a repo like coreclr where the test build is in a separate step
        private void GetTestContainer(BuildContainerInformation b)
        {
            string logData = GetBuildLog(b.ProductBuild);
            int i = 0;
            MatchCollection mCollection = Regex.Matches(logData, containerRegex, RegexOptions.None);
            foreach (Match m in mCollection)
            {
                if (i == 0)
                {
                    b.TestDropContainer = m.Groups[1].Value;
                    b.TestDropStorageAccount = m.Groups[2].Value;
                }
                //Assumption that SendToHelix api order printed out doesn't change. 
                if (i == 1)
                {
                    b.TestResultsContainer = m.Groups[1].Value;
                    b.TestResultsStorageAccount = m.Groups[2].Value;
                }
                i++;
            }
            
        }

        private string GetBuildLog(VSOBuild build)
        {
            string logData = string.Empty;
            string stepNumber = string.Empty;
            string LogApiUrl = $"https://{build.VSOProjectNamespace}.visualstudio.com/DefaultCollection/{build.DefaultCollection}/_apis/build/builds/{build.VSOBuildId}/logs";
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(string.Format("{0}:{1}", "", BuildPAT))));
                    using (HttpResponseMessage response = client.GetAsync(LogApiUrl).Result)
                    {
                        response.EnsureSuccessStatusCode();
                        logData = response.Content.ReadAsStringAsync().Result;
                        JObject logCountObject = (JObject)JsonConvert.DeserializeObject(logData);
                        string count = logCountObject["count"].ToString();
                        int stepNumber1 = int.Parse(count) - 1;
                        stepNumber = stepNumber1.ToString();
                    }
                    string LogCountApiUrl = $"https://{build.VSOProjectNamespace}.visualstudio.com/DefaultCollection/{build.DefaultCollection}/_apis/build/builds/{build.VSOBuildId}/logs/{stepNumber}";
                    using (HttpResponseMessage response = client.GetAsync(LogCountApiUrl).Result)
                    {
                        response.EnsureSuccessStatusCode();
                        logData = response.Content.ReadAsStringAsync().Result;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.LogMessage(ex.ToString());
            }

            return logData;
        }
    }
}

internal class BuildMaps
{
    public Dictionary<string, BuildContainerInformation> buildmap;
    public string BuildNumber;
    public BuildMaps()
    {
        buildmap = new Dictionary<string, BuildContainerInformation>();
    }
}

internal class VSOBuild
{
    public string DefaultCollection { get; set; }
    public string VSOProjectNamespace { get; set; }
    public string VSOBuildId { get; set; }

    public string ProductStepContainingContainer { get; set; }
    public string TestStepContainingContainer { get; set; }

    public VSOBuild(string project, string collection, string buildid)
    {
        VSOProjectNamespace = project;
        DefaultCollection = collection;
        VSOBuildId = buildid;
    }

}

internal class BuildContainerInformation
{
    public VSOBuild ProductBuild { get; set; }
    public string TestDropStorageAccount { get; set; }
    public string TestDropContainer { get; set; }

    public string ProductDropStorageAccount { get; set; }
    public string ProductDropContainer { get; set; }

    public string TestResultsContainer { get; set; }
    public string TestResultsStorageAccount { get; set; }

}