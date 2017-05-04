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
          try
          {
            Console.WriteLine();
            Console.WriteLine($"AsyncEvents.Run #2, Thread = {Thread.CurrentThread.ManagedThreadId}");

            await Task.Delay(2000, cancellationToken);
            Console.WriteLine($"AsyncEvents.Run #3, Thread = {Thread.CurrentThread.ManagedThreadId}");

            await OutOfProcessCall(cancellationToken);
            Console.WriteLine($"AsyncEvents.Run #4, Thread = {Thread.CurrentThread.ManagedThreadId}");

            SomeEvent?.Invoke(this, new EventArgs());
          }
          catch (OperationCanceledException operationCanceledException)
          {
            Console.WriteLine($"===> AsyncEvents.Run.OperationCanceledException, Thread = {Thread.CurrentThread.ManagedThreadId}");

            if (operationCanceledException.CancellationToken.Equals(cancellationToken))
            {
              Console.WriteLine($"===> AsyncEvents.Run.OperationCanceledException, Rethrowing, Thread = {Thread.CurrentThread.ManagedThreadId}");
              throw;
            }
            else
            {
              Console.WriteLine($"===> AsyncEvents.Run.OperationCanceledException, Logging/Retrying, Thread = {Thread.CurrentThread.ManagedThreadId}");
            }
          }
          catch (AggregateException aggregateException)
          {
            Console.WriteLine($"===> AsyncEvents.Run.AggregateException, Thread = {Thread.CurrentThread.ManagedThreadId}");
            aggregateException.Handle((e) => e is OperationCanceledException);
            throw;
          }
        }
      }, cancellationToken);
    }

    private Task OutOfProcessCall(CancellationToken cancellationToken)
    {
      Console.WriteLine($"AsyncEvents.OutOfProcessCall #1, Thread = {Thread.CurrentThread.ManagedThreadId}");
      Task task = Task.Delay(2000);
      task.Wait(1000);
      Console.WriteLine($"AsyncEvents.OutOfProcessCall #2, Thread = {Thread.CurrentThread.ManagedThreadId}");

      return Task.FromResult(0);
    }
  }
}
