using System;
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

        var sleepExecContext = new TaskExecContext(async () =>
        {
          Logger.Verbose("Before sleep");
          await Task.Delay(PeriodicJob.SleepInterval, cancellationToken).ConfigureAwait(false);
          Logger.Verbose("After sleep");
        },
        this,
        Logger);

        var jobExecContext = new TaskExecContext<JobResult>(async () =>
        {
          Logger.Verbose("Before PeriodicJob.Run()");
          var result = await PeriodicJob.Run(JobExceptionState, Logger, cancellationToken).ConfigureAwait(false);
          JobExceptionState.Clear();
          Logger.Verbose("After backgroundTask.Run()");
          return result;
        },
        this,
        Logger);

        while (!cancellationToken.IsCancellationRequested)
        {
          var jobResult = await jobExecContext.ExecAsync(JobExceptionState).ConfigureAwait(false);
          if (ShouldStop(cancellationToken, jobResult)) break;

          // Now let it handle any exception that occurred
          if (JobExceptionState.LastException != null)
          {
            if (!PeriodicJob.HandleException(JobExceptionState, Logger)) break;
          }

          await sleepExecContext.ExecAsync(Common.JobExceptionState.None).ConfigureAwait(false);

          //// Let the job run
          //var jobResult = await DoAsyncOperation(JobExceptionState, async () =>
          //{
          //  Logger.Verbose("Before backgroundTask.Run()");
          //  var result = await PeriodicJob.Run(JobExceptionState, Logger, cancellationToken).ConfigureAwait(false);
          //  JobExceptionState.Clear();
          //  Logger.Verbose("After backgroundTask.Run()");
          //  return result;
          //}).ConfigureAwait(false);
          //if (ShouldStop(cancellationToken, jobResult)) break;

          //// Now let it handle any exception that occurred
          //if (JobExceptionState.LastException != null)
          //{
          //  if (!PeriodicJob.HandleException(JobExceptionState, Logger)) break;
          //}

          // Sleep if necessary before running again
          //await TryOperationAsync(Common.JobExceptionState.None, async () =>
          //{
          //  Logger.Verbose("Before sleep");
          //  await Task.Delay(PeriodicJob.SleepInterval, cancellationToken).ConfigureAwait(false);
          //  Logger.Verbose("After sleep");
          //}).ConfigureAwait(false);
        }
      }, cancellationToken);
    }

    #endregion
  }
}
