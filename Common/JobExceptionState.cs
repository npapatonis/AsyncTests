using System;

namespace Tks.G1Track.Mobile.Shared.Common
{
  public interface IJobExceptionState
  {
    Exception LastException { get; set; }
    string LastExceptionMessage { get; set; }
    int LastExceptionCount { get; set; }
    int ExceptionCount { get; set; }
    void Clear();
  }

  public class JobExceptionState : IJobExceptionState
  {
    #region =====[ Public Properties ]=============================================================================

    public Exception LastException { get; set; }
    public string LastExceptionMessage { get; set; }
    public int LastExceptionCount { get; set; }
    public int ExceptionCount { get; set; }

    public static IJobExceptionState None { get; } = new JobExceptionState();

    public void Clear()
    {
      LastException = null;
      LastExceptionCount = 0;
      ExceptionCount = 0;
    }

    #endregion
  }
}
