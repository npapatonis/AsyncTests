using System;
using System.Threading;
using System.Threading.Tasks;
using Tks.G1Track.Mobile.Shared.Common;

namespace AsyncTests.ApplyFilter
{
  internal class ApplyFilterSignal
  {
    internal ApplyFilterSignal(ILogger logger)
    {
      Logger = logger;
      Clear();
    }

    private ILogger Logger;
    private object Mutex = new object();
    private TaskCompletionSource<object> Signal;

    internal void Set()
    {
      Logger.Information($"Set - Getting mutex lock");
      lock (Mutex)
      {
        Logger.Information($"Set - Before signal TrySetResult");
        Signal.TrySetResult(null);
        Logger.Information($"Set - After signal TrySetResult");
      }
    }

    internal void Clear()
    {
      lock (Mutex)
      {
        Logger.Information($"Clear - Creating ApplyFilterSignal");
        Signal = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
      }
    }

    internal async Task WaitAsync(CancellationToken cancellationToken)
    {
      var tcs = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);

      //using (cancellationToken.Register(s => ((TaskCompletionSource<bool>)s).TrySetResult(true), tcs)) { }
      Logger.Information($"WaitAsync - Register signal cancellation delegate");
      using (cancellationToken.Register(s =>
      {
        Logger.Information($"WaitAsync - Cancelling ApplyFilterSignal");
        ((TaskCompletionSource<object>)s).TrySetCanceled(cancellationToken);
      }, tcs))
      {
        Logger.Information($"WaitAsync - Before Task.WhenAny");
        if (Signal.Task != await Task.WhenAny(Signal.Task, tcs.Task).ConfigureAwait(false))
          throw new OperationCanceledException(cancellationToken);
        Logger.Information($"WaitAsync - After Task.WhenAny");

        await Signal.Task.ConfigureAwait(false);
      }
    }
  }
}
