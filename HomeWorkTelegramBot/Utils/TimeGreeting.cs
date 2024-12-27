using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeWorkTelegramBot.Utils
{
  /// <summary>
  /// Класс для генерации приветствий в зависимости от времени суток.
  /// </summary>
  public static class TimeGreeting
  {
    /// <summary>
    /// Возвращает приветствие в зависимости от текущего времени.
    /// </summary>
    /// <returns>Строка с приветствием.</returns>
    public static string GetGreeting()
    {
      var currentHour = DateTime.Now.Hour;

      if (currentHour >= 5 && currentHour < 12)
      {
        return "Доброе утро";
      }
      else if (currentHour >= 12 && currentHour < 18)
      {
        return "Добрый день";
      }
      else
      {
        return "Добрый вечер";
      }
    }
  }
}
