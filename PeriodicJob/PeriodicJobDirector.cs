using System.Threading;
using System.Threading.Tasks;

namespace Tks.G1Track.Mobile.Shared.Common
{
  public class PeriodicJobDirector : JobDirectorBase, ICancellationSource
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

    #region =====[ ICancellationSource ]=============================================================================

    public bool IsCancellationRequested => CancellationTokenSource.IsCancellationRequested;

    #endregion

    #region =====[ Protected Methods ]===============================================================================

    protected override Task InternalStartAsync(CancellationToken cancellationToken)
    {
      return Task.Run(async () =>
      {
        Logger.Verbose("Task.Run starting");

        while (!cancellationToken.IsCancellationRequested)
        {
          // Let the task run
          var jobResult = await TaskExecContext.ExecAsync(async () =>
          {
            Logger.Verbose("Before PeriodicJob.Run()");
            var result = await PeriodicJob.Run(JobExceptionState, Logger, cancellationToken).ConfigureAwait(false);
            JobExceptionState.Clear();
            Logger.Verbose("After backgroundTask.Run()");
            return result;
          },
          JobExceptionState,
          this,
          Logger).ConfigureAwait(false);

          if (ShouldStop(cancellationToken, jobResult)) break;

          // Now let it handle any exception that occurred
          if (JobExceptionState.LastException != null)
          {
            if (!PeriodicJob.HandleException(JobExceptionState, Logger)) break;
          }

          await TaskExecContext.ExecAsync(async () =>
          {
            Logger.Verbose("Before sleep");
            await Task.Delay(PeriodicJob.SleepInterval, cancellationToken).ConfigureAwait(false);
            Logger.Verbose("After sleep");
          },
          Common.JobExceptionState.None,
          this,
          Logger).ConfigureAwait(false);
        }
      }, cancellationToken);
    }

    #endregion
  }
}
