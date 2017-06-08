namespace Tks.G1Track.Mobile.Shared.Common
{
  public class ProducerResult<TData>
  {
    #region =====[ ctor ]==========================================================================================

    internal ProducerResult(bool cont, TData data)
    {
      Continue = cont;
      Data = data;
    }

    #endregion

    #region =====[ IProdTaskResult ]===============================================================================

    public bool Continue { get; private set; }
    public TData Data { get; private set; }

    #endregion
  }
}
