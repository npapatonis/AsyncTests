using System;
using System.Threading;
using System.Threading.Tasks;

namespace Tks.G1Track.Mobile.Shared.Common
{
  public interface IPeriodicTask
  {
    // HandleException and Run Must return 'true' to continue.  Returning 'false' will abort the periodic task.
    bool HandleException(IDirectedTaskExceptionState taskExceptionState, ILogger logger);
    Task<bool> Run(IDirectedTaskExceptionState taskExceptionState, ILogger logger, CancellationToken cancellationToken);
    TimeSpan SleepInterval { get; }
  }
}
