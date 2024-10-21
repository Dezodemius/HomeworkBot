using Core;
using DataContracts.Models;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBot.Roles.Administrator;
using TelegramBot.Roles.Teacher;
using TelegramBot.Roles.Student;

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
        await HandleMessageAsync(botClient, update.Message, cancellationToken);
      }
      else if (update.Type == UpdateType.CallbackQuery)
      {
        await HandleCallbackQueryAsync(botClient, update.CallbackQuery, cancellationToken);
      }
    }

    #region Обработка сообщений.

    private async Task HandleMessageAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
      var chatId = message.Chat.Id;
      var messageText = message.Text;

      UserRole? userRole = CommonUserModel.GetUserRoleById(chatId);
      bool responseMessage = userRole switch
      {
        UserRole.Administrator => await HandleAdministratorMessageAsync(chatId, messageText),
        UserRole.Teacher => await HandleTeacherMessageAsync(chatId, messageText),
        UserRole.Student => await HandleStudentMessageAsync(chatId, messageText),
        _ => false
      };

      if (!responseMessage)
      {
        // TODO : Сделать регистрацию...
        await SendMessageAsync(botClient, chatId, "Ошибка: Данные пользователя не найдены!");
      }
    }

    /// <summary>
    /// Обрабатывает сообщение от администратора.
    /// </summary>
    /// <param name="chatId">Идентификатор чата администратора.</param>
    /// <param name="message">Текст сообщения.</param>
    /// <returns>Ответ на сообщение администратора.</returns>
    private async Task<bool> HandleAdministratorMessageAsync(long chatId, string message)
    {
      var userData = CommonUserModel.GetUserById(chatId);
      if (userData != null)
      {
        var admin = new Administrator(userData.TelegramChatId, userData.FirstName, userData.LastName, userData.Email);
        await admin.ProcessMessageAsync(_botClient, chatId, message);
        return true;
      }
      return false;
    }

    /// <summary>
    /// Обрабатывает сообщение от учителя.
    /// </summary>
    /// <param name="chatId">Идентификатор чата учителя.</param>
    /// <param name="message">Текст сообщения.</param>
    /// <returns>Ответ на сообщение учителя.</returns>
    private async Task<bool> HandleTeacherMessageAsync(long chatId, string message)
    {
      var userData = CommonUserModel.GetUserById(chatId);
      if (userData != null)
      {
        var teacher = new Teacher(userData.TelegramChatId, userData.FirstName, userData.LastName, userData.Email);
        await teacher.ProcessMessageAsync(_botClient, chatId, message);
        return true;
      }
      return false;
    }

    /// <summary>
    /// Обрабатывает сообщение от студента.
    /// </summary>
    /// <param name="chatId">Идентификатор чата студента.</param>
    /// <param name="message">Текст сообщения.</param>
    /// <returns>Ответ на сообщение студента.</returns>
    private async Task<bool> HandleStudentMessageAsync(long chatId, string message)
    {
      var userData = CommonUserModel.GetUserById(chatId);
      if (userData != null)
      {
        var student = new Student(userData.TelegramChatId, userData.FirstName, userData.LastName, userData.Email);
        await student.ProcessMessageAsync(_botClient, chatId, message);
        return true;
      }
      return false;
    }


    #endregion

    #region Обработка callback-запросов

    /// <summary>
    /// Обрабатывает входящие callback-запросы от пользователей.
    /// </summary>
    /// <param name="callbackQuery">Данные callback-запроса.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    private async Task HandleCallbackQueryAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
      var chatId = callbackQuery.From.Id;

      UserRole? userRole = CommonUserModel.GetUserRoleById(chatId);

      if (userRole is null)
      {
        await _botClient.SendTextMessageAsync(chatId, "Ошибка: Данные пользователя не найдены!");
        return;
      }

      bool responseMessage = userRole switch
      {
        UserRole.Administrator => await HandleAdministratorCallbackAsync(chatId, callbackQuery.Data, callbackQuery.Message.MessageId),
        UserRole.Teacher => await HandleTeacherCallbackAsync(chatId, callbackQuery.Data, callbackQuery.Message.MessageId),
        UserRole.Student => await HandleStudentCallbackAsync(chatId, callbackQuery.Data, callbackQuery.Message.MessageId),
        _ => false
      };

      if (!responseMessage)
      {
        await SendMessageAsync(botClient, chatId, "Ошибка при обработке запроса.");
      }

    }

    /// <summary>
    /// Обрабатывает callback-запрос от администратора.
    /// </summary>
    /// <param name="chatId">Идентификатор чата администратора.</param>
    /// <param name="callbackData">Данные callback-запроса.</param>
    /// <returns>Ответ на callback-запрос администратора.</returns>
    private async Task<bool> HandleAdministratorCallbackAsync(long chatId, string callbackData, int messageId)
    {
      var userData = CommonUserModel.GetUserById(chatId);
      if (userData != null)
      {
        var admin = new Administrator(userData.TelegramChatId, userData.FirstName, userData.LastName, userData.Email);
        await admin.ProcessCallbackAsync(_botClient, chatId, callbackData, messageId);
        return true;
      }
      return false;
    }

    /// <summary>
    /// Обрабатывает callback-запрос от учителя.
    /// </summary>
    /// <param name="chatId">Идентификатор чата учителя.</param>
    /// <param name="callbackData">Данные callback-запроса.</param>
    /// <returns>Ответ на callback-запрос учителя.</returns>
    private async Task<bool> HandleTeacherCallbackAsync(long chatId, string callbackData, int messageId)
    {
      var userData = CommonUserModel.GetUserById(chatId);
      if (userData != null)
      {
        var teacher = new Teacher(userData.TelegramChatId, userData.FirstName, userData.LastName, userData.Email);
        await teacher.ProcessCallbackAsync(_botClient, chatId, callbackData, messageId);
        return true;
      }
      return false;
    }

    /// <summary>
    /// Обрабатывает callback-запрос от студента.
    /// </summary>
    /// <param name="chatId">Идентификатор чата студента.</param>
    /// <param name="callbackData">Данные callback-запроса.</param>
    /// <returns>Ответ на callback-запрос студента.</returns>
    private async Task<bool> HandleStudentCallbackAsync(long chatId, string callbackData, int messageId)
    {
      var userData = CommonUserModel.GetUserById(chatId);
      if (userData != null)
      {
        var student = new Student(userData.TelegramChatId, userData.FirstName, userData.LastName, userData.Email);
        await student.ProcessCallbackAsync(_botClient, chatId, callbackData, messageId);
        return true;
      }
      return false;
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

    /// <summary>
    /// Асинхронно отправляет или редактирует сообщение пользователю через Telegram бота.
    /// </summary>
    /// <param name="botClient">Клиент Telegram бота.</param>
    /// <param name="chatId">Идентификатор чата пользователя.</param>
    /// <param name="message">Текст сообщения для отправки или редактирования.</param>
    /// <param name="inlineKeyboardMarkup">Опциональная встроенная клавиатура.</param>
    /// <param name="messageId">Идентификатор сообщения для редактирования (если есть).</param>
    /// <returns>Задача, представляющая асинхронную операцию отправки или редактирования сообщения.</returns>
    public static async Task SendMessageAsync(ITelegramBotClient botClient, long chatId, string message, InlineKeyboardMarkup inlineKeyboardMarkup = null, int? messageId = null)
    {
      if (inlineKeyboardMarkup == null)
      {
        await botClient.SendTextMessageAsync(chatId, message);
      }
      else if (messageId.HasValue)
      {
        await botClient.EditMessageTextAsync(chatId, messageId.Value, message, replyMarkup: inlineKeyboardMarkup);
      }
      else
      {
        await botClient.SendTextMessageAsync(chatId, message, replyMarkup: inlineKeyboardMarkup);
      }
    }
  }
}