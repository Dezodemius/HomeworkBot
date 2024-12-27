using NLog;
using System.Runtime.CompilerServices;

namespace HomeWorkTelegramBot.Config
{
  /// <summary>
  /// Логирование данных.
  /// </summary>
  internal static class Logger
  {
    /// <summary>
    /// Логирование информационного сообщения.
    /// </summary>
    /// <param name="message">Сообщение для записи в лог.</param>
    public static void LogInformation(string message, [CallerFilePath] string callerFilePath = "")
    {
      var logger = LogManager.GetLogger(GetCallerClassName(callerFilePath));
      logger.Info(message);
    }

    /// <summary>
    /// Логирует сообщение об исключении.
    /// </summary>
    /// <param name="ex">Исключение для логирования.</param>
    public static void LogException(Exception ex, [CallerFilePath] string callerFilePath = "")
    {
      var logger = LogManager.GetLogger(GetCallerClassName(callerFilePath));
      logger.Error(ex);
    }

    /// <summary>
    /// Логирование предупреждающего сообщения.
    /// </summary>
    /// <param name="message">Сообщение для записи в лог.</param>
    /// <param name="callerClassName">Имя вызывающего класса (автоматически заполняется компилятором).</param>
    public static void LogWarning(string message, [CallerFilePath] string callerFilePath = "")
    {
      var logger = LogManager.GetLogger(GetCallerClassName(callerFilePath));
      logger.Warn(message);
    }

    /// <summary>
    /// Логирование сообщения об ошибке.
    /// </summary>
    /// <param name="message">Сообщение для записи в лог.</param>
    /// <param name="callerClassName">Имя вызывающего класса (автоматически заполняется компилятором).</param>
    public static void LogError(string message, [CallerFilePath] string callerFilePath = "")
    {
      var logger = LogManager.GetLogger(GetCallerClassName(callerFilePath));
      logger.Error(message);
    }

    /// <summary>
    /// Получает имя вызывающего класса из пути к файлу.
    /// </summary>
    /// <param name="filePath">Путь к файлу вызывающего класса.</param>
    /// <returns>Имя вызывающего класса.</returns>
    private static string GetCallerClassName(string filePath)
    {
      return Path.GetFileNameWithoutExtension(filePath);
    }
  }
}
