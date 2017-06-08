using System.Threading;
using System.Threading.Tasks;

namespace Tks.G1Track.Mobile.Shared.Common
{
  internal interface IParallelTask<TResult>
    where TResult : ITaskResult
  {
    Task<TResult> Run(ILogger logger, CancellationToken cancellationToken);
  }
}
