using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace stress.runtime.contracts
{
    public interface IWorkerStrategy
    {
        void SpawnWorker(ITestPattern pattern, CancellationToken cancelToken);
    }
}
