namespace Tks.G1Track.Mobile.Shared.Common
{
  internal interface ITaskResult
  {
    bool Continue { get; }
  }

  internal class TaskResult : ITaskResult
  {
    #region =====[ ctor ]==========================================================================================

    internal TaskResult(bool cont)
    {
      Continue = cont;
    }

    #endregion

    #region =====[ ITaskResult ]===================================================================================

    public bool Continue { get; private set; }

    #endregion
  }
}
