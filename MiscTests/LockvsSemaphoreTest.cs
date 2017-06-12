using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncTests.MiscTests
{
  internal class LockvsSemaphoreTest
  {
    internal async Task Test(bool lockTest)
    {
      const int maxIterations = 100000000;

      Stopwatch sw = new Stopwatch();

      object lockMutex = new object();
      SemaphoreSlim semaphoreMutex = new SemaphoreSlim(1, 1);

      if (lockTest)
      {
        sw.Restart();
        for (int n = 0; n < maxIterations; n++)
        {
          lock (lockMutex)
          {
            var x = 1;
          }
        }
        sw.Stop();
      }
      else
      {
        sw.Restart();
        for (int n = 0; n < maxIterations; n++)
        {
          await semaphoreMutex.WaitAsync().ConfigureAwait(false);
          var x = 1;
          semaphoreMutex.Release();
        }
        sw.Stop();
      }

      var testType = lockTest ? "lock" : "semaphore";
      Console.WriteLine($"Test type: {testType}, Elapsed time: {sw.Elapsed}");
      Console.ReadLine();
    }
  }
}
