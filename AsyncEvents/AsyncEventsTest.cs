using System;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncTests.AsyncEvents
{
  internal class AsyncEventsTest
  {
    internal void Test()
    {
      CancellationTokenSource cancellationSource = new CancellationTokenSource();

      Console.WriteLine("AsyncEventsTest.Test, Press a key to exit...");

      AsyncEvents asyncEvents = new AsyncEvents();
      asyncEvents.SomeEvent += SomeEventHandler;
      Task task = asyncEvents.Run(cancellationSource.Token);

      Console.ReadKey();
      cancellationSource.Cancel();

      Console.WriteLine("AsyncEventsTest.Test, Waiting for task completion...");
      try
      {
        task.Wait();
      }
      catch (OperationCanceledException)
      { }
      catch (AggregateException aggregateException)
      {
        aggregateException.Handle((e) => e is OperationCanceledException);
      }
      Console.WriteLine("AsyncEventsTest.Test, Task finished");
    }

    private async void SomeEventHandler(object sender, EventArgs args)
    {
      Console.WriteLine($"AsyncEventsTest.SomeEventHandler, Thread = {Thread.CurrentThread.ManagedThreadId}");
      await Task.Delay(1000);
    }
  }
}
