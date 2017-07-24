using System;
using System.Threading;
using System.Threading.Tasks;
using Tks.G1Track.Mobile.Shared.Common;

namespace AsyncTests.ApplyFilter
{
  internal class CountedSignal
  {
    internal CountedSignal(ILogger logger)
    {
      Logger = logger;
      m_setCount = 0;
      Clear(m_setCount);
    }

    private ILogger Logger;
    private object Mutex = new object();
    private TaskCompletionSource<object> Signal;
    private int m_setCount;

    internal void Set()
    {
      Logger.Information($"Set - Getting mutex lock");
      lock (Mutex)
      {
        Logger.Information($"Set - Before signal TrySetResult");
        Signal.TrySetResult(null);
        m_setCount++;
        Logger.Information($"Set - After signal TrySetResult");
      }
    }

    internal int SetCount
    {
      get
      {
        lock (Mutex)
        {
          Logger.Information($"SetCount Getter - returning {m_setCount}");
          return m_setCount;
        }
      }
    }

    internal bool Clear(int setCount)
    {
      var result = false;

      lock (Mutex)
      {
        Logger.Information($"Clear - Testing Set count {setCount} == {m_setCount}");
        if (setCount == m_setCount)
        {
          Logger.Information($"Clear - Creating ApplyFilterSignal");
          Signal = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
          m_setCount = 0;
          result = true;
        }
      }

      return result;
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
