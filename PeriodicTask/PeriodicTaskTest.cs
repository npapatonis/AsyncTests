using System;
using System.Threading.Tasks;
using Tks.G1Track.Mobile.Shared.Common;

namespace AsyncTests.PeriodicTask
{
  internal class PeriodicTaskTest
  {
    internal void Test()
    {
      ILogger logger = new Logger();
      IPeriodicTask periodicTask = new PeriodicTask();
      (periodicTask as PeriodicTask).HttpGetCompleted += HandleHttpGetCompleted;

      Console.WriteLine("PeriodicTaskTest.Test, Press a key to stop...");

      ITaskDirector director = new PeriodicTaskDirector(periodicTask, logger);
      Task task = director.StartAsync();

      Task.WaitAny(KeyPressed(), task);

      Console.WriteLine();
      director.StopAsync().Wait();
      task.Wait();

      Console.WriteLine("PeriodicTaskTest.Test, Press a key to exit...");
      KeyPressed().Wait();
    }

    private void HandleHttpGetCompleted(object sender, HttpGetCompletedEventArgs eventArgs)
    {
      Console.WriteLine("Start handling HttpGetCompleted event...");

      var responseMessage = eventArgs.HttpResponseMessage;
      var content = responseMessage.Content.ReadAsByteArrayAsync().Result;

      Task.Delay(2000, eventArgs.CancellationToken).Wait();

      throw new InvalidOperationException("Invalid operation!!!!");

      Console.WriteLine("Finish handling HttpGetCompleted event...");
      Console.ReadLine();
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
