using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncTests.CancellationHandling
{
  internal class CancellationHandling
  {
    internal async Task Run()
    {
      var cts = new CancellationTokenSource();

      try
      {
        // request cancellation
        //cts.Cancel();

        // get a "hot" task
        var httpClient = new HttpClient();
        httpClient.Timeout = TimeSpan.FromMilliseconds(10);
        var task = httpClient.GetAsync("http://www.google.com", cts.Token);

        await task;

        // pass:
        Debug.Assert(true, "expected TaskCanceledException to be thrown");
      }
      catch (OperationCanceledException ex)
      {
        // pass:
        Debug.Assert(cts.Token.IsCancellationRequested, "expected cancellation requested on original token");

        // fail:
        Debug.Assert(ex.CancellationToken.IsCancellationRequested, "expected cancellation requested on token attached to exception");
      }
    }
  }
}
