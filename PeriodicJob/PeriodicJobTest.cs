using System;
using System.Threading.Tasks;
using Tks.G1Track.Mobile.Shared.Common;

namespace AsyncTests.PeriodicJob
{
  internal class PeriodicJobTest
  {
    internal void Test()
    {
      ILogger logger = new Logger();
      IPeriodicJob periodicJob = new PeriodicJob();
      (periodicJob as PeriodicJob).HttpGetCompleted += HandleHttpGetCompleted;

      Console.WriteLine("PeriodicJobTest.Test, Press a key to stop...");

      IJobDirector director = new PeriodicJobDirector(periodicJob, logger);
      Task task = director.StartAsync();

      Task keyPressTask = new KeyPress().GetAsync(ConsoleKey.Escape);
      if (Task.WaitAny(keyPressTask, task) == 0) keyPressTask = new KeyPress().GetAsync(ConsoleKey.Escape);

      Console.WriteLine();
      director.StopAsync().Wait();
      task.Wait();

      Console.WriteLine("PeriodicJobTest.Test, Press Escape to exit...");
      keyPressTask.Wait();
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
  }
}
