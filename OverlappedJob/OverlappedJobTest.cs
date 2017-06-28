using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tks.G1Track.Mobile.Shared.Common;

namespace AsyncTests.OverlappedTask
{
  internal class OverlappedJobTest
  {
    internal void Test()
    {
      ILogger logger = new Logger();
      IOverlappedProducer<List<int>> producer = new TestProducer();
      IOverlappedConsumer<List<int>> consumer = new TestConsumer(52);

      logger.Information("OverlappedJobTest.Test, Press a key to stop...");

      IJobDirector director = new OverlappedJobDirector<List<int>>(producer, consumer, logger);
      Task task = director.StartAsync();

      Task.WaitAny(KeyPressed(), task);

      logger.Information("----");
      director.StopAsync().Wait();
      task.Wait();

      logger.Information("OverlappedJobTest.Test, Press a key to exit...");
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
