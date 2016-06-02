// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Newtonsoft.Json;
using System.Collections.Generic;
using System.Configuration;
using System.IO;

namespace DumplingLib
{
    public static class NearbyConfig
    {
        public static Dictionary<string, string> Settings { get; set; }

        static NearbyConfig()
        {
            // Paste JSON config object here.
            var secrets = "{ }";
            Settings = JsonConvert.DeserializeObject<Dictionary<string, string>>(secrets);
        }
    }
}
