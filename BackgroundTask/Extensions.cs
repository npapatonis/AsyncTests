using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Tks.G1Track.Mobile.Shared.Common
{
  public static class Extensions
  {
    /// <summary>
    /// Expands the message.
    /// </summary>
    /// <param name="exception">The exception.</param>
    /// <returns></returns>
    public static string ExpandMessage(this Exception exception)
    {
      return ExpandMessage(exception, true, false);
    }

    /// <summary>
    /// Expands the message.
    /// </summary>
    /// <param name="exception">The exception.</param>
    /// <param name="includeInnerExceptions">if set to <c>true</c> [include inner exceptions].</param>
    /// <param name="includeStackTrace">if set to <c>true</c> [include stack trace].</param>
    /// <returns></returns>
    public static string ExpandMessage(
      this Exception exception,
      bool includeInnerExceptions,
      bool includeStackTrace)
    {
      StringBuilder sb = new StringBuilder();
      AppendToExceptionMessage(includeInnerExceptions, includeStackTrace, sb, exception, false);
      return sb.ToString();
    }

    private static void AppendToExceptionMessage(
      bool includeInnerExceptions,
      bool includeStackTrace,
      StringBuilder sb,
      Exception ex,
      bool isInnerException)
    {
      if (isInnerException)
      {
        sb.Append("<InnerException>");
      }

      if (includeStackTrace && !string.IsNullOrWhiteSpace(ex.StackTrace))
      {
        sb.AppendFormat(
          CultureInfo.InvariantCulture,
          "{0}: {1}\r\nStackTrace:\r\n{2}",
          ex.GetType().FullName,
          ex.Message,
          ex.StackTrace);
      }
      else
      {
        sb.AppendFormat(
          CultureInfo.InvariantCulture, "{0}: {1}",
          ex.GetType().FullName,
          ex.Message);
      }

      if (includeInnerExceptions)
      {
        var aggregateException = ex as AggregateException;
        if (aggregateException != null)
        {
          for (int i = 0; i < aggregateException.InnerExceptions.Count; i++)
          {
            sb.AppendLine();
            AppendToExceptionMessage(includeInnerExceptions, includeStackTrace, sb, aggregateException.InnerExceptions[i], true);
          }
        }
        else if (ex.InnerException != null)
        {
          sb.AppendLine();
          AppendToExceptionMessage(includeInnerExceptions, includeStackTrace, sb, ex.InnerException, true);
        }
      }

      if (isInnerException)
      {
        sb.Append("\r\n</InnerException>");
      }
    }

  }
}
