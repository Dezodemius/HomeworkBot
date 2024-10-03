using Telegram.Bot.Types.ReplyMarkups;
using TelegramBot.Data;
using TelegramBot.Models;

namespace TelegramBot.Roles
{
  /// <summary>
  /// Представляет пользователя с ролью студента в системе.
  /// </summary>
  public class Student : User
  {
    private readonly DatabaseManager _dbManager;

    /// <summary>
    /// Инициализирует новый экземпляр класса Student.
    /// </summary>
    /// <param name="telegramChatId">Идентификатор чата Telegram.</param>
    /// <param name="firstName">Имя студента.</param>
    /// <param name="lastName">Фамилия студента.</param>
    /// <param name="email">Адрес электронной почты студента.</param>
    /// <param name="dbManager">Менеджер базы данных.</param>
    public Student(long telegramChatId, string firstName, string lastName, string email, DatabaseManager dbManager)
        : base(telegramChatId, firstName, lastName, email, UserRole.Student)
    {
      _dbManager = dbManager;
    }

    public override InlineKeyboardMarkup Start()
    {
      return new InlineKeyboardMarkup(new[]
      {
         new[]
         {
             InlineKeyboardButton.WithCallbackData("Тест студента 1", $"test_student_1"),
             InlineKeyboardButton.WithCallbackData("Тест студента 2", $"test_student_2")
         }
      });
    }


    /// <summary>
    /// Обрабатывает входящее сообщение от студента.
    /// </summary>
    /// <param name="message">Текст сообщения от студента.</param>
    /// <returns>Ответ на сообщение студента.</returns>
    public override async Task<string> ProcessMessageAsync(string message)
    {
      if (message.StartsWith("/submit"))
      {
        return await SubmitHomeworkAsync(message);
      }
      else if (message.StartsWith("/status"))
      {
        return await CheckHomeworkStatusAsync(message);
      }
      else if (message.StartsWith("/mycourses"))
      {
        return await ListCoursesAsync();
      }
      return "Неизвестная команда. Доступные команды: /submit, /status, /mycourses";
    }

    public override Task<string> ProcessCallbackAsync(string callbackData)
    {
      return base.ProcessCallbackAsync(callbackData);
    }

    /// <summary>
    /// Отправляет домашнее задание.
    /// </summary>
    /// <param name="message">Сообщение с информацией о домашнем задании.</param>
    /// <returns>Результат отправки домашнего задания.</returns>
    private async Task<string> SubmitHomeworkAsync(string message)
    {
      // Реализация отправки домашнего задания
      return "Домашнее задание отправлено.";
    }

    /// <summary>
    /// Проверяет статус домашнего задания.
    /// </summary>
    /// <param name="message">Сообщение с запросом статуса.</param>
    /// <returns>Статус домашнего задания.</returns>
    private async Task<string> CheckHomeworkStatusAsync(string message)
    {
      // Реализация проверки статуса домашнего задания
      return "Статус домашнего задания: проверяется.";
    }

    /// <summary>
    /// Получает список курсов студента.
    /// </summary>
    /// <returns>Список курсов студента.</returns>
    private async Task<string> ListCoursesAsync()
    {
      // Реализация получения списка курсов студента
      return "Список ваших курсов: ...";
    }
  }
}