using System;

namespace Tks.G1Track.Mobile.Shared.Common
{
  public interface IBackgroundTaskContext
  {
    Exception LastException { get; set; }
    int LastExceptionCount { get; set; }
    int ExceptionCount { get; set; }
    object State { get; set; }
  }

  public class BackgroundTaskContext : IBackgroundTaskContext
  {
    #region =====[ Public Properties ]=============================================================================

    public Exception LastException { get; set; }
    public int LastExceptionCount { get; set; }
    public int ExceptionCount { get; set; }
    public object State { get; set; }

    #endregion
  }
}
