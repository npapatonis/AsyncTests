using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Tks.G1Track.Mobile.Shared.Common;

namespace AsyncTests.TriggeredJob
{
  internal class TriggeredJob : ITriggeredJob
  {
    AsyncManualResetEvent m_mre = new AsyncManualResetEvent(false);

    internal TriggeredJob(TriggeredJobTest triggeredJobTest)
    {
      triggeredJobTest.JobTriggered += HandleJobTriggered;
    }

    private void HandleJobTriggered(object sender, EventArgs e)
    {
      m_mre.Set();
    }

    public Task[] GetTriggers(CancellationToken cancellationToken)
    {
      return new Task[] { m_mre.WaitAsync(cancellationToken), Task.Delay(3000, cancellationToken) };
      //return new Task[] { m_mre.WaitAsync(cancellationToken) };
    }

    public bool HandleException(IJobExceptionState jobExceptionState, ILogger logger)
    {
      m_mre.Reset();
      return true;
    }

    public async Task<JobResult> Run(IJobExceptionState jobExceptionState, ILogger logger, CancellationToken cancellationToken)
    {
      logger.Verbose($"jobExceptionState.LastExceptionCount: {jobExceptionState.LastExceptionCount}");
      //if (taskContext.LastExceptionCount > 3) return false;

      logger.Information("Attempting HTTP GET that will timeout");
      var httpClient = new HttpClient();

      // Use this to create aggregate exception with TaskCancelled inner
      //httpClient.Timeout = TimeSpan.FromMilliseconds(10);
      //var task = httpClient.GetAsync("http://www.google.com", cancellationToken);
      //await Task.WhenAny(task, Task.CompletedTask);
      //var response = task.Result;

      httpClient.Timeout = TimeSpan.FromMilliseconds(10);
      var response = await httpClient.GetAsync("http://www.google.com", cancellationToken);
      //HttpGetCompleted?.Invoke(this, new HttpGetCompletedEventArgs(response, cancellationToken));

      logger.Information("Resetting MRE");
      m_mre.Reset();

      return JobResult.TrueResult;
    }
  }
}
