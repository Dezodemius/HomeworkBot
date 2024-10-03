using Telegram.Bot.Types.ReplyMarkups;
using TelegramBot.Data;
using TelegramBot.Models;

namespace TelegramBot.Roles
{
  /// <summary>
  /// Представляет пользователя с ролью учителя в системе.
  /// </summary>
  public class Teacher : User
  {
    private readonly DatabaseManager _dbManager;

    /// <summary>
    /// Инициализирует новый экземпляр класса Teacher.
    /// </summary>
    /// <param name="telegramChatId">Идентификатор чата Telegram.</param>
    /// <param name="firstName">Имя учителя.</param>
    /// <param name="lastName">Фамилия учителя.</param>
    /// <param name="email">Адрес электронной почты учителя.</param>
    /// <param name="dbManager">Менеджер базы данных.</param>
    public Teacher(long telegramChatId, string firstName, string lastName, string email, DatabaseManager dbManager)
        : base(telegramChatId, firstName, lastName, email, UserRole.Teacher)
    {
      _dbManager = dbManager;
    }

    public override InlineKeyboardMarkup Start()
    {
      return new InlineKeyboardMarkup(new[]
      {
         new[]
         {
             InlineKeyboardButton.WithCallbackData("Тест препода 1", $"test_teacher_1"),
             InlineKeyboardButton.WithCallbackData("Тест препода 2", $"test_teacher_2")
         }
      });
    }


    /// <summary>
    /// Метод для создания нового задания.
    /// </summary>
    public void CreateAssignment()
    {
      // Реализация создания задания
    }

    /// <summary>
    /// Обрабатывает входящее сообщение от учителя.
    /// </summary>
    /// <param name="message">Текст сообщения от учителя.</param>
    /// <returns>Ответ на сообщение учителя.</returns>
    public override async Task<string> ProcessMessageAsync(string message)
    {
      if (message.StartsWith("/createcourse"))
      {
        return await CreateCourseAsync(message);
      }
      else if (message.StartsWith("/assignhomework"))
      {
        return await AssignHomeworkAsync(message);
      }
      return "Неизвестная команда учителя.";
    }

    public override Task<string> ProcessCallbackAsync(string callbackData)
    {
      return base.ProcessCallbackAsync(callbackData);
    }

    /// <summary>
    /// Создает новый курс.
    /// </summary>
    /// <param name="message">Сообщение с информацией о курсе.</param>
    /// <returns>Результат создания курса.</returns>
    private async Task<string> CreateCourseAsync(string message)
    {
      // Реализация создания курса
      return "Курс успешно создан.";
    }

    /// <summary>
    /// Назначает домашнее задание.
    /// </summary>
    /// <param name="message">Сообщение с информацией о домашнем задании.</param>
    /// <returns>Результат назначения домашнего задания.</returns>
    private async Task<string> AssignHomeworkAsync(string message)
    {
      // Реализация назначения домашнего задания
      return "Домашнее задание успешно назначено.";
    }
  }
}