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

    private object ApplyFilterSignalMutex = new object();
    private TaskCompletionSource<int> ApplyFilterSignal = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);

    private Task BackgroundFilterTask = null;

    internal void Test()
    {
      CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

      ApplyFilterSignal = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
      RunBackgroundFilter(cancellationTokenSource.Token);

      ApplyFilter("N");

      cancellationTokenSource.Cancel();
      BackgroundFilterTask.Wait();

      Console.ReadLine();
    }

    public void ApplyFilter(string searchFilter)
    {
      Logger.Information($"ApplyFilter('{searchFilter}') - Start");

      Logger.Information($"ApplyFilter('{searchFilter}') - Getting mutex lock");
      lock (ApplyFilterSignalMutex)
      {
        Logger.Information($"ApplyFilter('{searchFilter}') - Before signal TrySetResult");
        ApplyFilterSignal.TrySetResult(0);
        Logger.Information($"ApplyFilter('{searchFilter}') - After signal TrySetResult");
      }
    }

    public void RunBackgroundFilter(CancellationToken cancellationToken)
    {
      BackgroundFilterTask = Task.Run(async () =>
      {
        Logger.Information($"RunBackgroundFilter - Start");

        while (!cancellationToken.IsCancellationRequested)
        {
          Logger.Information($"RunBackgroundFilter - Awaiting {nameof(ApplyFilterSignal)}");
          await ApplyFilterSignal.Task.ConfigureAwait(false);

          while (true)
          {
            var searchFilter = SearchFilter;
            Logger.Information($"RunBackgroundFilter('{searchFilter}') - Copied SearchFilter");

            if (string.IsNullOrWhiteSpace(searchFilter))
            {
              Logger.Information($"RunBackgroundFilter('{searchFilter}') - (w/o filter) Assign List");
              //FilteredInmates = Inmates;
              break;
            }
            else
            {
              Logger.Information($"RunBackgroundFilter('{searchFilter}') - (w/ filter) Before filter query");
              await Task.Delay(1000);

              //var tempList = Inmates.Where(inmate =>
              //                    inmate.InmateDisplayNameUpper.Contains(searchFilter) ||
              //                    inmate.BedNameUpper.Contains(searchFilter) ||
              //                    inmate.PermanentNumberUpper.Contains(searchFilter) ||
              //                    inmate.BookingNumberUpper.Contains(searchFilter)
              //                    );
              Logger.Information($"RunBackgroundFilter('{searchFilter}') - (w/ filter) After filter query");

              Logger.Information($"RunBackgroundFilter('{searchFilter}') - (w/ filter) Cancellation Requested: {cancellationToken.IsCancellationRequested}");
              if (cancellationToken.IsCancellationRequested) break;

              if (searchFilter == SearchFilter)
              {
                Logger.Information($"RunBackgroundFilter('{searchFilter}') - (w/ filter) Filter unchanged, assign List");
                //FilteredInmates = new MvxObservableCollection<InmateViewModel>(tempList);
                break;
              }
              else
              {
                Logger.Information($"RunBackgroundFilter('{searchFilter}') - (w/ filter) Filter changed, start again");
              }
            }
          }

          Logger.Information($"ApplyFilter - Getting mutex lock");
          lock (ApplyFilterSignalMutex)
          {
            Logger.Information($"ApplyFilter - Recreating ApplyFilterSignal");
            ApplyFilterSignal = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
          }
        }
        Logger.Information($"RunBackgroundFilter - Exit");
      }, cancellationToken);
    }

    //public async Task ApplySearchFilterAsync(string searchFilter)
    //{
    //  await Task.Run(async () =>
    //  {
    //    Logger.Information($"ApplySearchFilterAsync('{searchFilter}') - Start");

    //    Logger.Information($"ApplySearchFilterAsync('{searchFilter}') - {nameof(currentApplyFilterTask)} != null: {currentApplyFilterTask != null}");
    //    if (currentApplyFilterTask != null)
    //    {
    //      Logger.Information($"ApplySearchFilterAsync('{searchFilter}') - Before semaphore wait");
    //      Task semaphoreTask = semaphore.WaitAsync();
    //      Task task = await Task.WhenAny(semaphoreTask, tcs.Task);


    //      Logger.Information($"ApplySearchFilterAsync('{searchFilter}') - After semaphore wait");

    //      Logger.Information($"ApplySearchFilterAsync('{searchFilter}') - {nameof(currentApplyFilterTask)} != null: {currentApplyFilterTask != null}");
    //      if (currentApplyFilterTask != null)
    //      {
    //        Logger.Information($"ApplySearchFilterAsync('{searchFilter}') - Cancel token");
    //        tokenSource?.Cancel();

    //        Logger.Information($"ApplySearchFilterAsync('{searchFilter}') - Before await {nameof(currentApplyFilterTask)}");
    //        await currentApplyFilterTask;
    //        Logger.Information($"ApplySearchFilterAsync('{searchFilter}') - After await {nameof(currentApplyFilterTask)}");

    //        Logger.Information($"ApplySearchFilterAsync('{searchFilter}') - Set {nameof(currentApplyFilterTask)} = null");
    //        currentApplyFilterTask = null;

    //        Logger.Information($"ApplySearchFilterAsync('{searchFilter}') - Create new cancellation token source");
    //        tokenSource = new CancellationTokenSource();
    //      }

    //      Logger.Information($"ApplySearchFilterAsync('{searchFilter}') - Release semaphore");
    //      semaphore.Release();
    //    }

    //    Logger.Information($"ApplySearchFilterAsync('{searchFilter}') - Call ApplyFilterAsync");
    //    currentApplyFilterTask = ApplyFilterAsync(SearchFilter.ToUpperInvariant(), tokenSource.Token);

    //    Logger.Information($"ApplySearchFilterAsync('{searchFilter}') - Before await {nameof(currentApplyFilterTask)}");
    //    await currentApplyFilterTask;
    //    Logger.Information($"ApplySearchFilterAsync('{searchFilter}') - After await {nameof(currentApplyFilterTask)}");

    //    Logger.Information($"ApplySearchFilterAsync('{searchFilter}') - Set {nameof(currentApplyFilterTask)} = null");
    //    currentApplyFilterTask = null;

    //    Logger.Information($"ApplySearchFilterAsync('{searchFilter}') - Exit");
    //  });
    //}

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
