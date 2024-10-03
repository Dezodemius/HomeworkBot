using System.Runtime.InteropServices;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBot.Data;
using TelegramBot.Models;

namespace TelegramBot.Roles
{
  /// <summary>
  /// Представляет пользователя с ролью администратора в системе.
  /// </summary>
  public class Administrator : Models.User
  {
    private readonly DatabaseManager _dbManager;

    /// <summary>
    /// Инициализирует новый экземпляр класса Administrator.
    /// </summary>
    /// <param name="telegramChatId">Идентификатор чата Telegram.</param>
    /// <param name="firstName">Имя администратора.</param>
    /// <param name="lastName">Фамилия администратора.</param>
    /// <param name="email">Адрес электронной почты администратора.</param>
    /// <param name="dbManager">Менеджер базы данных.</param>
    public Administrator(long telegramChatId, string firstName, string lastName, string email, DatabaseManager dbManager)
        : base(telegramChatId, firstName, lastName, email, UserRole.Administrator)
    {
      _dbManager = dbManager;
    }


    /// <summary>
    /// Обрабатывает входящее сообщение от администратора.
    /// </summary>
    /// <param name="message">Текст сообщения от администратора.</param>
    /// <returns>Ответ на сообщение администратора.</returns>
    public override async Task<string> ProcessMessageAsync(string message)
    {
      if (string.IsNullOrWhiteSpace(message))
      {
        return "Пожалуйста, введите команду.";
      }

      switch (message.ToLower())
      {
        case "/start":
          return "Добро пожаловать, администратор! Используйте /change_role для смены роли или /users для просмотра списка пользователей.";
        case "/users":
          return await ListUsersAsync();
        case "/change_role":
          return "Выберите новую роль:";
        default:
          return "Неизвестная команда. Доступные команды: /start, /users, /change_role";
      }
    }

    public override async Task<string> ProcessCallbackAsync(string callbackData)
    {
      var message = callbackData.Split('_');
      if ((message[0].ToLower().Contains("approve") || message[0].ToLower().Contains("reject")) && long.TryParse(message[1], out long userChatId))
      {
        return await HandleAdminDecisionAsync(message[0], userChatId);
      }

      return "Callback запрос не разпознан!";
    }

    public async Task<string> ListUsersAsync()
    {
      // TODO : Реализовать логику получения списка пользователей
      return "Список пользователей: ...";
    }

    public override InlineKeyboardMarkup Start()
    {
      return new InlineKeyboardMarkup(new[]
      {
         new[]
         {
             InlineKeyboardButton.WithCallbackData("Тест админа 1", $"test_admin_1"),
             InlineKeyboardButton.WithCallbackData("Тест админа 2", $"test_admin_2")
         }
      });
    }

    public async Task<string> HandleAdminDecisionAsync(string decision, long userChatId)
    {
      var registrationRequest = await _dbManager.GetRegistrationRequestAsync(userChatId);
      if (registrationRequest == null)
      {
        return "Заявка на регистрацию не найдена или уже обработана.";
      }

      switch (decision.ToLower())
      {
        case "approve":
          await _dbManager.ApproveRegistrationAsync(userChatId);
          return $"Регистрация пользователя {registrationRequest.FirstName} {registrationRequest.LastName} подтверждена.";
        case "reject":
          await _dbManager.RejectRegistrationAsync(userChatId);
          return $"Заявка на регистрацию пользователя {registrationRequest.FirstName} {registrationRequest.LastName} отклонена.";
        default:
          return "Неверное решение. Используйте 'approve' или 'reject'.";
      }
    }
  }
}