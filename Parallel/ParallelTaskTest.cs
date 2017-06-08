using AsyncTests.BackgroundTask;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tks.G1Track.Mobile.Shared.Common;

namespace AsyncTests.Parallel
{
  internal class ParallelTaskTest
  {
    internal void Test()
    {
      ILogger logger = new Logger();
      IParallelProducer<List<int>> producer = new TestProducer();
      IParallelConsumer<List<int>> consumer = new TestConsumer(52);

      logger.Information("ParallelTaskTest.Test, Press a key to stop...");

      IParallelTasksHost<List<int>> host = new ParallelTasksHost<List<int>>(logger);
      Task task = host.StartAsync(producer, consumer);

      Task.WaitAny(KeyPressed(), task);

      logger.Information("----");
      host.StopAsync().Wait();
      task.Wait();

      logger.Information("BackgroundTaskTest.Test, Press a key to exit...");
      KeyPressed().Wait();
    }

    private async Task KeyPressed()
    {
      await Task.Yield();

      while (true)
      {
        if (Console.KeyAvailable)
        {
          Console.ReadKey(true);
          break;
        }
        await Task.Delay(125);
      }
    }
  }
}
