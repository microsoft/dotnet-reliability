// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Web.Configuration;


namespace DumplingLib
{
    public static class NearbyConfig
    {
        public static Dictionary<string, string> Settings { get; set; } = new Dictionary<string, string>();

        static NearbyConfig()
        {
            // Paste JSON config object here.
            var names =   "{ \"state table name\": \"dumplingstates\", \"analysis topic path\": \"dumplingtopic\", \"dumpling-service data-worker queue path\": \"dataworkerqueue\", \"dumpling-service eventhub path\": \"dumplinghub\"  }";
            Settings    = JsonConvert.DeserializeObject<Dictionary<string, string>>(names);
        }

        public static async Task RetrieveSecrets()
        {
            await DumplingKeyVaultAuthConfig.RegisterAsync();
        }
    }
}
