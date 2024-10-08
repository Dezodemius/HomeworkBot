using Telegram.Bot;

namespace ModelInterfaceHub.Interfaces
{
  public interface IMessageHandler
  {
    /// <summary>
    /// Обрабатывает входящее сообщение от пользователя.
    /// </summary>
    /// <param name="message">Текст сообщения от пользователя.</param>
    /// <returns>Ответ на сообщение пользователя.</returns>
    Task ProcessMessageAsync(ITelegramBotClient botClient, long chatId, string message);

    /// <summary>
    /// Обрабатывает callback запрос от пользователя.
    /// </summary>
    /// <param name="message">Текст сообщения от пользователя.</param>
    /// <returns>Ответ на сообщение пользователя.</returns>
    Task ProcessCallbackAsync(ITelegramBotClient botClient, long chatId, string callbackData);
  }
}