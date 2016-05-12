// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.


using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace triage.database
{
    public static class TriageDb
    {
        private static string s_connStr;

        public static void Init(string connStr)
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
            if (triageData == null) throw new ArgumentNullException("triageData");

            using (var context = new TriageDbContext(s_connStr))
            {
                //find the dump to update
                var dump = await context.Dumps.FindAsync(dumpId);

                if(dump == null)
                {
                    throw new ArgumentException($"Could not update dump.  No dump was found with id {dumpId}", "dumpId");
                }

                dump.LoadTriageData(triageData);

                await context.SaveChangesAsync();
            }
        }
        
        private const string BUCKET_DATA_QUERY = @"
WITH [BucketHits]([BucketId], [HitCount], [StartTime], [EndTime]) AS
(
    SELECT [B].[Id] AS [BucketId], COUNT([D].[Id]) AS [HitCount], @p0 AS [StartTime], @p1 AS [EndTime]
    FROM [Dumps] [D]
    JOIN [Buckets] [B]
        ON [D].[BucketId] = [B].[BucketId]
        AND [D].[DumpTime] >= @p0
        AND [D].[DumpTime] <= @p1
    GROUP BY [B].[Id]
)
SELECT [B].*, [H].[HitCount], [H].[Start], [H].[End]
FROM [Buckets] AS [B]
JOIN [BucketHits] AS [H]
    ON [B].[BucketId] = [H].[BucketId]
";
        public static async Task<IEnumerable<BucketData>> GetBucketDataAsync(DateTime start, DateTime end)
        {
            using (var context = new TriageDbContext(s_connStr))
            {
                return await context.Database.SqlQuery<BucketData>(BUCKET_DATA_QUERY, start, end).ToArrayAsync();
            }
        }

        private const string BUCKET_DATA_DUMPS_QUERY = @"
SELECT * 
FROM [Dumps]
WHERE [BucketId] = @p0
    AND [DumpTime] >= @p1
    AND [DumpTime] <= @p2
";
        public static async Task<IEnumerable<Dump>> GetBucketDataDumpsAysnc(BucketData bucketData)
        {
            using (var context = new TriageDbContext(s_connStr))
            {
                return await context.Dumps.SqlQuery(BUCKET_DATA_DUMPS_QUERY, bucketData.BucketId, bucketData.StartTime, bucketData.EndTime).ToArrayAsync();
            }
        }
    }
}
