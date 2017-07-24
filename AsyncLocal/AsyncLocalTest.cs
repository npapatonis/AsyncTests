using System;
using System.Threading;
using System.Threading.Tasks;
using Tks.G1Track.Mobile.Shared.Common;

namespace AsyncTests.AsyncLocal
{
  internal class AsyncLocalTest
  {
    private ILogger Logger;
    private WorkScope WorkScope;

    internal void Test()
    {
      Logger = new Logger();
      WorkScope = new WorkScope(Logger);

      CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

      Task testTask1 = RunTestTask(1, 2000, cancellationTokenSource.Token);
      Task testTask2 = RunTestTask(2, 1000, cancellationTokenSource.Token);

      Task.WhenAll(testTask1, testTask2);

      Console.ReadLine();
    }

    internal Task RunTestTask(int instance, int workDuration, CancellationToken cancellationToken)
    {
      return Task.Run(async () =>
      {
        await WorkScope.DoUnitOfWorkAsync(async c =>
        {
          var msgPrefix = $"**{instance} {nameof(RunTestTask)}.{nameof(WorkScope)}.{nameof(WorkScope.DoUnitOfWorkAsync)} Before work";
          Logger.Verbose($"{msgPrefix} Before work");
          await Task.Delay(workDuration).ConfigureAwait(false);
          Logger.Verbose($"{msgPrefix} After work");
        }, cancellationToken).ConfigureAwait(false);
      }, cancellationToken);
    }
  }
}
