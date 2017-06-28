using AsyncTests.ApplyFilter;
using AsyncTests.MiscTests;
using AsyncTests.PeriodicJob;
using AsyncTests.OverlappedTask;

namespace AsyncTests
{
  class Program
  {
    static void Main(string[] args)
    {
      //AsyncEventsTest asyncEventsTest = new AsyncEventsTest();
      //asyncEventsTest.Test();

      //CancellationHandlingTest cancellationHandlingTest = new CancellationHandlingTest();
      //cancellationHandlingTest.Test();

      //BackgroundTaskTest backgroundTaskTest = new BackgroundTaskTest();
      //backgroundTaskTest.Test();

      //PeriodicJobTest periodicJobTest = new PeriodicJobTest();
      //periodicJobTest.Test();

      //ParallelTaskTest parallelTaskTest = new ParallelTaskTest();
      //parallelTaskTest.Test();

      new OverlappedJobTest().Test();

      //ApplyFilterTest applyFilterTest = new ApplyFilterTest();
      //applyFilterTest.Test();

      //LockvsSemaphoreTest lockvsSemaphoreTest = new LockvsSemaphoreTest();
      //lockvsSemaphoreTest.Test(false).Wait();
    }
  }
}
