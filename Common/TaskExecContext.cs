using System;
using System.Threading;
using System.Threading.Tasks;

namespace Tks.G1Track.Mobile.Shared.Common
{
  internal interface ICancellationSource
  {
    bool IsCancellationRequested { get; }
  }

  internal static class TaskExecContext
  {
    #region =====[ Internal Methods ]===============================================================================

    internal static async Task ExecAsync(
      Func<Task> operation,
      IJobExceptionState jobExceptionState,
      ICancellationSource cancellationSource,
      ILogger logger)
    {
      await ExecAsync(async () =>
      {
        await operation().ConfigureAwait(false);
        return 0;
      },
      jobExceptionState,
      cancellationSource,
      logger);
    }

    internal static async Task<TReturn> ExecAsync<TReturn>(
      Func<Task<TReturn>> operation,
      IJobExceptionState jobExceptionState,
      ICancellationSource cancellationSource,
      ILogger logger)
    {
      try
      {
        logger.Verbose("Before try operation");
        var result = await operation().ConfigureAwait(false);
        logger.Verbose("After try operation");
        return result;
      }
      catch (OperationCanceledException operationCanceledException)
      {
        logger.Verbose("Caught OperationCanceledException");
        if (!cancellationSource.IsCancellationRequested)
        {
          logger.Verbose("Before log OperationCanceledException");
          HandleException(jobExceptionState, operationCanceledException, logger.Warning);
          logger.Verbose("After log OperationCanceledException");
        }
      }
      catch (AggregateException aggregateException)
      {
        logger.Verbose("Caught AggregateException");
        aggregateException.Handle((e) =>
        {
          if (e is OperationCanceledException)
          {
            if (!cancellationSource.IsCancellationRequested)
            {
              logger.Verbose("Before log aggregate's OperationCanceledException");
              HandleException(jobExceptionState, e, logger.Warning);
              logger.Verbose("After log aggregate's OperationCanceledException");
            }
            return true;
          }

          logger.Verbose("Before log aggregate's other inner exception");
          HandleException(jobExceptionState, e, logger.Error);
          logger.Verbose("After log aggregate's other inner exception");
          return false;
        });
      }
      catch (Exception exception)
      {
        logger.Verbose("Before log Exception");
        HandleException(jobExceptionState, exception, logger.Error);
        logger.Verbose("After log Exception");
      }

      return default(TReturn);
    }

    #endregion

    #region =====[ Private Methods ]=================================================================================

    private static void HandleException(IJobExceptionState jobExceptionState, Exception exception, Action<string> logAction)
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
