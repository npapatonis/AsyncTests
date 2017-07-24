using System;
using System.Threading;
using System.Threading.Tasks;
using Tks.G1Track.Mobile.Shared.Common;

namespace AsyncTests.AsyncLocal
{
  internal class WorkScope
  {
    internal WorkScope(ILogger logger)
    {
      Logger = logger;
    }

    private ILogger Logger { get; set; }
    private SemaphoreSlim Lock { get; } = new SemaphoreSlim(1);

    private AsyncLocal<int> m_unitOfWorkCount = new AsyncLocal<int>();
    private int UnitOfWorkCount
    {
      get
      {
        Logger.Verbose($"{nameof(UnitOfWorkCount)} Getter: Value = {m_unitOfWorkCount.Value}");
        return m_unitOfWorkCount.Value;
      }
      set
      {
        m_unitOfWorkCount.Value = value;
        Logger.Verbose($"{nameof(UnitOfWorkCount)} Setter: Value = {m_unitOfWorkCount.Value}");
      }
    }

    public async Task DoUnitOfWorkAsync(Func<CancellationToken, Task> workUnit, CancellationToken cancellationToken)
    {
      await DoUnitOfWorkAsync(async (c) =>
      {
        await workUnit(cancellationToken).ConfigureAwait(false);
        return 0;
      }, cancellationToken).ConfigureAwait(false);
    }

    public async Task<TReturn> DoUnitOfWorkAsync<TReturn>(Func<CancellationToken, Task<TReturn>> workUnit, CancellationToken cancellationToken)
    {
      try
      {
        await BeginUnitOfWorkAsync(cancellationToken).ConfigureAwait(false);
        return await workUnit(cancellationToken).ConfigureAwait(false);
      }
      finally
      {
        await EndUnitOfWorkAsync(cancellationToken).ConfigureAwait(false);
      }
    }

    public async Task BeginUnitOfWorkAsync(CancellationToken cancellationToken)
    {
      Logger.Verbose($"{nameof(BeginUnitOfWorkAsync)}");
      if (UnitOfWorkCount == 0)
      {
        Logger.Verbose($"{nameof(BeginUnitOfWorkAsync)} awaiting Lock");
        await Lock.WaitAsync(cancellationToken).ConfigureAwait(false);
        Logger.Verbose($"{nameof(BeginUnitOfWorkAsync)} acquired Lock");
      }
      UnitOfWorkCount++;
    }

    public Task EndUnitOfWorkAsync(CancellationToken cancellationToken)
    {
      Logger.Verbose($"{nameof(EndUnitOfWorkAsync)}");
      if (UnitOfWorkCount == 1)
      {
        Logger.Verbose($"{nameof(EndUnitOfWorkAsync)} releasing Lock");
        Lock.Release();
        Logger.Verbose($"{nameof(EndUnitOfWorkAsync)} released Lock");
      }
      UnitOfWorkCount--;
      return Task.CompletedTask;
    }
  }
}
