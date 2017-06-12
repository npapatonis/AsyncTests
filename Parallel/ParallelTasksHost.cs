using System;
using System.Threading;
using System.Threading.Tasks;

namespace Tks.G1Track.Mobile.Shared.Common
{
  internal interface IParallelTasksHost<TData>
  {
    Task StartAsync(IParallelProducer<TData> producer, IParallelConsumer<TData> consumer);
    Task StopAsync();
  }

  internal class ParallelTasksHost<TData> : IParallelTasksHost<TData>
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

    public Task StartAsync(IParallelProducer<TData> producer, IParallelConsumer<TData> consumer)
    {
      CancellationTokenSource = new CancellationTokenSource();
      CancellationToken cancellationToken = CancellationTokenSource.Token;

      Task = Task.Run(async () =>
      {
        bool producerExit = false;

        var producerTask = producer.Run(Logger, cancellationToken);
        Task<bool> consumerTask = Task.FromResult(true);

        while (!cancellationToken.IsCancellationRequested)
        {
          // Wait for consumer first since it regulates the overall data flow
          bool consumerContinue = await DoAsyncOperation(async () =>
          {
            Logger.Verbose("Before awaiting consumer task");
            var result = await consumerTask;
            Logger.Verbose("After awaiting consumer task");
            return result;
          }).ConfigureAwait(false);
          if (cancellationToken.IsCancellationRequested) break;

          // Did the producer signal end of data during the last iteration?
          Logger.Verbose($"Testing producerExit flag: {producerExit}");
          if (producerExit) break;

          // If consumer does not want more data, cancel before awaiting producer
          Logger.Verbose($"Testing consumerContinue flag: {consumerContinue}");
          if (!consumerContinue) CancellationTokenSource.Cancel();

          // Wait for producer
          var producerResult = await DoAsyncOperation(async () =>
          {
            Logger.Verbose("Before awaiting producer task");
            var result = await producerTask;
            Logger.Verbose("After awaiting producer task");
            return result;
          }).ConfigureAwait(false);
          if (cancellationToken.IsCancellationRequested) break;

          // If producer has more data, run it again
          Logger.Verbose($"Testing producerResult.Continue flag: {producerResult.Continue}");
          if (producerResult.Continue)
          {
            Logger.Verbose("Run producer task again");
            producerTask = producer.Run(Logger, cancellationToken);
          }
          else
          {
            // If producer is out of data, set an exit flag and create
            // fake producer task so consumer can run one more time
            Logger.Verbose("Prepare for producer exit");
            producerExit = true;
            producerTask = Task.FromResult(default(ProducerResult<TData>));
          }
          // Run consumer
          Logger.Verbose("Run consumer task again");
          consumerTask = consumer.Run(producerResult.Data, Logger, cancellationToken);


          //// Let the tasks run
          //await DoAsyncOperation(async () => await Task.WhenAny(producerTask, consumerTask)).ConfigureAwait(false);
          //if (cancellationToken.IsCancellationRequested) break;

          //// If consumer is done, wait for producer
          //if (consumerTask.IsCompleted)
          //{
          //  // If consumer does not want more data, cancel producer
          //  if (!consumerTask.Result) CancellationTokenSource.Cancel();
          //  await DoAsyncOperation(async () => await producerTask).ConfigureAwait(false);
          //}
          //if (cancellationToken.IsCancellationRequested) break;

          //// If producer is done, wait for consumer
          //if (producerTask.IsCompleted)
          //{
          //  cont = producerTask.Result.Continue;
          //  await DoAsyncOperation(async () => await consumerTask).ConfigureAwait(false);
          //}
        }
      }, cancellationToken);

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
      await DoAsyncOperation<int>(async () =>
      {
        await operation().ConfigureAwait(false);
        return 0;
      }).ConfigureAwait(false);
    }

    private async Task<TReturn> DoAsyncOperation<TReturn>(Func<Task<TReturn>> operation)
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

      return default(TReturn);
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
