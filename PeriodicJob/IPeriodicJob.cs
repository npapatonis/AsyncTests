using System;

namespace Tks.G1Track.Mobile.Shared.Common
{
  public interface IPeriodicJob : IDirectableJob
  {
    TimeSpan SleepInterval { get; }
  }
}
