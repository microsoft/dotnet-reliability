// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using triage.database;

namespace dumplingWeb.Controllers
{
    public class HomeController : Controller
    {
        public async Task<ActionResult> Index(DateTime start, DateTime end)
        {
            var buckets = await TriageDb.GetBucketDataAsync(start, end);

            foreach (var bucket in buckets)
            {
                var dumps = await TriageDb.GetBucketDataDumpsAsync(bucket);

                ViewData[bucket.Name] = dumps;

                foreach (var dump in dumps)
                {
                    ViewData["dumpid." + dump.DumpId.ToString()] = TriageDb.GetPropertiesAsJsonAsync(dump);
                }
            }

            ViewBag.Title = "dumpling";

            ViewData["Buckets"] = buckets;

            return View();
        }
    }
}
