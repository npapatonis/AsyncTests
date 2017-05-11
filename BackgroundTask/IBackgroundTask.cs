using System;
using System.Threading;
using System.Threading.Tasks;

namespace Tks.G1Track.Mobile.Shared.Common
{
  public interface IBackgroundTask
  {
    // HandleException and Run Must return 'true' to continue.  Returning 'false' will abort the background process.
    bool HandleException(IBackgroundTaskContext taskContext, ILogger logger);
    Task<bool> Run(IBackgroundTaskContext taskContext, ILogger logger, CancellationToken cancellationToken);
    TimeSpan SleepInterval { get; }
  }
}
