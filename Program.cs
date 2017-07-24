using AsyncTests.ApplyFilter;
using AsyncTests.AsyncLocal;
using AsyncTests.OverlappedTask;
using AsyncTests.PeriodicJob;
using AsyncTests.TriggeredJob;

namespace AsyncTests
{
  class Program
  {
    static void Main(string[] args)
    {
      //new AsyncEventsTest().Test();
      //new CancellationHandlingTest().Test();
      //new PeriodicJobTest().Test();
      new ApplyFilterTest().Test();
      //new LockvsSemaphoreTest().Test(false).Wait();
      //new OverlappedJobTest().Test();
      //new TriggeredJobTest().Test();
      //new AsyncLocalTest().Test();
    }
  }
}
