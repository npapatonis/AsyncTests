using System;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncTests.AsyncEvents
{
  internal class AsyncEvents
  {
    internal event EventHandler SomeEvent;

    internal Task Run(CancellationToken cancellationToken)
    {
      Console.WriteLine($"AsyncEvents.Run #1, Thread = {Thread.CurrentThread.ManagedThreadId}");

      return Task.Run(async () =>
      {
        while (!cancellationToken.IsCancellationRequested)
        {
          Console.WriteLine();
          Console.WriteLine($"AsyncEvents.Run #2, Thread = {Thread.CurrentThread.ManagedThreadId}");

          await Task.Delay(3000, cancellationToken);
          Console.WriteLine($"AsyncEvents.Run #3, Thread = {Thread.CurrentThread.ManagedThreadId}");
          SomeEvent?.Invoke(this, new EventArgs());
        }
      }, cancellationToken);
    }
  }
}
