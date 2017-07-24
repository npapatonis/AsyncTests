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

    private CountedSignal CountedSignal;

    private Task BackgroundFilterTask = null;

    internal void Test()
    {
      CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
      CountedSignal = new CountedSignal(Logger);

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
      Logger.Information($"ApplyFilter('{searchFilter}') - Setting ApplyFilterSignal");
      CountedSignal.Set();
    }

    public void RunBackgroundFilter(CancellationToken cancellationToken)
    {
      BackgroundFilterTask = Task.Run(async () =>
      {
        try
        {
          Logger.Information($"RunBackgroundFilter - Start");

          while (!cancellationToken.IsCancellationRequested)
          {
            Logger.Information($"RunBackgroundFilter - Awaiting {nameof(CountedSignal)}");
            await CountedSignal.WaitAsync(cancellationToken).ConfigureAwait(false);

            while (true)
            {
              var searchFilter = SearchFilter;
              Logger.Information($"RunBackgroundFilter('{searchFilter}') - Copied SearchFilter");

              var setCount = CountedSignal.SetCount;

              if (string.IsNullOrWhiteSpace(searchFilter))
              {
                Logger.Information($"RunBackgroundFilter('{searchFilter}') - (w/o filter) Assign List");
                //FilteredInmates = Inmates;
                if (CountedSignal.Clear(setCount)) break;
              }
              else
              {
                Logger.Information($"RunBackgroundFilter('{searchFilter}') - (w/ filter) Before filter query");
                await Task.Delay(250, cancellationToken);

                //var tempList = Inmates.Where(inmate =>
                //                    inmate.InmateDisplayNameUpper.Contains(searchFilter) ||
                //                    inmate.BedNameUpper.Contains(searchFilter) ||
                //                    inmate.PermanentNumberUpper.Contains(searchFilter) ||
                //                    inmate.BookingNumberUpper.Contains(searchFilter)
                //                    );
                Logger.Information($"RunBackgroundFilter('{searchFilter}') - (w/ filter) After filter query");

                Logger.Information($"RunBackgroundFilter('{searchFilter}') - (w/ filter) Cancellation Requested: {cancellationToken.IsCancellationRequested}");
                if (cancellationToken.IsCancellationRequested) break;

                if (setCount == CountedSignal.SetCount)
                {
                  Logger.Information($"RunBackgroundFilter('{searchFilter}') - (w/ filter) Filter unchanged, assign List");
                  //FilteredInmates = new MvxObservableCollection<InmateViewModel>(tempList);

                  // !!TODO!! Small timing window here.  If a user presses a key after this comparison and before the
                  // signal is recreated, the signal will be lost.
                  Logger.Information($"RunBackgroundFilter - TIMING WINDOW!!");
                  await Task.Delay(2000, cancellationToken);

                  Logger.Information($"RunBackgroundFilter - Clearing ApplyFilterSignal");
                  if (CountedSignal.Clear(setCount)) break;
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
}
