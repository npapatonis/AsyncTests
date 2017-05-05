using System;
using System.Threading;
using System.Threading.Tasks;

namespace Tks.G1Track.Mobile.Shared.ConfigurationCache
{
  public interface IBackgroundTaskRunner
  {
    Task StartAsync(IBackgroundTask backgroundTask);
    Task StopAsync();
  }

  public class BackgroundTaskRunner : IBackgroundTaskRunner
  {
    #region =====[ ctor ]==========================================================================================

    public BackgroundTaskRunner(ILogger logger)
    {
      Logger = logger;
      TaskContext = new BackgroundTaskContext();
    }

    #endregion

    #region =====[ Private Properties ]============================================================================

    private CancellationTokenSource CancellationTokenSource { get; set; }
    private string LastExceptionMessage { get; set; }
    private ILogger Logger { get; set; }
    private IBackgroundTaskContext TaskContext { get; set; }
    private Task Task { get; set; }

    #endregion

    #region =====[ IBackgroundTaskRunner ]=========================================================================

    public Task StartAsync(IBackgroundTask backgroundTask)
    {
      CancellationTokenSource = new CancellationTokenSource();

      Task = Task.Run(async () =>
      {
        Logger.Verbose("Task.Run starting");

        while (!CancellationTokenSource.Token.IsCancellationRequested)
        {
          // Let the task run
          bool cont = true;
          await DoAsyncOperation(async () =>
          {
            Logger.Verbose("Before backgroundTask.Run()");

            cont = await backgroundTask.Run(TaskContext, Logger, CancellationTokenSource.Token);

            TaskContext.LastException = null;
            TaskContext.LastExceptionCount = 0;
            TaskContext.ExceptionCount = 0;

            Logger.Verbose("After backgroundTask.Run()");
          });
          if (!cont) break;

          // Now let it handle any exception that occurred
          if (TaskContext.LastException != null)
          {
            cont = backgroundTask.HandleException(TaskContext, Logger);
          }
          if (!cont) break;

          // Sleep if necessary before running again
          await DoAsyncOperation(async () =>
          {
            Logger.Verbose("Before sleep");
            await Task.Delay(backgroundTask.SleepInterval, CancellationTokenSource.Token);
            Logger.Verbose("After sleep");
          });
          Console.WriteLine();
        }
      }, CancellationTokenSource.Token);

      return Task;
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
      TaskContext.LastException = exception;
      TaskContext.ExceptionCount++;

      string message = exception.ExpandMessage();
      if (message != LastExceptionMessage)
      {
        TaskContext.LastExceptionCount = 1;

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
        TaskContext.LastExceptionCount++;
      }
    }

    #endregion
  }
}
