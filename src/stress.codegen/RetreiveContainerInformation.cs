using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Xml.Linq;
using Microsoft.DotNet.Build.CloudTestTasks;

namespace stress.codegen
{
    class RetreiveBlobNames
    {
        public static List<string> GetBlobs(string storageAccount, string accountKey, string container)
        {
            string url = $"https://{ storageAccount }.blob.core.windows.net/{ container }?restype=container&comp=list";
            List<string> testZipStrings = new List<string>();
            try
            {
                Func<HttpRequestMessage> createRequest = () =>
                {
                    DateTime dateTime = DateTime.UtcNow;
                    var request = new HttpRequestMessage(HttpMethod.Get, url);
                    request.Headers.Add(AzureHelper.DateHeaderString, dateTime.ToString("R", CultureInfo.InvariantCulture));
                    request.Headers.Add(AzureHelper.VersionHeaderString, AzureHelper.StorageApiVersion);
                    request.Headers.Add(AzureHelper.AuthorizationHeaderString, AzureHelper.AuthorizationHeader(
                            storageAccount,
                            accountKey,
                            "GET",
                            dateTime,
                            request));
                    return request;
                };

                XDocument responseFile;
                using (HttpClient client = new HttpClient())
                using (HttpResponseMessage response = client.SendAsync(createRequest()).Result)
                {
                    string content = response.Content.ReadAsStringAsync().Result;
                    using (StreamReader sr = new StreamReader(response.Content.ReadAsStreamAsync().Result))
                    {
                        responseFile = XDocument.Load(sr);

                        List<XElement> blobs = responseFile.Descendants("EnumerationResults").First()
                                                           .Descendants("Blobs").First()
                                                           .Descendants("Blob").ToList();

                        var testZipBlobs = from blob in blobs
                                           where blob.Element("Name").ToString().Contains(".Tests.zip")
                                           select blob.Element("Name").Value;
                        testZipStrings = testZipBlobs.ToList();

                        if (testZipStrings.Count == 0)
                            Console.WriteLine("No test zips found.");
                    }

                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            return testZipStrings;
        }
    }
}
