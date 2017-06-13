using System;

namespace Tks.G1Track.Mobile.Shared.Common
{
  public interface IDirectedTaskContext
  {
    Exception LastException { get; set; }
    int LastExceptionCount { get; set; }
    int ExceptionCount { get; set; }
  }

  public class DirectedTaskContext : IDirectedTaskContext
  {
    #region =====[ Public Properties ]=============================================================================

    public Exception LastException { get; set; }
    public int LastExceptionCount { get; set; }
    public int ExceptionCount { get; set; }

    #endregion
  }
}
