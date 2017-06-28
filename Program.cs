using AsyncTests.ApplyFilter;
using AsyncTests.MiscTests;
using AsyncTests.PeriodicTask;
using AsyncTests.SyncronizedTask;

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

      //PeriodicTaskTest periodicTaskTest = new PeriodicTaskTest();
      //periodicTaskTest.Test();

      //ParallelTaskTest parallelTaskTest = new ParallelTaskTest();
      //parallelTaskTest.Test();

      new SyncronizedTaskTest().Test();

      //ApplyFilterTest applyFilterTest = new ApplyFilterTest();
      //applyFilterTest.Test();

      //LockvsSemaphoreTest lockvsSemaphoreTest = new LockvsSemaphoreTest();
      //lockvsSemaphoreTest.Test(false).Wait();
    }
  }
}
