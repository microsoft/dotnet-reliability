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
            if(args.Length > 3)
            {
                PrintUsage();
                return;
            }

            if(args.Length > 0 && !TimeSpan.TryParse(args[0], out s_duration))
            {
                PrintUsage();
                return;
            }

            if (args.Length > 1 && !int.TryParse(args[1], out s_workerCount))
            {
                PrintUsage();
                return;
            }

            if (args.Length > 2 && !int.TryParse(args[1], out s_seed))
            {
                PrintUsage();
                return;
            }

            ExecuteTestMix();
        }

        public static void PrintUsage()
        {
            Console.WriteLine("USAGE: stress.run.exe [duration (timespan)] [workerCount (int)] [seed (int)]");
        }

        public static void ExecuteTestMix()
        {
            Console.WriteLine($"executing stress test mix (duration={s_duration} workercount={s_workerCount} seed={s_seed})");

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
