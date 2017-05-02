using AsyncTests.AsyncEvents;

namespace AsyncTests
{
  class Program
  {
    static void Main(string[] args)
    {
      AsyncEventsTest asyncEventsTest = new AsyncEventsTest();
      asyncEventsTest.Test();
    }
  }
}
