using System;

namespace Tks.G1Track.Mobile.Shared.Common
{
  internal class Logger : ILogger
  {
    private object m_lock = new object();

    private void LogMessage(string prefix, ConsoleColor prefixColor, string message)
    {
      message = $"{DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss.fff")} [{System.Threading.Thread.CurrentThread.ManagedThreadId}] {message}";
      lock (m_lock)
      {
        var currentColor = Console.ForegroundColor;
        Console.ForegroundColor = prefixColor;
        Console.Write(prefix);
        Console.ForegroundColor = currentColor;
        Console.WriteLine($" {message}");
      }
    }

    public void Error(string message)
    {
      LogMessage("ERROR", ConsoleColor.Red, message);
    }

    public void Information(string message)
    {
      LogMessage("INFO", ConsoleColor.Cyan, message);
    }

    public void Verbose(string message)
    {
      LogMessage("DEBUG", ConsoleColor.White, message);
    }

    public void Warning(string message)
    {
      LogMessage("WARNING", ConsoleColor.Yellow, message);
    }
  }
}
