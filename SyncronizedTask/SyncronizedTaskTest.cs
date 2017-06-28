using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tks.G1Track.Mobile.Shared.Common;

namespace AsyncTests.SyncronizedTask
{
  internal class SyncronizedTaskTest
  {
    internal void Test()
    {
      ILogger logger = new Logger();
      ISyncronizedProducer<List<int>> producer = new TestSyncronizedProducer();
      ISyncronizedConsumer<List<int>> consumer = new TestSyncronizedConsumer(52);

      logger.Information("SyncronizedTaskTest.Test, Press a key to stop...");

      ITaskDirector director = new SyncronizedTaskDirector<List<int>>(producer, consumer, logger);
      Task task = director.StartAsync();

      Task.WaitAny(KeyPressed(), task);

      logger.Information("----");
      director.StopAsync().Wait();
      task.Wait();

      logger.Information("SyncronizedTaskTest.Test, Press a key to exit...");
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
