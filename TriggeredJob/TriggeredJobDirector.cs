using System.Threading;
using System.Threading.Tasks;

namespace Tks.G1Track.Mobile.Shared.Common
{
  public class TriggeredJobDirector : JobDirectorBase
  {
    #region =====[ ctor ]==========================================================================================

    public TriggeredJobDirector(ITriggeredJob triggeredJob, ILogger logger)
      : base(logger)
    {
      TriggeredJob = triggeredJob;
      JobExceptionState = new JobExceptionState();
    }

    #endregion

    #region =====[ Private Properties ]============================================================================

    private ITriggeredJob TriggeredJob { get; set; }
    private IJobExceptionState JobExceptionState { get; set; }

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
            Logger.Verbose("Before TriggeredJob.Run()");
            var result = await TriggeredJob.Run(JobExceptionState, Logger, cancellationToken).ConfigureAwait(false);
            JobExceptionState.Clear();
            Logger.Verbose("After TriggeredJob.Run()");
            return result;
          },
          JobExceptionState,
          this,
          Logger).ConfigureAwait(false);

          if (ShouldStop(cancellationToken, jobResult)) break;

          // Now let it handle any exception that occurred
          if (JobExceptionState.LastException != null)
          {
            if (!TriggeredJob.HandleException(JobExceptionState, Logger)) break;
          }

          // Wait for a trigger to resume
          await TaskExecContext.ExecAsync(async () =>
          {
            Logger.Verbose("Before WhenAny(triggers)");
            await await Task.WhenAny(TriggeredJob.GetTriggers(cancellationToken)).ConfigureAwait(false);
            Logger.Verbose("After WhenAny(triggers)");
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
