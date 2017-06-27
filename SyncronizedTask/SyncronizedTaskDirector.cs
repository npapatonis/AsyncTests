using System.Threading;
using System.Threading.Tasks;

namespace Tks.G1Track.Mobile.Shared.Common
{
  internal class SyncronizedTaskDirector<TData> : TaskDirectorBase
  {
    #region =====[ ctor ]==========================================================================================

    public SyncronizedTaskDirector(
      ISyncronizedProducer<TData> producer,
      ISyncronizedConsumer<TData> consumer,
      ILogger logger)
      : base(logger)
    {
      Producer = Producer;
      Consumer = Consumer;
      ProducerContext = new DirectedTaskContext();
      ConsumerContext = new DirectedTaskContext();
    }

    #endregion

    #region =====[ Private Properties ]============================================================================

    private ISyncronizedProducer<TData> Producer { get; set; }
    private ISyncronizedConsumer<TData> Consumer { get; set; }
    private IDirectedTaskContext ProducerContext { get; set; }
    private IDirectedTaskContext ConsumerContext { get; set; }

    #endregion

    #region =====[ Protected Methods ]===============================================================================

    protected override Task InternalStartAsync(CancellationToken cancellationToken)
    {
      return Task.Run(async () =>
      {
        bool producerExit = false;

        var producerTask = Producer.Run(Logger, cancellationToken);
        Task<bool> consumerTask = Task.FromResult(true);

        while (!cancellationToken.IsCancellationRequested)
        {
          // Wait for consumer first since it regulates the overall data flow
          bool consumerContinue = await DoAsyncOperation(ConsumerContext, async () =>
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
          if (!consumerContinue) Cancel();

          // Wait for producer
          var producerResult = await DoAsyncOperation(ProducerContext, async () =>
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
            producerTask = Producer.Run(Logger, cancellationToken);
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
          consumerTask = Consumer.Run(producerResult.Data, Logger, cancellationToken);
        }
      }, cancellationToken);
    }

    #endregion
  }
}
