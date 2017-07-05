using AsyncTests.OverlappedTask;
using AsyncTests.PeriodicJob;

namespace AsyncTests
{
  class Program
  {
    static void Main(string[] args)
    {
      //new AsyncEventsTest().Test();
      //new CancellationHandlingTest().Test();
      new PeriodicJobTest().Test();
      //new ApplyFilterTest().Test();
      //new LockvsSemaphoreTest().Test(false).Wait();

      //new OverlappedJobTest().Test();
    }
  }
}
