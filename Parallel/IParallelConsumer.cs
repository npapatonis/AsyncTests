﻿using System.Threading;
using System.Threading.Tasks;

namespace Tks.G1Track.Mobile.Shared.Common
{
  public interface IParallelConsumer<TData>
  {
    Task<bool> Run(TData data, ILogger logger, CancellationToken cancellationToken);
  }
}
