using System;
using System.Threading;
using System.Threading.Tasks;
using Tks.G1Track.Mobile.Shared.Common;

namespace AsyncTests.ApplyFilter
{
  internal class ApplyFilterTest
  {
    private ILogger Logger = new Logger();
    private string SearchFilter = "";

    private object ApplyFilterSignalMutex = new object();
    private TaskCompletionSource<object> ApplyFilterSignal;

    private Task BackgroundFilterTask = null;

    internal void Test()
    {
      CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

      RunBackgroundFilter(cancellationTokenSource.Token);

      while (true)
      {
        ConsoleKeyInfo? consoleKeyInfo = KeyPressed(CancellationToken.None).Result;
        if (consoleKeyInfo.Value.Key == ConsoleKey.Escape) break;
        SearchFilter += consoleKeyInfo.Value.KeyChar;
        ApplyFilter(SearchFilter);
      }

      cancellationTokenSource.Cancel();
      try { BackgroundFilterTask.Wait(); }
      catch (AggregateException) { }
      catch (TaskCanceledException) { }

      Console.ReadLine();
    }

    public void ApplyFilter(string searchFilter)
    {
      Logger.Information($"ApplyFilter('{searchFilter}') - Start");

      Logger.Information($"ApplyFilter('{searchFilter}') - Getting mutex lock");
      lock (ApplyFilterSignalMutex)
      {
        Logger.Information($"ApplyFilter('{searchFilter}') - Before signal TrySetResult");
        ApplyFilterSignal.TrySetResult(null);
        Logger.Information($"ApplyFilter('{searchFilter}') - After signal TrySetResult");
      }
    }

    public void RunBackgroundFilter(CancellationToken cancellationToken)
    {
      ApplyFilterSignal = CreateApplyFilterSignal();

      BackgroundFilterTask = Task.Run(async () =>
      {
        try
        {
          Logger.Information($"RunBackgroundFilter - Start");

          while (!cancellationToken.IsCancellationRequested)
          {
            Logger.Information($"RunBackgroundFilter - Awaiting {nameof(ApplyFilterSignal)}");
            await ApplyFilterSignal.Task.WithCancellation(Logger, cancellationToken).ConfigureAwait(false);

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
                await Task.Delay(75, cancellationToken);

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
                  // !!TODO!! Small timing window here.  If a user presses a key after this comparison and before the
                  // signal is recreated, the signal will be lost.
                  Logger.Information($"RunBackgroundFilter - Recreating ApplyFilterSignal");
                  ApplyFilterSignal = CreateApplyFilterSignal();

                  Logger.Information($"RunBackgroundFilter('{searchFilter}') - (w/ filter) Filter unchanged, assign List");
                  //FilteredInmates = new MvxObservableCollection<InmateViewModel>(tempList);
                  break;
                }
                else
                {
                  Logger.Information($"RunBackgroundFilter('{searchFilter}') - (w/ filter) Filter changed, do over");
                }
              }
            }
          }
          Logger.Information($"RunBackgroundFilter - Exit");
        }
        catch (Exception exception)
        {
          Logger.Information($"RunBackgroundFilter - Caugth exception {exception.Message}");
          throw;
        }
      }, cancellationToken);
    }

    private TaskCompletionSource<object> CreateApplyFilterSignal()
    {
      lock (ApplyFilterSignalMutex)
      {
        Logger.Information($"CreateApplyFilterSignal - Recreating ApplyFilterSignal");
        return new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
      }
    }

    private async Task<ConsoleKeyInfo?> KeyPressed(CancellationToken cancellationToken)
    {
      await Task.Yield();

      ConsoleKeyInfo? consoleKeyInfo = null;

      while (!cancellationToken.IsCancellationRequested)
      {
        if (Console.KeyAvailable)
        {
          consoleKeyInfo = Console.ReadKey(true);
          break;
        }
        await Task.Delay(125, cancellationToken);
      }

      return consoleKeyInfo;
    }
  }

  internal static class TaskExtensions
  {
    public static async Task<T> WithCancellation<T>(this Task<T> task, ILogger logger, CancellationToken cancellationToken)
    {
      var tcs = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);

      //using (cancellationToken.Register(s => ((TaskCompletionSource<bool>)s).TrySetResult(true), tcs)) { }
      logger.Information($"WithCancellation - Register signal cancellation delegate");
      using (cancellationToken.Register(s =>
      {
        logger.Information($"WithCancellation - Cancelling ApplyFilterSignal");
        ((TaskCompletionSource<object>)s).TrySetCanceled(cancellationToken);
      }, tcs))
      {
        logger.Information($"WithCancellation - Before Task.WhenAny");
        if (task != await Task.WhenAny(task, tcs.Task).ConfigureAwait(false))
          throw new OperationCanceledException(cancellationToken);
        logger.Information($"WithCancellation - After Task.WhenAny");

        return await task.ConfigureAwait(false);
      }
    }
  }
}
