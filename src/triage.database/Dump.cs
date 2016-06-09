// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace triage.database
{
    public class Dump
    {
        public Dump()
        {
            this.Threads = new HashSet<Thread>();

            this.Properties = new HashSet<Property>();
        }

        public int DumpId { get; set; }

        public string DisplayName { get; set; }

        public string DumpPath { get; set; }

        public string Origin { get; set; }

        public int? BucketId { get; set; }

        [Required]
        [Index]
        public DateTime DumpTime { get; set; }

        public virtual ICollection<Thread> Threads { get; set; }

        public virtual ICollection<Property> Properties { get; set; }

        [ForeignKey("BucketId")]
        public virtual Bucket Bucket { get; set; }

        public void LoadTriageData(TriageData triageData)
        {
            if (this.BucketId != triageData.BucketId)
            {
                this.BucketId = triageData.BucketId;
            }

            //for now clear all the threads and insert all new ones
            //might consider changing to do an in place update if turns out to be a perf issue
            this.Threads.Clear();

            foreach (var t in triageData.Threads)
            {
                this.Threads.Add(t);
            }
            
            var existingProps = new Dictionary<string, Property>();

            foreach (var prop in this.Properties)
            {
                existingProps[prop.Name] = prop;
            }
            
            foreach (var p in triageData.Properties)
            {
                //if the property already exists update it, otherwise add it
                if (existingProps.ContainsKey(p.Name))
                {
                    existingProps[p.Name].Value = p.Value;
                }
                else
                {
                    this.Properties.Add(p);
                }
            }
        }
    }
}
