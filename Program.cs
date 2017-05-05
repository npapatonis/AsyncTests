using AsyncTests.AsyncEvents;
using AsyncTests.BackgroundTask;
using AsyncTests.CancellationHandling;

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

      BackgroundTaskTest backgroundTaskTest = new BackgroundTaskTest();
      backgroundTaskTest.Test();
    }
  }
}
