﻿using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Tks.G1Track.Mobile.Shared.Common;

namespace AsyncTests.PeriodicJob
{
  internal class HttpGetCompletedEventArgs : EventArgs
  {
    internal HttpGetCompletedEventArgs(HttpResponseMessage httpResponseMessage, CancellationToken cancellationToken)
    {
      HttpResponseMessage = httpResponseMessage;
      CancellationToken = cancellationToken;
    }

    internal HttpResponseMessage HttpResponseMessage { get; private set; }
    internal CancellationToken CancellationToken { get; private set; }
  }

  internal class PeriodicJob : IPeriodicJob
  {
    public event EventHandler<HttpGetCompletedEventArgs> HttpGetCompleted;

    private TimeSpan m_sleepInterval = TimeSpan.FromMilliseconds(5000);
    public TimeSpan SleepInterval => m_sleepInterval;

    public bool HandleException(IJobExceptionState jobExceptionState, ILogger logger)
    {
      if (jobExceptionState.LastExceptionCount == 3)
      {
        logger.Warning("Handling exception that has occurred too many times");
        return false;
      }

      m_sleepInterval = m_sleepInterval.Subtract(TimeSpan.FromMilliseconds(2000));
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

      //httpClient.Timeout = TimeSpan.FromMilliseconds(10);
      var response = await httpClient.GetAsync("http://www.google.com", cancellationToken);
      HttpGetCompleted?.Invoke(this, new HttpGetCompletedEventArgs(response, cancellationToken));

      return JobResult.TrueResult;
    }
  }
}
