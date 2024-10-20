using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramBot.Models
{
  internal static class UserStateTracker
  {
    private static Dictionary<long, string> userStates = new Dictionary<long, string>();
    private static Dictionary<long, Dictionary<string, string>> temporaryData = new Dictionary<long, Dictionary<string, string>>();

    /// <summary>
    /// Устанавливает состояние пользователя по его идентификатору чата.
    /// </summary>
    /// <param name="chatId">Идентификатор чата пользователя.</param>
    /// <param name="state">Состояние, которое нужно установить.</param>
    public static void SetUserState(long chatId, string state)
    {
      userStates[chatId] = state;
    }

    /// <summary>
    /// Получает состояние пользователя по его идентификатору чата.
    /// </summary>
    /// <param name="chatId">Идентификатор чата пользователя.</param>
    /// <returns>Состояние пользователя или null, если оно не найдено.</returns>
    public static string GetUserState(long chatId)
    {
      if (userStates.ContainsKey(chatId))
      {
        Console.WriteLine($"Key found: {chatId}, Value: {userStates[chatId]}");
        return userStates[chatId];
      }
      else
      {
        Console.WriteLine($"Key not found: {chatId}");
        return null;
      }
    }

    /// <summary>
    /// Устанавливает временные данные для пользователя по его идентификатору чата.
    /// </summary>
    /// <param name="chatId">Идентификатор чата пользователя.</param>
    /// <param name="key">Ключ для временных данных.</param>
    /// <param name="value">Значение временных данных.</param>
    public static void SetTemporaryData(long chatId, string key, string value)
    {
      if (!temporaryData.ContainsKey(chatId))
      {
        temporaryData[chatId] = new Dictionary<string, string>();
      }
      temporaryData[chatId][key] = value;
    }

    /// <summary>
    /// Получает временные данные пользователя по его идентификатору чата и ключу.
    /// </summary>
    /// <param name="chatId">Идентификатор чата пользователя.</param>
    /// <param name="key">Ключ для временных данных.</param>
    /// <returns>Значение временных данных или null, если оно не найдено.</returns>
    public static string GetTemporaryData(long chatId, string key)
    {
      return temporaryData.ContainsKey(chatId) && temporaryData[chatId].ContainsKey(key) ? temporaryData[chatId][key] : null;
    }

    /// <summary>
    /// Очищает временные данные пользователя по его идентификатору чата.
    /// </summary>
    /// <param name="chatId">Идентификатор чата пользователя.</param>
    public static void ClearTemporaryData(long chatId)
    {
      if (temporaryData.ContainsKey(chatId))
      {
        temporaryData[chatId].Clear();
      }
    }
  }
}
