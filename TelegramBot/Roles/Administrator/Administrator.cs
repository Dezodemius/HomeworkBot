using ModelInterfaceHub.Interfaces;
using ModelInterfaceHub.Models;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramBot.Roles
{
  /// <summary>
  /// Представляет пользователя с ролью администратора в системе.
  /// </summary>
  public class Administrator : UserModel , IMessageHandler
  {

    /// <summary>
    /// Инициализирует новый экземпляр класса Administrator.
    /// </summary>
    /// <param name="telegramChatId">Идентификатор чата Telegram.</param>
    /// <param name="firstName">Имя администратора.</param>
    /// <param name="lastName">Фамилия администратора.</param>
    /// <param name="email">Адрес электронной почты администратора.</param>
    /// <param name="dbManager">Менеджер базы данных.</param>
    public Administrator(long telegramChatId, string firstName, string lastName, string email)
        : base(telegramChatId, firstName, lastName, email, UserRole.Administrator) { }


    /// <summary>
    /// Обрабатывает входящее сообщение от администратора.
    /// </summary>
    /// <param name="message">Текст сообщения от администратора.</param>
    /// <returns>Ответ на сообщение администратора.</returns>
    public async Task<string> ProcessMessageAsync(string message)
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Обработка Callback запросов от администратора.
    /// </summary>
    /// <param name="callbackData"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public async Task<string> ProcessCallbackAsync(string callbackData)
    {
      throw new NotImplementedException();
    }

  }
}