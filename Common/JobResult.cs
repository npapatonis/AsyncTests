namespace Tks.G1Track.Mobile.Shared.Common
{
  public class JobResult
  {
    #region =====[ ctor ]==========================================================================================

    internal JobResult(bool cont)
    {
      Continue = cont;
    }

    #endregion

    #region =====[ Public Properties ]=============================================================================

    public bool Continue { get; private set; }

    public static JobResult TrueResult = new JobResult(true);
    public static JobResult FalseResult = new JobResult(false);

    #endregion
  }
}
