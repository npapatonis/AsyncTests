using System;
using System.Threading.Tasks;
using Tks.G1Track.Mobile.Shared.Common;

namespace AsyncTests.BackgroundTask
{
  internal class BackgroundTaskTest
  {
    internal void Test()
    {
      ILogger logger = new Logger();
      IBackgroundTask backgroundTask = new BackgroundTask();

      Console.WriteLine("BackgroundTaskTest.Test, Press a key to stop...");

      IBackgroundTaskHost host = new BackgroundTaskHost(logger);
      Task task = host.StartAsync(backgroundTask);

      Task.WaitAny(KeyPressed(), task);

      Console.WriteLine();
      host.StopAsync().Wait();
      task.Wait();

      Console.WriteLine("BackgroundTaskTest.Test, Press a key to exit...");
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

  internal class Logger : ILogger
  {
    private void LogMessage(string prefix, ConsoleColor prefixColor, string message)
    {
      var currentColor = Console.ForegroundColor;
      Console.ForegroundColor = prefixColor;
      Console.Write(prefix);
      Console.ForegroundColor = currentColor;
      Console.WriteLine($" {message}");
    }

    public void Error(string message)
    {
      LogMessage("ERROR", ConsoleColor.Red, message);
    }

    public void Information(string message)
    {
      LogMessage("INFO", ConsoleColor.Cyan, message);
    }

    public void Verbose(string message)
    {
      LogMessage("DEBUG", ConsoleColor.White, message);
    }

    public void Warning(string message)
    {
      LogMessage("WARNING", ConsoleColor.Yellow, message);
    }
  }
}
