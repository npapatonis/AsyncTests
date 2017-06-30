using System;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncTests
{
  internal class KeyPress
  {
    internal Task<ConsoleKeyInfo?> GetAsync()
    {
      return GetAsync(CancellationToken.None);
    }

    internal Task<ConsoleKeyInfo?> GetAsync(CancellationToken cancellationToken)
    {
      return GetAsync(null, cancellationToken);
    }

    internal Task<ConsoleKeyInfo?> GetAsync(ConsoleKey? consoleKey)
    {
      return GetAsync(consoleKey, CancellationToken.None);
    }

    internal async Task<ConsoleKeyInfo?> GetAsync(ConsoleKey? consoleKey, CancellationToken cancellationToken)
    {
      await Task.Yield();

      ConsoleKeyInfo? consoleKeyInfo = null;

      while (!cancellationToken.IsCancellationRequested)
      {
        if (Console.KeyAvailable)
        {
          consoleKeyInfo = Console.ReadKey(true);
          if (!consoleKey.HasValue || consoleKeyInfo.Value.Key == consoleKey.Value) break;
        }
        try { await Task.Delay(125, cancellationToken); }
        catch (OperationCanceledException) { }
      }

      return consoleKeyInfo;
    }
  }
}
