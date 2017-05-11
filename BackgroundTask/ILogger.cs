namespace Tks.G1Track.Mobile.Shared.Common
{
  public interface ILogger
  {
    void Verbose(string message);
    void Information(string message);
    void Warning(string message);
    void Error(string message);
  }
}
