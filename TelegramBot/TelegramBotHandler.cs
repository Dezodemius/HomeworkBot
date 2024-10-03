using System.Data.SQLite;
using TelegramBot.Roles;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBot.Data;
using TelegramBot.Models;
using System.Collections.Concurrent;

namespace TelegramBot
{
  /// <summary>
  /// Класс для обработки сообщений Telegram бота.
  /// </summary>
  public class TelegramBotHandler
  {
    /// <summary>
    /// Клиент Telegram бота.
    /// </summary>
    private readonly ITelegramBotClient _botClient;

    /// <summary>
    /// Менеджер базы данных.
    /// </summary>
    private readonly DatabaseManager _dbManager;

    private Dictionary<long, RegistrationRequest> _registrationRequests = new Dictionary<long, RegistrationRequest>();

    public static ConcurrentDictionary<long, UserRole> _activeAdmins = new ConcurrentDictionary<long, UserRole>();

    /// <summary>
    /// Инициализирует новый экземпляр класса TelegramBotHandler.
    /// </summary>
    /// <param name="dbManager">Менеджер базы данных.</param>
    /// <param name="botToken">Токен Telegram бота.</param>
    public TelegramBotHandler(DatabaseManager dbManager, string botToken)
    {
      _dbManager = dbManager;
      _botClient = new TelegramBotClient(botToken);
    }

    /// <summary>
    /// Запускает бота и начинает обработку сообщений.
    /// </summary>
    public async Task StartBotAsync()
    {
      var cts = new CancellationTokenSource();
      var receiverOptions = new ReceiverOptions
      {
        AllowedUpdates = Array.Empty<UpdateType>()
      };

      _registrationRequests = await _dbManager.GetAllRegistrationRequestsAsync();

      _botClient.StartReceiving(
          updateHandler: HandleUpdateAsync,
          pollingErrorHandler: HandlePollingErrorAsync,
          receiverOptions: receiverOptions,
          cancellationToken: cts.Token
      );

      var me = await _botClient.GetMeAsync();
      Console.WriteLine($"Start listening for @{me.Username}");
    }


    /// <summary>
    /// Обрабатывает входящие обновления от Telegram.
    /// </summary>
    /// <param name="botClient">Клиент Telegram бота.</param>
    /// <param name="update">Объект обновления.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
      if (update.Type == UpdateType.Message && update.Message?.Text != null)
      {
        await HandleMessageAsync(update.Message, cancellationToken);
      }
      else if (update.Type == UpdateType.CallbackQuery)
      {
        await HandleCallbackQueryAsync(update.CallbackQuery, cancellationToken);
      }
    }

    #region Обработка сообщений.

    /// <summary>
    /// Обрабатывает входящие сообщения от пользователей.
    /// </summary>
    /// <param name="message">Сообщение от пользователя.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    private async Task HandleMessageAsync(Message message, CancellationToken cancellationToken)
    {
      var chatId = message.Chat.Id;
      var messageText = message.Text;
      UserRole? userRole = await GetUserRoleAsync(chatId);

      if (userRole == null)
      {
        await ProcessUserRegistrationAsync(chatId, messageText, cancellationToken);
        return;
      }
      else
      {
        string responseMessage = userRole switch
        {
          UserRole.Administrator => await HandleAdministratorMessageAsync(chatId, messageText),
          UserRole.Teacher => await HandleTeacherMessageAsync(chatId, messageText),
          UserRole.Student => await HandleStudentMessageAsync(chatId, messageText),
          _ => "Извините, произошла ошибка при определении вашей роли."
        };

        if (responseMessage.ToLower().Contains("change_role"))
        {
          await SendRoleSelectionMenuAsync(chatId, cancellationToken);
        }
      }
    }

    /// <summary>
    /// Обрабатывает сообщение от администратора.
    /// </summary>
    /// <param name="chatId">Идентификатор чата администратора.</param>
    /// <param name="message">Текст сообщения.</param>
    /// <returns>Ответ на сообщение администратора.</returns>
    private async Task<string> HandleAdministratorMessageAsync(long chatId, string message)
    {
      var userData = await _dbManager.GetUserDataAsync(chatId);
      if (userData != null)
      {
        var admin = new Administrator(userData.TelegramChatId, userData.FirstName, userData.LastName, userData.Email, _dbManager);
        return await admin.ProcessMessageAsync(message);
      }
      return "Ошибка: данные администратора не найдены.";
    }

    /// <summary>
    /// Обрабатывает сообщение от учителя.
    /// </summary>
    /// <param name="chatId">Идентификатор чата учителя.</param>
    /// <param name="message">Текст сообщения.</param>
    /// <returns>Ответ на сообщение учителя.</returns>
    private async Task<string> HandleTeacherMessageAsync(long chatId, string message)
    {
      var userData = await _dbManager.GetUserDataAsync(chatId);
      if (userData != null)
      {
        var teacher = new Teacher(userData.TelegramChatId, userData.FirstName, userData.LastName, userData.Email, _dbManager);
        return await teacher.ProcessMessageAsync(message);
      }
      return "Ошибка: данные учителя не найдены.";
    }

    /// <summary>
    /// Обрабатывает сообщение от студента.
    /// </summary>
    /// <param name="chatId">Идентификатор чата студента.</param>
    /// <param name="message">Текст сообщения.</param>
    /// <returns>Ответ на сообщение студента.</returns>
    private async Task<string> HandleStudentMessageAsync(long chatId, string message)
    {
      var userData = await _dbManager.GetUserDataAsync(chatId);
      if (userData != null)
      {
        var student = new Student(userData.TelegramChatId, userData.FirstName, userData.LastName, userData.Email, _dbManager);
        return await student.ProcessMessageAsync(message);
      }
      return "Ошибка: данные студента не найдены.";
    }

    #endregion

    #region Обработка callback-запросов

    /// <summary>
    /// Обрабатывает входящие callback-запросы от пользователей.
    /// </summary>
    /// <param name="callbackQuery">Данные callback-запроса.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    private async Task HandleCallbackQueryAsync(CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
      var chatId = callbackQuery.From.Id;
      UserRole? userRole = await GetUserRoleAsync(chatId);

      string responseMessage = string.Empty;

      if (userRole == null && callbackQuery.Data.Contains("course_"))
      {
        var number = callbackQuery.Data.Split('_');
        await ProcessUserRegistrationAsync(chatId, number[1], cancellationToken);
      }
      else if (callbackQuery.Data.StartsWith("role_"))
      {
        await HandleRoleSelectionAsync(chatId, callbackQuery.Data, cancellationToken);
      }
      else
      {
        responseMessage = userRole switch
        {
          UserRole.Administrator => await HandleAdministratorCallbackAsync(chatId, callbackQuery.Data),
          UserRole.Teacher => await HandleTeacherCallbackAsync(chatId, callbackQuery.Data),
          UserRole.Student => await HandleStudentCallbackAsync(chatId, callbackQuery.Data),
          _ => "Извините, произошла ошибка при определении вашей роли."
        };
      }

      try
      {
        if (userRole == UserRole.Administrator)
        {
          var userIdString = callbackQuery.Data.Split('_')[1];

          if (long.TryParse(userIdString, out long userId))
          {
            if (_registrationRequests.TryGetValue(userId, out var registrationRequest))
            {
              var course = await _dbManager.GetCourseNameAsync(registrationRequest.CourseId);
              string fullMessage = $"Имя: {registrationRequest.FirstName}\n" +
                                   $"Фамилия: {registrationRequest.LastName}\n" +
                                   $"Email: {registrationRequest.Email}\n" +
                                   $"Курс: {course}\n" +
                                   responseMessage;

              await _botClient.EditMessageTextAsync(
                chatId: chatId,
                messageId: callbackQuery.Message.MessageId,
                text: fullMessage,
                cancellationToken: cancellationToken
              );

              // Отправляем сообщение студенту о результате регистрации
              string studentMessage = callbackQuery.Data.StartsWith("approve")
                ? "Ваша регистрация была подтверждена администратором. Добро пожаловать!"
                : "К сожалению, ваша заявка на регистрацию была отклонена администратором.";

              await _botClient.SendTextMessageAsync(
                chatId: userId,
                text: studentMessage,
                cancellationToken: cancellationToken
              );

              _registrationRequests.Remove(userId);
            }
            else
            {
              await _botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "Ошибка: данные пользователя не найдены.",
                cancellationToken: cancellationToken
              );
            }
          }
        }
      }
      catch (Exception ex)
      {
        await _botClient.SendTextMessageAsync(
          chatId: chatId,
          text: $"Произошла ошибка при обработке запроса: {ex.Message}",
          cancellationToken: cancellationToken
        );
      }
    }

    /// <summary>
    /// Обрабатывает callback-запрос от администратора.
    /// </summary>
    /// <param name="chatId">Идентификатор чата администратора.</param>
    /// <param name="callbackData">Данные callback-запроса.</param>
    /// <returns>Ответ на callback-запрос администратора.</returns>
    private async Task<string> HandleAdministratorCallbackAsync(long chatId, string callbackData)
    {
      var userData = await _dbManager.GetUserDataAsync(chatId);
      if (userData != null)
      {
        var admin = new Administrator(userData.TelegramChatId, userData.FirstName, userData.LastName, userData.Email, _dbManager);
        return await admin.ProcessCallbackAsync(callbackData);
      }
      return "Ошибка: данные администратора не найдены.";
    }

    /// <summary>
    /// Обрабатывает callback-запрос от учителя.
    /// </summary>
    /// <param name="chatId">Идентификатор чата учителя.</param>
    /// <param name="callbackData">Данные callback-запроса.</param>
    /// <returns>Ответ на callback-запрос учителя.</returns>
    private async Task<string> HandleTeacherCallbackAsync(long chatId, string callbackData)
    {
      var userData = await _dbManager.GetUserDataAsync(chatId);
      if (userData != null)
      {
        var teacher = new Teacher(userData.TelegramChatId, userData.FirstName, userData.LastName, userData.Email, _dbManager);
        return await teacher.ProcessCallbackAsync(callbackData);
      }
      return "Ошибка: данные учителя не найдены.";
    }

    /// <summary>
    /// Обрабатывает callback-запрос от студента.
    /// </summary>
    /// <param name="chatId">Идентификатор чата студента.</param>
    /// <param name="callbackData">Данные callback-запроса.</param>
    /// <returns>Ответ на callback-запрос студента.</returns>
    private async Task<string> HandleStudentCallbackAsync(long chatId, string callbackData)
    {
      var userData = await _dbManager.GetUserDataAsync(chatId);
      if (userData != null)
      {
        var student = new Student(userData.TelegramChatId, userData.FirstName, userData.LastName, userData.Email, _dbManager);
        return await student.ProcessCallbackAsync(callbackData);
      }
      return "Ошибка: данные студента не найдены.";
    }

    #endregion

    #region Допы админы.
    /// <summary>
    /// Выбор роли
    /// </summary>
    /// <param name="chatId">Идентификатор чата администратора.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns></returns>
    private async Task SendRoleSelectionMenuAsync(long chatId, CancellationToken cancellationToken)
    {
      var keyboard = new InlineKeyboardMarkup(new[]
      {
                new []
                {
                    InlineKeyboardButton.WithCallbackData("Администратор", "role_admin"),
                    InlineKeyboardButton.WithCallbackData("Учитель", "role_teacher"),
                    InlineKeyboardButton.WithCallbackData("Студент", "role_student")
                }
            });

      await _botClient.SendTextMessageAsync(
          chatId: chatId,
          text: "Выберите роль:",
          replyMarkup: keyboard,
          cancellationToken: cancellationToken);
    }
    #endregion



    private async Task HandleRoleSelectionAsync(long chatId, string callbackData, CancellationToken cancellationToken)
    {
      UserRole selectedRole = callbackData switch
      {
        "role_admin" => UserRole.Administrator,
        "role_teacher" => UserRole.Teacher,
        "role_student" => UserRole.Student,
        _ => throw new ArgumentException("Неверный выбор роли")
      };

      _activeAdmins[chatId] = selectedRole;
      var admin = await _dbManager.GetUserDataAsync(chatId);

      string responseMessage = selectedRole switch
      {
        UserRole.Administrator => GetAdminFunctions(),
        UserRole.Teacher => GetTeacherFunctions(),
        UserRole.Student => GetStudentFunctions(),
        _ => "Произошла ошибка при выборе роли."
      };

      var keyboard = selectedRole switch
      {
        UserRole.Administrator => new Administrator(chatId, admin.FirstName, admin.LastName, admin.Email, _dbManager).Start(),
        UserRole.Teacher => new Teacher(chatId, admin.FirstName, admin.LastName, admin.Email, _dbManager).Start(),
        UserRole.Student => new Student(chatId, admin.FirstName, admin.LastName, admin.Email, _dbManager).Start(),
        _ => "Произошла ошибка при выборе роли."
      };


      await _botClient.SendTextMessageAsync(
          chatId: chatId,
          text: responseMessage,
          replyMarkup: keyboard,
          cancellationToken: cancellationToken);
    }

    private async Task HandleOtherCallbackQueryAsync(long chatId, string callbackData, CancellationToken cancellationToken)
    {
      if (_activeAdmins.TryGetValue(chatId, out UserRole role))
      {
        string responseMessage = role switch
        {
          UserRole.Administrator => await HandleAdministratorCallbackAsync(chatId, callbackData),
          UserRole.Teacher => await HandleTeacherCallbackAsync(chatId, callbackData),
          UserRole.Student => await HandleStudentCallbackAsync(chatId, callbackData),
          _ => "Произошла ошибка при обработке запроса."
        };

        await _botClient.SendTextMessageAsync(
            chatId: chatId,
            text: responseMessage,
            cancellationToken: cancellationToken);
      }
    }
    private string GetAdminFunctions()
    {
      return "Функции администратора:";
    }

    private string GetTeacherFunctions()
    {
      return "Функции учителя:";
    }

    private string GetStudentFunctions()
    {
      return "Функции студента:";
    }

    /// <summary>
    /// Обрабатывает ошибки, возникающие при получении обновлений.
    /// </summary>
    /// <param name="botClient">Клиент Telegram бота.</param>
    /// <param name="exception">Возникшее исключение.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    private Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
      var ErrorMessage = exception switch
      {
        ApiRequestException apiRequestException
            => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
        _ => exception.ToString()
      };

      Console.WriteLine(ErrorMessage);
      return Task.CompletedTask;
    }

    /// <summary>
    /// Асинхронно получает роль пользователя по идентификатору чата.
    /// </summary>
    /// <param name="chatId">Идентификатор чата пользователя.</param>
    /// <returns>Роль пользователя или null, если пользователь не найден.</returns>
    private async Task<UserRole?> GetUserRoleAsync(long chatId)
    {
      return await _dbManager.GetUserRoleAsync(chatId);
    }

    /// <summary>
    /// Обрабатывает процесс регистрации пользователя.
    /// </summary>
    /// <param name="chatId">Идентификатор чата пользователя.</param>
    /// <param name="message">Сообщение от пользователя.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    private async Task ProcessUserRegistrationAsync(long chatId, string message, CancellationToken cancellationToken)
    {
      if (!_registrationRequests.TryGetValue(chatId, out var request))
      {
        request = new RegistrationRequest(chatId);
        _registrationRequests[chatId] = request;
      }

      if (message == "/start")
      {
        await _botClient.SendTextMessageAsync(
            chatId: chatId,
            text: "Добро пожаловать! Для регистрации, пожалуйста, введите ваше имя.",
            cancellationToken: cancellationToken);
        return;
      }

      var response = await request.ProcessRegistrationStepAsync(message, _dbManager);

      if (request.Step == RegistrationStep.Course)
      {
        // Отправляем inline-клавиатуру с курсами
        var coursesKeyboard = await GetCoursesKeyboardAsync();
        await _botClient.SendTextMessageAsync(
            chatId: chatId,
            text: "Выберите курс:",
            replyMarkup: coursesKeyboard,
            cancellationToken: cancellationToken);
        return;
      }

      if (request.Step == RegistrationStep.Completed)
      {
        await NotifyAdminAboutRegistrationAsync(request, cancellationToken);
      }

      await _botClient.SendTextMessageAsync(chatId, response, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Асинхронно получает клавиатуру с курсами.
    /// </summary>
    /// <returns>Клавиатура с курсами.</returns>
    private async Task<InlineKeyboardMarkup> GetCoursesKeyboardAsync()
    {
      var courses = await _dbManager.GetCoursesAsync();
      return new InlineKeyboardMarkup(
          courses.Select(c => new[] { InlineKeyboardButton.WithCallbackData(c.Name, $"course_{c.Id}") })
      );
    }

    /// <summary>
    /// Уведомляет администратора о новой заявке на регистрацию.
    /// </summary>
    /// <param name="state">Состояние регистрации.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    private async Task NotifyAdminAboutRegistrationAsync(RegistrationRequest state, CancellationToken cancellationToken)
    {
      var adminId = await _dbManager.GetAdminIdAsync();
      if (adminId != null)
      {
        var message = $"Новая заявка на регистрацию:\n" +
                    $"Имя: {state.FirstName}\n" +
                    $"Фамилия: {state.LastName}\n" +
                    $"Email: {state.Email}\n" +
                    $"Выбранный курс: {await _dbManager.GetCourseNameAsync(state.CourseId)}";

        var keyboard = new InlineKeyboardMarkup(new[]
        {
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("Подтвердить", $"approve_{state.TelegramChatId}"),
                        InlineKeyboardButton.WithCallbackData("Отклонить", $"reject_{state.TelegramChatId}")
                    }
                });

        await _botClient.SendTextMessageAsync(
            chatId: adminId.Value,
            text: message,
            replyMarkup: keyboard,
            cancellationToken: cancellationToken);
      }
    }
  }
}