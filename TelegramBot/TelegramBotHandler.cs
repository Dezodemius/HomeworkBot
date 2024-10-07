using System.Data.SQLite;
using TelegramBot.Roles;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.ReplyMarkups;
using System.Collections.Concurrent;
using ModelInterfaceHub.Models;
using Core;

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
    /// Инициализирует новый экземпляр класса TelegramBotHandler.
    /// </summary>
    /// <param name="dbManager">Менеджер базы данных.</param>
    /// <param name="botToken">Токен Telegram бота.</param>
    public TelegramBotHandler(string botToken)
    {
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
      UserRole? userRole = CommonDataModel.GetUserRoleById(chatId);

      string responseMessage = userRole switch
      {
        UserRole.Administrator => await HandleAdministratorMessageAsync(chatId, messageText),
        UserRole.Teacher => await HandleTeacherMessageAsync(chatId, messageText),
        UserRole.Student => await HandleStudentMessageAsync(chatId, messageText),
        _ => "Извините, произошла ошибка при определении вашей роли."
      };
    }

    /// <summary>
    /// Обрабатывает сообщение от администратора.
    /// </summary>
    /// <param name="chatId">Идентификатор чата администратора.</param>
    /// <param name="message">Текст сообщения.</param>
    /// <returns>Ответ на сообщение администратора.</returns>
    private async Task<string> HandleAdministratorMessageAsync(long chatId, string message)
    {
      var userData =  CommonDataModel.GetUserById(chatId);
      if (userData != null)
      {
        var admin = new Administrator(userData.TelegramChatId, userData.FirstName, userData.LastName, userData.Email);
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
      var userData = CommonDataModel.GetUserById(chatId);
      if (userData != null)
      {
        var teacher = new Teacher(userData.TelegramChatId, userData.FirstName, userData.LastName, userData.Email);
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
      var userData = CommonDataModel.GetUserById(chatId);
      if (userData != null)
      {
        var student = new Student(userData.TelegramChatId, userData.FirstName, userData.LastName, userData.Email);
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
      UserRole? userRole = CommonDataModel.GetUserRoleById(chatId);

      string responseMessage = string.Empty;
      responseMessage = userRole switch
      {
        UserRole.Administrator => await HandleAdministratorCallbackAsync(chatId, callbackQuery.Data),
        UserRole.Teacher => await HandleTeacherCallbackAsync(chatId, callbackQuery.Data),
        UserRole.Student => await HandleStudentCallbackAsync(chatId, callbackQuery.Data),
        _ => "Извините, произошла ошибка при определении вашей роли."
      };
    }

    /// <summary>
    /// Обрабатывает callback-запрос от администратора.
    /// </summary>
    /// <param name="chatId">Идентификатор чата администратора.</param>
    /// <param name="callbackData">Данные callback-запроса.</param>
    /// <returns>Ответ на callback-запрос администратора.</returns>
    private async Task<string> HandleAdministratorCallbackAsync(long chatId, string callbackData)
    {
      var userData = CommonDataModel.GetUserById(chatId);
      if (userData != null)
      {
        var admin = new Administrator(userData.TelegramChatId, userData.FirstName, userData.LastName, userData.Email);
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
      var userData = CommonDataModel.GetUserById(chatId);
      if (userData != null)
      {
        var teacher = new Teacher(userData.TelegramChatId, userData.FirstName, userData.LastName, userData.Email);
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
      var userData = CommonDataModel.GetUserById(chatId);
      if (userData != null)
      {
        var student = new Student(userData.TelegramChatId, userData.FirstName, userData.LastName, userData.Email);
        return await student.ProcessCallbackAsync(callbackData);
      }
      return "Ошибка: данные студента не найдены.";
    }

    #endregion

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


  }
}