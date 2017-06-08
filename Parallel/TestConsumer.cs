using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Tks.G1Track.Mobile.Shared.Common
{
  public class TestConsumer : IParallelConsumer<List<int>>
  {
    public Task<bool> Run(List<int> numbers, ILogger logger, CancellationToken cancellationToken)
    {
      return Task.Run(async () =>
      {
        foreach (var number in numbers)
        {
          if (cancellationToken.IsCancellationRequested) break;

          logger.Verbose($"Consuming number: {number}");
          await Task.Delay(400).ConfigureAwait(false);

          logger.Information($"Outputing number {number} somewhere");
        }

        return true;
      }, cancellationToken);
    }
  }
}
