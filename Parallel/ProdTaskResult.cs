using System.Collections.Generic;

namespace Tks.G1Track.Mobile.Shared.Common
{
  internal interface IProdTaskResult : ITaskResult
  {
    List<int> Numbers { get; }
  }

  internal class ProdTaskResult : TaskResult, IProdTaskResult
  {
    #region =====[ ctor ]==========================================================================================

    internal ProdTaskResult(bool cont, List<int> numbers)
      : base(cont)
    {
      Numbers = numbers;
    }

    #endregion

    #region =====[ IProdTaskResult ]===============================================================================

    public List<int> Numbers { get; private set; }

    #endregion

  }
}
