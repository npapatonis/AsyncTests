﻿namespace Tks.G1Track.Mobile.Shared.Common
{
  public class ProducerResult<TData> : JobResult
  {
    #region =====[ ctor ]==========================================================================================

    internal ProducerResult(bool cont, TData data)
      : base(cont)
    {
      Data = data;
    }

    #endregion

    #region =====[ Public Properties ]=============================================================================

    public TData Data { get; private set; }

    #endregion
  }
}
