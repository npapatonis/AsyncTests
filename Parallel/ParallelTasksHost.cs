using System;
using System.Threading;
using System.Threading.Tasks;
using Tks.G1Track.Mobile.Shared.ConfigurationCache;

namespace Tks.G1Track.Mobile.Shared.Common
{
  internal interface IParallelTasksHost<TResult1, TResult2>
    where TResult1 : ITaskResult
    where TResult2 : ITaskResult
  {
    Task StartAsync(IParallelTask<TResult1> parallelTask1, IParallelTask<TResult2> parallelTask2);
    Task StopAsync();
  }

  internal class ParallelTasksHost<TResult1, TResult2> : IParallelTasksHost<TResult1, TResult2>
    where TResult1 : ITaskResult
    where TResult2 : ITaskResult
  {
    #region =====[ ctor ]==========================================================================================

    public ParallelTasksHost(ILogger logger)
    {
      Logger = logger;
    }

    #endregion

    #region =====[ Private Properties ]============================================================================

    private CancellationTokenSource CancellationTokenSource { get; set; }
    private string LastExceptionMessage { get; set; }
    private ILogger Logger { get; set; }
    private Task Task { get; set; }

    #endregion

    #region =====[ IParallelTasksHost ]============================================================================

    public async Task StartAsync(IParallelTask<TResult1> parallelTask1, IParallelTask<TResult2> parallelTask2)
    {
      CancellationTokenSource = new CancellationTokenSource();
      CancellationToken cancellationToken = CancellationTokenSource.Token;

      bool cont = true;
      TResult1 taskResult1;
      TResult2 taskResult2;

      while (!cancellationToken.IsCancellationRequested)
      {
        // Let the task run
        await DoAsyncOperation(async () =>
        {
          Logger.Verbose("Before backgroundTask.Run()");
          taskResult1 = await parallelTask1.Run(Logger, cancellationToken);
          Logger.Verbose("After backgroundTask.Run()");
        });
        if (cancellationToken.IsCancellationRequested || !cont) break;

        //// Now let it handle any exception that occurred
        //if (TaskContext.LastException != null)
        //{
        //  cont = backgroundTask.HandleException(TaskContext, Logger);
        //}
        //if (cancellationToken.IsCancellationRequested || !cont) break;
      }

    }

    public async Task StopAsync()
    {
      Logger.Verbose("Before CancellationTokenSource.Cancel()");
      CancellationTokenSource.Cancel();
      Logger.Verbose("After CancellationTokenSource.Cancel()");

      Logger.Verbose("Before awaiting Task");
      await Task;
      Logger.Verbose("After awaiting Task");
    }

    #endregion

    #region =====[ Private Methods ]===============================================================================

    private async Task DoAsyncOperation(Func<Task> operation)
    {
      try
      {
        await operation();
      }
      catch (OperationCanceledException operationCanceledException)
      {
        Logger.Verbose("Caught OperationCanceledException");
        if (!CancellationTokenSource.IsCancellationRequested)
        {
          Logger.Verbose("Before log OperationCanceledException");
          HandleException(operationCanceledException, false);
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
              HandleException(e, false);
              Logger.Verbose("After log aggregate's OperationCanceledException");
            }
            return true;
          }

          Logger.Verbose("Before log aggregate's other inner exception");
          HandleException(e, true);
          Logger.Verbose("After log aggregate's other inner exception");
          return false;
        });
      }
      catch (Exception exception)
      {
        Logger.Verbose("Before log Exception");
        HandleException(exception, true);
        Logger.Verbose("After log Exception");
      }
    }

    private void HandleException(Exception exception, bool isError)
    {
      //TaskContext.LastException = exception;
      //TaskContext.ExceptionCount++;

      string message = exception.ExpandMessage();
      if (message != LastExceptionMessage)
      {
        //TaskContext.LastExceptionCount = 1;

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
        //TaskContext.LastExceptionCount++;
      }
    }

    #endregion
  }
}
