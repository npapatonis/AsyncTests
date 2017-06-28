using System.Threading;
using System.Threading.Tasks;

namespace Tks.G1Track.Mobile.Shared.Common
{
  public interface IOverlappedProducer<TData>
  {
    Task<ProducerResult<TData>> Run(ILogger logger, CancellationToken cancellationToken);
  }
}
