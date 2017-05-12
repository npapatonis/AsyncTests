using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Tks.G1Track.Mobile.Shared.Common;

namespace AsyncTests.BackgroundTask
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

  internal class BackgroundTask : IBackgroundTask
  {
    public event EventHandler<HttpGetCompletedEventArgs> HttpGetCompleted;

    public TimeSpan SleepInterval => TimeSpan.FromMilliseconds(3000);

    public bool HandleException(IBackgroundTaskContext taskContext, ILogger logger)
    {
      //if (taskContext.LastException is TaskCanceledException && taskContext.LastExceptionCount == 3)
      if (taskContext.LastExceptionCount == 3)
      {
        logger.Warning("Handling exception that has occurred too many times");
        return false;
      }

      return true;
    }

    public async Task<bool> Run(IBackgroundTaskContext taskContext, ILogger logger, CancellationToken cancellationToken)
    {
      logger.Verbose($"taskContext.LastExceptionCount: {taskContext.LastExceptionCount}");
      //if (taskContext.LastExceptionCount > 3) return false;

      logger.Information("Attempting HTTP GET that will timeout");
      var httpClient = new HttpClient();
      //httpClient.Timeout = TimeSpan.FromMilliseconds(10);
      var response = await httpClient.GetAsync("http://www.google.com", cancellationToken);
      HttpGetCompleted?.Invoke(this, new HttpGetCompletedEventArgs(response, cancellationToken));

      return true;
    }
  }
}
