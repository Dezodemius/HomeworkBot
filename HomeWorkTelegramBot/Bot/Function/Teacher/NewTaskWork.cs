using Telegram.Bot.Types;
using Telegram.Bot;

namespace HomeWorkTelegramBot.Bot.Function.Teacher
{
  /// <summary>
  /// Класс, управляющий процессом создания нового задания.
  /// </summary>
  public class NewTaskWork 
  {
    /// <summary>
    /// Обрабатывает callback-запрос, полученный от пользователя.
    /// </summary>
    /// <param name="botClient">Экземпляр клиента Telegram бота.</param>
    /// <param name="callbackQuery">Callback-запрос, полученный от пользователя.</param>
    /// <returns>Асинхронная задача, представляющая процесс обработки.</returns>
    public async Task HandleCallback(ITelegramBotClient botClient, CallbackQuery callbackQuery)
    {
      await CreateTaskWork.ProcessCreationStep(botClient, callbackQuery);
    }

    /// <summary>
    /// Обрабатывает сообщение, полученное от пользователя.
    /// </summary>
    /// <param name="botClient">Экземпляр клиента Telegram бота.</param>
    /// <param name="message">Сообщение, полученное от пользователя.</param>
    /// <returns>Асинхронная задача, представляющая процесс обработки.</returns>
    public async Task HandleMessageAsync(ITelegramBotClient botClient, Message message)
    {
      await CreateTaskWork.ProcessCreationStep(botClient, message);
    }

    /// <summary>
    /// Очищает временные данные.
    /// </summary>
    /// <returns>Асинхронная задача, представляющая процесс обработки.</returns>
    public async Task ClearData()
    {
      await CreateTaskWork.ClearData();
    }
  }
}
