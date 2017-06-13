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
      TaskContext = new DirectedTaskContext();
    }

    #endregion

    #region =====[ Private Properties ]============================================================================

    private IPeriodicTask PeriodicTask { get; set; }
    private IDirectedTaskContext TaskContext { get; set; }

    #endregion

    #region =====[ Protected Methods ]===============================================================================

    protected override Task StartImplAsync(CancellationToken cancellationToken)
    {
      return Task.Run(async () =>
      {
        bool cont = true;
        Logger.Verbose("Task.Run starting");

        while (!cancellationToken.IsCancellationRequested)
        {
          // Let the task run
          await DoAsyncOperation(TaskContext, async () =>
          {
            Logger.Verbose("Before backgroundTask.Run()");

            cont = await PeriodicTask.Run(TaskContext, Logger, cancellationToken).ConfigureAwait(false);

            TaskContext.LastException = null;
            TaskContext.LastExceptionCount = 0;
            TaskContext.ExceptionCount = 0;

            Logger.Verbose("After backgroundTask.Run()");
          }).ConfigureAwait(false);
          if (cancellationToken.IsCancellationRequested || !cont) break;

          // Now let it handle any exception that occurred
          if (TaskContext.LastException != null)
          {
            cont = PeriodicTask.HandleException(TaskContext, Logger);
          }
          if (cancellationToken.IsCancellationRequested || !cont) break;

          // Sleep if necessary before running again
          await DoAsyncOperation(TaskContext, async () =>
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
