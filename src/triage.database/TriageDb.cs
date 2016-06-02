// Copyright(c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.


using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace triage.database
{
    public static class TriageDb
    {
        private static string s_connStr;

        static TriageDb()
        {
            // Init(ConnString)
        }

        private static void Init(string connStr)
        {
            s_connStr = connStr;
        }

        public static async Task<Dump> GetDumpAsync(int dumpId)
        {
            using (var context = new TriageDbContext(s_connStr))
            {
                return await context.Dumps.FindAsync(dumpId);
            }
        }

        public static async Task<int> AddDumpAsync(Dump dump)
        {
            using (var context = new TriageDbContext(s_connStr))
            {
                context.Dumps.Add(dump);

                await context.SaveChangesAsync();
            }

            return dump.DumpId;
        }
        public static async Task UpdateDumpTriageInfo(int dumpId, Dictionary<string, string> triageData)
        {
            await UpdateDumpTriageInfo(dumpId, TriageData.FromDictionary(triageData));
        }

        public static async Task UpdateDumpTriageInfo(int dumpId, TriageData triageData)
        {
            if (triageData == null) throw new ArgumentNullException("triageData");

            Dump dump = null;

            using (var context = new TriageDbContext(s_connStr))
            {
                //find the dump to update
                dump = await context.Dumps.FindAsync(dumpId);

                if (dump == null)
                {
                    throw new ArgumentException($"Could not update dump.  No dump was found with id {dumpId}", "dumpId");
                }

                await UpdateUniquelyNamedEntitiesAsync(context, triageData);

                //remove all the dump properties from the context before updating
                //this is needed because property has a required FK to dumpId so
                //properties without an associated dump are not allowed
                context.Properties.RemoveRange(dump.Properties);

                var frames = dump.Threads.SelectMany(t => t.Frames).ToArray();

                context.Frames.RemoveRange(frames);

                context.Threads.RemoveRange(dump.Threads);

                await context.SaveChangesAsync();

                await context.Entry<Dump>(dump).ReloadAsync();

                dump.LoadTriageData(triageData);

                await context.SaveChangesAsync();
            }
        }

        private static async Task UpdateUniquelyNamedEntitiesAsync(TriageDbContext context, TriageData triageData)
        {
            //add or update the bucket if it exists
            if (triageData.Bucket != null && triageData.Bucket.BucketId == 0)
            {
                triageData.Bucket = await GetUniquelyNamedEntityAsync(context, context.Buckets, triageData.Bucket);
            }

            var addedModules = triageData.Threads.Where(t => t.ThreadId == 0).SelectMany(t => t.Frames).Select(f => f.Module).Where(m => m.ModuleId == 0).OrderBy(m => m.Name).ToArray();

            for (int i = 0; i < addedModules.Length; i++)
            {
                if (i > 0 && addedModules[i].Name == addedModules[i - 1].Name)
                {
                    addedModules[i].ModuleId = addedModules[i - 1].ModuleId;
                }
                else
                {
                    addedModules[i].ModuleId = (await GetUniquelyNamedEntityAsync(context, context.Modules, addedModules[i])).ModuleId;
                }
            }

            var addedRoutines = triageData.Threads.Where(t => t.ThreadId == 0).SelectMany(t => t.Frames).Select(f => f.Routine).Where(r => r.RoutineId == 0).OrderBy(r => r.Name).ToArray();

            for (int i = 0; i < addedRoutines.Length; i++)
            {
                if (i > 0 && addedRoutines[i].Name == addedRoutines[i - 1].Name)
                {
                    addedRoutines[i].RoutineId = addedRoutines[i - 1].RoutineId;
                }
                else
                {
                    addedRoutines[i].RoutineId = (await GetUniquelyNamedEntityAsync(context, context.Routines, addedRoutines[i])).RoutineId;
                }
            }

            foreach (var frame in triageData.Threads.SelectMany(t => t.Frames))
            {
                frame.ModuleId = frame.Module.ModuleId;

                frame.Module = null;

                frame.RoutineId = frame.Routine.RoutineId;

                frame.Routine = null;
            }
        }

        private static async Task<T> GetUniquelyNamedEntityAsync<T>(TriageDbContext context, IDbSet<T> set, T entity) where T : UniquelyNamedEntity
        {
            if (entity.Name.Length > 450)
            {
                entity.Name = entity.Name.Substring(0, 450);
            }

            T tempEntity = await set.FirstOrDefaultAsync(e => e.Name == entity.Name);

            if (tempEntity != null)
            {
                return tempEntity;
            }

            set.Add(entity);

            await context.SaveChangesAsync();

            return entity;
        }

        private const string BUCKET_DATA_QUERY = @"
WITH [BucketHits]([BucketId], [HitCount], [StartTime], [EndTime]) AS
(
    SELECT [B].[BucketId] AS [BucketId], COUNT([D].[DumpId]) AS [HitCount], @p0 AS [StartTime], @p1 AS [EndTime]
    FROM [Dumps] [D]
    JOIN [Buckets] [B]
        ON [D].[BucketId] = [B].[BucketId]
        AND [D].[DumpTime] >= @p0
        AND [D].[DumpTime] <= @p1
    GROUP BY [B].[BucketId]
)
SELECT [B].[BucketId], [B].[Name], [B].[BugUrl], [H].[HitCount], [H].[StartTime], [H].[EndTime]
FROM [Buckets] AS [B]
JOIN [BucketHits] AS [H]
    ON [B].[BucketId] = [H].[BucketId]
ORDER BY [H].[HitCount] DESC
";
        public static async Task<IEnumerable<BucketData>> GetBucketDataAsync(DateTime start, DateTime end)
        {
            using (var context = new TriageDbContext(s_connStr))
            {
                var data = context.Database.SqlQuery<BucketData>(BUCKET_DATA_QUERY, start, end);
                var returnValue = await data.ToArrayAsync();
                return returnValue;
            }
        }

        private const string BUCKET_DATA_DUMPS_QUERY = @"
SELECT * 
FROM [Dumps]
WHERE [BucketId] = @p0
    AND [DumpTime] >= @p1
    AND [DumpTime] <= @p2
ORDER BY [Dumps].[DumpId] DESC
";
        public static async Task<IEnumerable<Dump>> GetBucketDataDumpsAsync(BucketData bucketData)
        {
            using (var context = new TriageDbContext(s_connStr))
            {
                return await context.Dumps.SqlQuery(BUCKET_DATA_DUMPS_QUERY, bucketData.BucketId, bucketData.StartTime, bucketData.EndTime).ToArrayAsync();
            }
        }

        private const string PROPERTIES_QUERY = @"
SELECT * 
FROM [Properties]
WHERE [DumpId] = @p0
";

        public static string GetPropertiesAsJsonAsync(Dump dump)
        {
            using (var context = new TriageDbContext(s_connStr))
            {
                var data = from property in context.Properties.Where(x => x.DumpId == dump.DumpId)
                           select new
                           {
                               DumpId = property.DumpId,
                               Name = property.Name,
                               Value = property.Value
                           };

                return JsonConvert.SerializeObject(data);
            }
        }
    }
}

