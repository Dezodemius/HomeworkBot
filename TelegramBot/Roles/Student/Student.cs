using ModelInterfaceHub.Interfaces;
using ModelInterfaceHub.Models;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramBot.Roles
{
  /// <summary>
  /// Представляет пользователя с ролью студента в системе.
  /// </summary>
  public class Student : UserModel, IMessageHandler
  {
    /// <summary>
    /// Инициализирует новый экземпляр класса Student.
    /// </summary>
    /// <param name="telegramChatId">Идентификатор чата Telegram.</param>
    /// <param name="firstName">Имя студента.</param>
    /// <param name="lastName">Фамилия студента.</param>
    /// <param name="email">Адрес электронной почты студента.</param>
    /// <param name="dbManager">Менеджер базы данных.</param>
    public Student(long telegramChatId, string firstName, string lastName, string email)
        : base(telegramChatId, firstName, lastName, email, UserRole.Student) { }



    /// <summary>
    /// Обрабатывает входящее сообщение от студента.
    /// </summary>
    /// <param name="message">Текст сообщения от студента.</param>
    /// <returns>Ответ на сообщение студента.</returns>
    public async Task<string> ProcessMessageAsync(string message)
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Обработка Callback запросов от студента.
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