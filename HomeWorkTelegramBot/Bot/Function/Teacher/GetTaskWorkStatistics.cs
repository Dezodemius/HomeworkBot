using Telegram.Bot.Types;
using Telegram.Bot;

namespace HomeWorkTelegramBot.Bot.Function.Teacher
{
  /// <summary>
  /// Класс, управляющий получением статистики по выполнению задания.
  /// </summary>
  internal class GetTaskWorkStatistics
  {
    /// <summary>
    /// Обрабатывает callback-запрос, полученный от пользователя.
    /// </summary>
    /// <param name="botClient">Экземпляр клиента Telegram бота.</param>
    /// <param name="callbackQuery">Callback-запрос, полученный от пользователя.</param>
    /// <returns>Асинхронная задача, представляющая процесс обработки.</returns>
    public async Task HandleCallbackQueryAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery)
    {
      await TaskWorkData.ProcessGetTasks(botClient, callbackQuery);
    }

    /// <summary>
    /// Очищает временные файлы.
    /// </summary>
    /// <returns>Асинхронная задача, представляющая процесс обработки.</returns>
    public async Task ClearData()
    {
      await TaskWorkData.ClearData();
    }
  }
}
