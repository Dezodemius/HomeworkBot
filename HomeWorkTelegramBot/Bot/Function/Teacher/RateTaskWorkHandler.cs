using Telegram.Bot.Types;
using Telegram.Bot;

namespace HomeWorkTelegramBot.Bot.Function.Teacher
{
  /// <summary>
  /// Класс, управляющий процессом выставления оценки за задание.
  /// </summary>
  internal class RateTaskWorkHandler
  {
    /// <summary>
    /// Обрабатывает callback-запрос, полученный от пользователя.
    /// </summary>
    /// <param name="botClient">Экземпляр клиента Telegram бота.</param>
    /// <param name="callbackQuery">Callback-запрос, полученный от пользователя.</param>
    /// <param name="taskId">Уникальный идентификатор задания.</param>
    /// <returns>Асинхронная задача, представляющая процесс обработки.</returns>
    public async Task HandleCallbackQueryAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery, int taskId = -1)
    {
      await RateTaskWork.ProcessUpdateAnswer(botClient, callbackQuery, taskId);
    }

    /// <summary>
    /// Очищает временные данные.
    /// </summary>
    /// <returns>Асинхронная задача, представляющая процесс обработки.</returns>
    public async Task ClearData()
    {
      await RateTaskWork.ClearData();
    }
  }
}
