using System;

namespace AsyncTests.CancellationHandling
{
  internal class CancellationHandlingTest
  {
    internal void Test()
    {
      CancellationHandling cancellationHandling = new CancellationHandling();
      cancellationHandling.Run().Wait();

      Console.WriteLine("CancellationHandlingTest.Test, Press a key to exit...");
      Console.ReadLine();
    }
  }
}
