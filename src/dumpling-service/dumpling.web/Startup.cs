// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(dumplingWeb.Startup))]

namespace dumplingWeb
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
        }
    }
}
