using System;
using System.Threading;
using System.Threading.Tasks;

namespace Tks.G1Track.Mobile.Shared.Common
{
  public interface IDirectableJob
  {
    // HandleException and Run Must return 'true' to continue.  Returning 'false' will abort the periodic job.
    bool HandleException(IJobExceptionState jobExceptionState, ILogger logger);
    Task<JobResult> Run(IJobExceptionState jobExceptionState, ILogger logger, CancellationToken cancellationToken);
  }
}
