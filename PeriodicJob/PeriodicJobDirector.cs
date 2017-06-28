using System.Threading;
using System.Threading.Tasks;

namespace Tks.G1Track.Mobile.Shared.Common
{
  public class PeriodicJobDirector : JobDirectorBase
  {
    #region =====[ ctor ]==========================================================================================

    public PeriodicJobDirector(IPeriodicJob periodicJob, ILogger logger)
      : base(logger)
    {
      PeriodicJob = periodicJob;
      JobExceptionState = new JobExceptionState();
    }

    #endregion

    #region =====[ Private Properties ]============================================================================

    private IPeriodicJob PeriodicJob { get; set; }
    private IJobExceptionState JobExceptionState { get; set; }

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
          await DoAsyncOperation(JobExceptionState, async () =>
          {
            Logger.Verbose("Before backgroundTask.Run()");
            cont = await PeriodicJob.Run(JobExceptionState, Logger, cancellationToken).ConfigureAwait(false);
            JobExceptionState.Clear();
            Logger.Verbose("After backgroundTask.Run()");
          }).ConfigureAwait(false);
          if (cancellationToken.IsCancellationRequested || !cont) break;

          // Now let it handle any exception that occurred
          if (JobExceptionState.LastException != null)
          {
            cont = PeriodicJob.HandleException(JobExceptionState, Logger);
          }
          if (cancellationToken.IsCancellationRequested || !cont) break;

          // Sleep if necessary before running again
          await DoAsyncOperation(JobExceptionState, async () =>
          {
            Logger.Verbose("Before sleep");
            await Task.Delay(PeriodicJob.SleepInterval, cancellationToken).ConfigureAwait(false);
            Logger.Verbose("After sleep");
          }).ConfigureAwait(false);
        }
      }, cancellationToken);
    }

    #endregion
  }
}
