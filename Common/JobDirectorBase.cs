using System.Threading;
using System.Threading.Tasks;

namespace Tks.G1Track.Mobile.Shared.Common
{
  public interface IJobDirector
  {
    Task StartAsync();
    Task StopAsync();
  }

  public abstract class JobDirectorBase : IJobDirector
  {
    #region =====[ ctor ]==========================================================================================

    public JobDirectorBase(ILogger logger)
    {
      Logger = logger;
    }

    #endregion

    #region =====[ Private Properties ]============================================================================

    private Task Task { get; set; }

    #endregion

    #region =====[ Protected Properties ]============================================================================

    protected CancellationTokenSource CancellationTokenSource { get; set; }
    protected ILogger Logger { get; set; }

    #endregion

    #region =====[ IJobDirector Methods ]============================================================================

    public Task StartAsync()
    {
      CancellationTokenSource = new CancellationTokenSource();
      Task = InternalStartAsync(CancellationTokenSource.Token);
      return Task;
    }

    public async Task StopAsync()
    {
      Logger.Verbose("Before CancellationTokenSource.Cancel()");
      Cancel();
      Logger.Verbose("After CancellationTokenSource.Cancel()");

      Logger.Verbose("Before awaiting Task");
      await Task;
      Logger.Verbose("After awaiting Task");
    }

    #endregion

    #region =====[ Protected Methods ]===============================================================================

    protected void Cancel()
    {
      CancellationTokenSource.Cancel();
    }

    protected abstract Task InternalStartAsync(CancellationToken cancellationToken);

    protected bool ShouldStop(CancellationToken cancellationToken, JobResult jobResult = null)
    {
      if (cancellationToken.IsCancellationRequested) return true;
      if (jobResult != null && !jobResult.Continue) return true;
      return false;
    }

    #endregion
  }
}
