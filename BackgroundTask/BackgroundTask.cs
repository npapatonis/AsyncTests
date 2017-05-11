using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Tks.G1Track.Mobile.Shared.Common;

namespace AsyncTests.BackgroundTask
{
  internal class BackgroundTask : IBackgroundTask
  {
    public TimeSpan SleepInterval => TimeSpan.FromMilliseconds(3000);

    public bool HandleException(IBackgroundTaskContext taskContext, ILogger logger)
    {
      if (taskContext.LastException is TaskCanceledException && taskContext.LastExceptionCount == 3)
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

      return true;
    }
  }
}
