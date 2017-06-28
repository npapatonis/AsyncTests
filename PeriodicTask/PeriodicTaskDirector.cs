using System.Threading;
using System.Threading.Tasks;

namespace Tks.G1Track.Mobile.Shared.Common
{
  public class PeriodicTaskDirector : TaskDirectorBase
  {
    #region =====[ ctor ]==========================================================================================

    public PeriodicTaskDirector(IPeriodicTask periodicTask, ILogger logger)
      : base(logger)
    {
      PeriodicTask = periodicTask;
      TaskExceptionState = new DirectedTaskExceptionState();
    }

    #endregion

    #region =====[ Private Properties ]============================================================================

    private IPeriodicTask PeriodicTask { get; set; }
    private IDirectedTaskExceptionState TaskExceptionState { get; set; }

    #endregion

    #region =====[ Protected Methods ]===============================================================================

    protected override Task InternalStartAsync(CancellationToken cancellationToken)
    {
      return Task.Run(async () =>
      {
        bool cont = true;
        Logger.Verbose("Task.Run starting");

        while (!cancellationToken.IsCancellationRequested)
        {
          // Let the task run
          await DoAsyncOperation(TaskExceptionState, async () =>
          {
            Logger.Verbose("Before backgroundTask.Run()");
            cont = await PeriodicTask.Run(TaskExceptionState, Logger, cancellationToken).ConfigureAwait(false);
            TaskExceptionState.Clear();
            Logger.Verbose("After backgroundTask.Run()");
          }).ConfigureAwait(false);
          if (cancellationToken.IsCancellationRequested || !cont) break;

          // Now let it handle any exception that occurred
          if (TaskExceptionState.LastException != null)
          {
            cont = PeriodicTask.HandleException(TaskExceptionState, Logger);
          }
          if (cancellationToken.IsCancellationRequested || !cont) break;

          // Sleep if necessary before running again
          await DoAsyncOperation(TaskExceptionState, async () =>
          {
            Logger.Verbose("Before sleep");
            await Task.Delay(PeriodicTask.SleepInterval, cancellationToken).ConfigureAwait(false);
            Logger.Verbose("After sleep");
          }).ConfigureAwait(false);
        }
      }, cancellationToken);
    }

    #endregion
  }
}
