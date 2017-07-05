using System;
using System.Threading.Tasks;
using Tks.G1Track.Mobile.Shared.Common;

namespace AsyncTests.TriggeredJob
{
  internal class TriggeredJobTest
  {
    public event EventHandler<EventArgs> JobTriggered;

    internal void Test()
    {
      ILogger logger = new Logger();
      ITriggeredJob triggeredJob = new TriggeredJob(this);

      Console.WriteLine("TriggeredJobTest.Test, Press a key to stop...");

      IJobDirector director = new TriggeredJobDirector(triggeredJob, logger);
      Task task = director.StartAsync();

      var keyPressTask = new KeyPress().GetAsync();
      while (true)
      {
        var taskIndex = Task.WaitAny(task, keyPressTask);
        if (taskIndex == 0) break;
        var keyInfo = keyPressTask.Result;
        if (keyInfo.Value.Key == ConsoleKey.T) JobTriggered?.Invoke(this, new EventArgs());
        keyPressTask = new KeyPress().GetAsync();
        if (keyInfo.Value.Key == ConsoleKey.Escape) break;
      }

      Console.WriteLine();
      director.StopAsync().Wait();
      task.Wait();

      Console.WriteLine("TriggeredJobTest.Test, Press Escape to exit...");
      keyPressTask.Wait();
    }

  }
}
