using System;
using System.Threading;
using System.Threading.Tasks;

namespace Tks.G1Track.Mobile.Shared.Common
{
  public interface ITaskDirector
  {
    Task StartAsync();
    Task StopAsync();
  }

  public abstract class TaskDirectorBase : ITaskDirector
  {
    #region =====[ ctor ]==========================================================================================

    public TaskDirectorBase(ILogger logger)
    {
      Logger = logger;
    }

    #endregion

    #region =====[ Private Properties ]============================================================================

    private CancellationTokenSource CancellationTokenSource { get; set; }
    private Task Task { get; set; }

    #endregion

    #region =====[ Protected Properties ]============================================================================

    protected string LastExceptionMessage { get; set; }
    protected ILogger Logger { get; set; }

    #endregion

    #region =====[ ITaskDirector Methods ]===========================================================================

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

    protected async Task DoAsyncOperation(IDirectedTaskContext taskContext, Func<Task> operation)
    {
      await DoAsyncOperation(taskContext, async () =>
      {
        await operation().ConfigureAwait(false);
        return 0;
      }).ConfigureAwait(false);
    }

    protected async Task<TReturn> DoAsyncOperation<TReturn>(
      IDirectedTaskContext taskContext,
      Func<Task<TReturn>> operation)
    {
      try
      {
        return await operation().ConfigureAwait(false);
      }
      catch (OperationCanceledException operationCanceledException)
      {
        Logger.Verbose("Caught OperationCanceledException");
        if (!CancellationTokenSource.IsCancellationRequested)
        {
          Logger.Verbose("Before log OperationCanceledException");
          HandleException(taskContext, operationCanceledException, false);
          Logger.Verbose("After log OperationCanceledException");
        }
      }
      catch (AggregateException aggregateException)
      {
        Logger.Verbose("Caught AggregateException");
        aggregateException.Handle((e) =>
        {
          if (e is OperationCanceledException)
          {
            if (!CancellationTokenSource.IsCancellationRequested)
            {
              Logger.Verbose("Before log aggregate's OperationCanceledException");
              HandleException(taskContext, e, false);
              Logger.Verbose("After log aggregate's OperationCanceledException");
            }
            return true;
          }

          Logger.Verbose("Before log aggregate's other inner exception");
          HandleException(taskContext, e, true);
          Logger.Verbose("After log aggregate's other inner exception");
          return false;
        });
      }
      catch (Exception exception)
      {
        Logger.Verbose("Before log Exception");
        HandleException(taskContext, exception, true);
        Logger.Verbose("After log Exception");
      }

      return default(TReturn);
    }

    protected abstract Task InternalStartAsync(CancellationToken cancellationToken);

    #endregion

    #region =====[ Private Methods ]=================================================================================

    private void HandleException(IDirectedTaskContext taskContext, Exception exception, bool isError)
    {
      taskContext.LastException = exception;
      taskContext.ExceptionCount++;

      string message = exception.ExpandMessage();
      if (message != LastExceptionMessage)
      {
        taskContext.LastExceptionCount = 1;

        if (isError)
        {
          Logger.Error(message);
        }
        else
        {
          Logger.Warning(message);
        }
        LastExceptionMessage = message;
      }
      else
      {
        taskContext.LastExceptionCount++;
      }
    }

    #endregion
  }
}
