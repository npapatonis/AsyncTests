using System;
using System.Threading;
using System.Threading.Tasks;

namespace Tks.G1Track.Mobile.Shared.Common
{
  internal interface ICancellationSource
  {
    bool IsCancellationRequested { get; }
  }

  internal class TaskExecContext : TaskExecContext<object>
  {
    #region =====[ ctor ]==========================================================================================

    internal TaskExecContext(
      Func<Task> operation,
      ICancellationSource cancellationSource,
      ILogger logger)
      : base(async () =>
      {
        await operation().ConfigureAwait(false);
        return Task.FromResult<object>(null);
      }, cancellationSource, logger)
    {
    }

    #endregion
  }

  internal class TaskExecContext<TReturn>
  {
    #region =====[ ctor ]==========================================================================================

    internal TaskExecContext(
      Func<Task<TReturn>> operation,
      ICancellationSource cancellationSource,
      ILogger logger)
    {
      Operation = operation;
      CancellationSource = cancellationSource;
      Logger = logger;
    }

    #endregion

    #region =====[ Private Properties ]============================================================================

    private ICancellationSource CancellationSource { get; set; }
    private Func<Task<TReturn>> Operation { get; set; }

    #endregion

    #region =====[ Protected Properties ]============================================================================

    protected ILogger Logger { get; set; }

    #endregion

    #region =====[ Internal Methods ]===============================================================================

    internal virtual async Task<TReturn> ExecAsync(IJobExceptionState jobExceptionState)
    {
      try
      {
        Logger.Verbose("Before try operation");
        var result = await Operation().ConfigureAwait(false);
        Logger.Verbose("After try operation");
        return result;
      }
      catch (OperationCanceledException operationCanceledException)
      {
        Logger.Verbose("Caught OperationCanceledException");
        if (!CancellationSource.IsCancellationRequested)
        {
          Logger.Verbose("Before log OperationCanceledException");
          HandleException(jobExceptionState, operationCanceledException, Logger.Warning);
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
            if (!CancellationSource.IsCancellationRequested)
            {
              Logger.Verbose("Before log aggregate's OperationCanceledException");
              HandleException(jobExceptionState, e, Logger.Warning);
              Logger.Verbose("After log aggregate's OperationCanceledException");
            }
            return true;
          }

          Logger.Verbose("Before log aggregate's other inner exception");
          HandleException(jobExceptionState, e, Logger.Error);
          Logger.Verbose("After log aggregate's other inner exception");
          return false;
        });
      }
      catch (Exception exception)
      {
        Logger.Verbose("Before log Exception");
        HandleException(jobExceptionState, exception, Logger.Error);
        Logger.Verbose("After log Exception");
      }

      return default(TReturn);
    }

    #endregion

    #region =====[ Private Methods ]=================================================================================

    private void HandleException(IJobExceptionState jobExceptionState, Exception exception, Action<string> logAction)
    {
      // If no jobExceptionState, ignore this operation's exception
      if (jobExceptionState == JobExceptionState.None) return;

      jobExceptionState.LastException = exception;
      jobExceptionState.ExceptionCount++;

      string message = exception.ExpandMessage();
      if (message != jobExceptionState.LastExceptionMessage)
      {
        jobExceptionState.LastExceptionCount = 1;
        jobExceptionState.LastExceptionMessage = message;
        logAction(message);
      }
      else
      {
        jobExceptionState.LastExceptionCount++;
      }
    }

    #endregion
  }
}
