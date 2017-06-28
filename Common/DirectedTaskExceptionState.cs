﻿using System;

namespace Tks.G1Track.Mobile.Shared.Common
{
  public interface IDirectedTaskExceptionState
  {
    Exception LastException { get; set; }
    int LastExceptionCount { get; set; }
    int ExceptionCount { get; set; }
    void Clear();
  }

  public class DirectedTaskExceptionState : IDirectedTaskExceptionState
  {
    #region =====[ Public Properties ]=============================================================================

    public Exception LastException { get; set; }
    public int LastExceptionCount { get; set; }
    public int ExceptionCount { get; set; }

    public void Clear()
    {
      LastException = null;
      LastExceptionCount = 0;
      ExceptionCount = 0;
    }

    #endregion
  }
}