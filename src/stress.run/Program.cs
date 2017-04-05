using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;
using stress.execution;
using stress.tests;
using System.Threading;

namespace stress.run
{
    public class Program
    {
        private static int s_workerCount = 8;
        private static TimeSpan s_duration = TimeSpan.FromMinutes(5);
        private static int s_seed = new Random().Next();

        public static void Main(string[] args)
        {

        }


        public static void ExecuteTestMix()
        {
            CancellationTokenSource completeSource = new CancellationTokenSource(s_duration);

            var unitTests = new StressTestsAssemblyTestEnumerator().ToArray();

            var testPattern = new RandomTestPattern();

            testPattern.Initialize(s_seed, unitTests);

            DedicatedThreadWorkerStrategy workerStrategy = new DedicatedThreadWorkerStrategy();

            StaticLoadPattern loadPattern = new StaticLoadPattern() { WorkerCount = s_workerCount };

            loadPattern.Execute(testPattern, workerStrategy, completeSource.Token);
        }
    }
}
