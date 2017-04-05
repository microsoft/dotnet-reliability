using System.Threading;
using System.Threading.Tasks;

namespace stress.runtime.contracts
{
    public interface ILoadPattern
    {
        Task ExecuteAsync(ITestPattern testPattern, IWorkerStrategy workerStrategy, CancellationToken cancelToken);

        void Execute(ITestPattern testPattern, IWorkerStrategy workerStrategy, CancellationToken cancelToken);
    }
}
