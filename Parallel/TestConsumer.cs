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
    private int MaxNumber { get; set; }

    public TestConsumer()
      : this(int.MaxValue)
    {
    }

    public TestConsumer(int maxNumber)
    {
      MaxNumber = maxNumber;
    }

    public Task<bool> Run(List<int> numbers, ILogger logger, CancellationToken cancellationToken)
    {
      return Task.Run(async () =>
      {
        bool cont = true;

        foreach (var number in numbers)
        {
          if (cancellationToken.IsCancellationRequested) break;

          logger.Information($"Consuming number: {number}");
          await Task.Delay(100).ConfigureAwait(false);

          logger.Information($"Outputing number {number} somewhere");

          if (number == MaxNumber)
          {
            logger.Warning($"Max Number Reached {number}");
            cont = false;
            break;
          }
        }

        return cont;
      }, cancellationToken);
    }
  }
}
