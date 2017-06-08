using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Tks.G1Track.Mobile.Shared.Common
{
  public class TestProducer : IParallelProducer<List<int>>
  {
    private int Iterations = 0;

    public Task<ProducerResult<List<int>>> Run(ILogger logger, CancellationToken cancellationToken)
    {
      const int maxIterations = 10;

      return Task.Run(async () =>
      {
        bool cont = false;
        List<int> numbers = null;

        if (Iterations < maxIterations)
        {
          cont = (Iterations < maxIterations - 1);
          numbers = new List<int>(10);

          for (int n = 0; n < 10; n++)
          {
            if (cancellationToken.IsCancellationRequested) break;

            int number = Iterations * 10 + n;
            logger.Verbose($"Producing number: {number}");
            await Task.Delay(100).ConfigureAwait(false);

            numbers.Add(number);
          }

          Iterations++;
        }

        return new ProducerResult<List<int>>(cont, numbers);
      }, cancellationToken);
    }
  }
}
