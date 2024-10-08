using ModelInterfaceHub.Interfaces;
using ModelInterfaceHub.Models;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramBot.Roles
{
  /// <summary>
  /// Представляет пользователя с ролью учителя в системе.
  /// </summary>
  public class Teacher : UserModel, IMessageHandler
  {

    /// <summary>
    /// Инициализирует новый экземпляр класса Teacher.
    /// </summary>
    /// <param name="telegramChatId">Идентификатор чата Telegram.</param>
    /// <param name="firstName">Имя учителя.</param>
    /// <param name="lastName">Фамилия учителя.</param>
    /// <param name="email">Адрес электронной почты учителя.</param>
    public Teacher(long telegramChatId, string firstName, string lastName, string email)
        : base(telegramChatId, firstName, lastName, email, UserRole.Teacher) { }

    /// <summary>
    /// Обрабатывает входящее сообщение от преподавателя.
    /// </summary>
    /// <param name="message">Текст сообщения от пропеодавателя.</param>
    /// <returns>Ответ на сообщение учителя.</returns>
    public async Task ProcessMessageAsync(ITelegramBotClient botClient, long chatId, string message)
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Обработка Callback запросов от преподавателя.
    /// </summary>
    /// <param name="callbackData">Данные обратного вызова.</param>
    /// <returns>Результат обработки запроса.</returns>
    /// <exception cref="NotImplementedException"></exception>
    public async Task ProcessCallbackAsync(ITelegramBotClient botClient, long chatId, string callbackData, int messageId)
    {
      throw new NotImplementedException();  
    }

    /// <summary>
    /// Просмотр непроверенных домашних заданий.
    /// </summary>
    /// <returns>Задача, представляющая асинхронную операцию.</returns>
    public async Task ViewUncheckedHomeworks()
    { 
      throw new NotImplementedException();  
    }

    /// <summary>
    /// Отображение непроверенных домашних заданий.
    /// </summary>
    /// <returns>Задача, представляющая асинхронную операцию.</returns>
    public async Task ShowUncheckedHomeworks()
    { 
      throw new NotImplementedException();  
    }

    /// <summary>
    /// Отображение кнопок для работы с домашними заданиями.
    /// </summary>
    /// <returns>Задача, представляющая асинхронную операцию.</returns>
    public async Task DisplayHomeworkButtons()
    { 
      throw new NotImplementedException();  
    }

    /// <summary>
    /// Отображение невыполненных домашних заданий.
    /// </summary>
    /// <returns>Задача, представляющая асинхронную операцию.</returns>
    public async Task ShowUncompletedHomeworks()
    { 
      throw new NotImplementedException();  
    }

    /// <summary>
    /// Отображение выполненных домашних заданий.
    /// </summary>
    /// <returns>Задача, представляющая асинхронную операцию.</returns>
    public async Task ShowCompletedHomeworks()
    {
      throw new NotImplementedException();
    }

  }
}