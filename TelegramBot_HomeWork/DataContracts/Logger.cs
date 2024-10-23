using NLog;
using System.IO;

namespace DataContracts
{
  /// <summary>
  /// Логгер для записи сообщений с использованием NLog.
  /// </summary>
  static public class Logger
  {
    private static readonly NLog.Logger _logger;

    static Logger()
    {
      var logFilePath = "logs/logfile.log";
      var logDirectory = Path.GetDirectoryName(logFilePath);

      if (!Directory.Exists(logDirectory))
      {
        Directory.CreateDirectory(logDirectory);
      }

      _logger = LogManager.GetCurrentClassLogger();
    }

    /// <summary>
    /// Записывает информационное сообщение в лог.
    /// </summary>
    /// <param name="message">Сообщение для записи.</param>
    static public void LogInfo(string message)
    {
      Console.ForegroundColor = ConsoleColor.White;
      Console.WriteLine(message);
      _logger.Info(message);
    }

    /// <summary>
    /// Записывает сообщение об ошибке в лог.
    /// </summary>
    /// <param name="message">Сообщение об ошибке для записи.</param>
    static public void LogError(string message)
    {
      Console.ForegroundColor = ConsoleColor.Red;
      Console.WriteLine(message);
      Console.ForegroundColor = ConsoleColor.White;
      _logger.Error(message);
    }
  }
}
