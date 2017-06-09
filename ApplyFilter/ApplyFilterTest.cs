using System;
using System.Threading;
using System.Threading.Tasks;
using Tks.G1Track.Mobile.Shared.Common;

namespace AsyncTests.ApplyFilter
{
  internal class ApplyFilterTest
  {
    private ILogger Logger = new Logger();
    private string SearchFilter;

    private Task currentApplyFilterTask = null;
    private CancellationTokenSource tokenSource = new CancellationTokenSource();

    private SemaphoreSlim semaphore = new SemaphoreSlim(1);


    internal void Test()
    {
      CancellationTokenSource cts = new CancellationTokenSource();

      ApplySearchFilterAsync("N").Wait();

      Console.ReadLine();
    }

    TaskCompletionSource<int> tcs = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);

    public async Task ApplySearchFilterAsync(string searchFilter)
    {
      await Task.Run(async () =>
      {
        Logger.Information($"ApplySearchFilterAsync('{searchFilter}') - Start");

        Logger.Information($"ApplySearchFilterAsync('{searchFilter}') - {nameof(currentApplyFilterTask)} != null: {currentApplyFilterTask != null}");
        if (currentApplyFilterTask != null)
        {
          Logger.Information($"ApplySearchFilterAsync('{searchFilter}') - Before semaphore wait");
          Task semaphoreTask = semaphore.WaitAsync();
          Task task = await Task.WhenAny(semaphoreTask, tcs.Task);


          Logger.Information($"ApplySearchFilterAsync('{searchFilter}') - After semaphore wait");

          Logger.Information($"ApplySearchFilterAsync('{searchFilter}') - {nameof(currentApplyFilterTask)} != null: {currentApplyFilterTask != null}");
          if (currentApplyFilterTask != null)
          {
            Logger.Information($"ApplySearchFilterAsync('{searchFilter}') - Cancel token");
            tokenSource?.Cancel();

            Logger.Information($"ApplySearchFilterAsync('{searchFilter}') - Before await {nameof(currentApplyFilterTask)}");
            await currentApplyFilterTask;
            Logger.Information($"ApplySearchFilterAsync('{searchFilter}') - After await {nameof(currentApplyFilterTask)}");

            Logger.Information($"ApplySearchFilterAsync('{searchFilter}') - Set {nameof(currentApplyFilterTask)} = null");
            currentApplyFilterTask = null;

            Logger.Information($"ApplySearchFilterAsync('{searchFilter}') - Create new cancellation token source");
            tokenSource = new CancellationTokenSource();
          }

          Logger.Information($"ApplySearchFilterAsync('{searchFilter}') - Release semaphore");
          semaphore.Release();
        }

        Logger.Information($"ApplySearchFilterAsync('{searchFilter}') - Call ApplyFilterAsync");
        currentApplyFilterTask = ApplyFilterAsync(SearchFilter.ToUpperInvariant(), tokenSource.Token);

        Logger.Information($"ApplySearchFilterAsync('{searchFilter}') - Before await {nameof(currentApplyFilterTask)}");
        await currentApplyFilterTask;
        Logger.Information($"ApplySearchFilterAsync('{searchFilter}') - After await {nameof(currentApplyFilterTask)}");

        Logger.Information($"ApplySearchFilterAsync('{searchFilter}') - Set {nameof(currentApplyFilterTask)} = null");
        currentApplyFilterTask = null;

        Logger.Information($"ApplySearchFilterAsync('{searchFilter}') - Exit");
      });
    }

    public Task ApplyFilterAsync(string searchFilter, CancellationToken token)
    {
      Task task;

      Logger.Information($"ApplyFilterAsync('{searchFilter}') - Start");

      if (string.IsNullOrWhiteSpace(searchFilter))
      {
        Logger.Information($"ApplyFilterAsync('{searchFilter}') - (w/o filter) Assign List");
        //FilteredInmates = Inmates;
        task = Task.CompletedTask;
      }
      else
      {
        task = Task.Run(async () =>
        {
          Logger.Information($"ApplyFilterAsync('{searchFilter}') - (w/ filter) Start");

          if (token.IsCancellationRequested) return;

          Logger.Information($"ApplyFilterAsync('{searchFilter}') - (w/ filter) Before filter query");
          await Task.Delay(1000);

          //var tempList = Inmates.Where(inmate =>
          //                    inmate.InmateDisplayNameUpper.Contains(searchFilter) ||
          //                    inmate.BedNameUpper.Contains(searchFilter) ||
          //                    inmate.PermanentNumberUpper.Contains(searchFilter) ||
          //                    inmate.BookingNumberUpper.Contains(searchFilter)
          //                    );
          Logger.Information($"ApplyFilterAsync('{searchFilter}') - (w/ filter) After filter query");

          Logger.Information($"ApplyFilterAsync('{searchFilter}') - (w/ filter) Cancellation Requested: {token.IsCancellationRequested}");
          if (token.IsCancellationRequested) return;

          Logger.Information($"ApplyFilterAsync('{searchFilter}') - (w/ filter) Assign List");
          //FilteredInmates = new MvxObservableCollection<InmateViewModel>(tempList);

          Logger.Information($"ApplyFilterAsync('{searchFilter}') - (w/ filter) Exit");
        },
        token);
      }

      Logger.Information($"ApplyFilterAsync('{searchFilter}') - Exit");
      return task;
    }
  }
}
