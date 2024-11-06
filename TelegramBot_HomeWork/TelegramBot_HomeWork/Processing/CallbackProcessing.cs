using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core;
using DataContracts.Models;
using Telegram.Bot.Types;
using Telegram.Bot;
using TelegramBot.Roles.Administrator;
using TelegramBot.Roles.Student;
using TelegramBot.Roles.Teacher;

namespace TelegramBot.Processing
{
  static internal class CallbackProcessing
  {

    /// <summary>
    /// Обрабатывает входящие callback-запросы от пользователей.
    /// </summary>
    /// <param name="callbackQuery">Данные callback-запроса.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    static internal async Task HandleCallbackQueryAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
      var chatId = callbackQuery.From.Id;

      UserRole? userRole = CommonUserModel.GetUserRoleByChatId(chatId);
      bool responseMessage = userRole switch
      {
        UserRole.Administrator => await HandleAdministratorCallbackAsync(botClient, chatId, callbackQuery.Data, callbackQuery.Message.MessageId),
        UserRole.Teacher => await HandleTeacherCallbackAsync(botClient, chatId, callbackQuery.Data, callbackQuery.Message.MessageId),
        UserRole.Student => await HandleStudentCallbackAsync(botClient, chatId, callbackQuery.Data, callbackQuery.Message.MessageId),
        _ => false
      };

      if (!responseMessage)
      {
        if (callbackQuery.Data.ToLower().Contains("registration"))
        {
          TelegramBotHandler.RegistrationRequests.Add(chatId, new RegistrationRequest(chatId));
        }

        if (TelegramBotHandler.RegistrationRequests.ContainsKey(chatId))
        {
          TelegramBotHandler.RegistrationRequests.TryGetValue(chatId, out RegistrationRequest data);
          if (data != null)
          {
            TelegramBotHandler.RegistrationRequests.Remove(chatId);
            await new RegistrationProcessing(data).ProcessRegistrationStepAsync(botClient, chatId, callbackQuery.Data);
            TelegramBotHandler.RegistrationRequests.Add(chatId, data);
          }
        }
        else
        {
          await TelegramBotHandler.SendMessageAsync(botClient, chatId, "Ошибка: Данные пользователя не найдены!");
        }
      }
    }

    /// <summary>
    /// Обрабатывает callback-запрос от администратора.
    /// </summary>
    /// <param name="chatId">Идентификатор чата администратора.</param>
    /// <param name="callbackData">Данные callback-запроса.</param>
    /// <returns>Ответ на callback-запрос администратора.</returns>
    static private async Task<bool> HandleAdministratorCallbackAsync(ITelegramBotClient botClient, long chatId, string callbackData, int messageId)
    {
      var userData = CommonUserModel.GetUserByChatId(chatId);
      if (userData != null)
      {
        var admin = new Administrator(userData.TelegramChatId, userData.FirstName, userData.LastName, userData.Email);
        await admin.ProcessCallbackAsync(botClient, chatId, callbackData, messageId);
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
    static private async Task<bool> HandleTeacherCallbackAsync(ITelegramBotClient botClient, long chatId, string callbackData, int messageId)
    {
      var userData = CommonUserModel.GetUserByChatId(chatId);
      if (userData != null)
      {
        var teacher = new Teacher(userData.TelegramChatId, userData.FirstName, userData.LastName, userData.Email);
        await teacher.ProcessCallbackAsync(botClient, chatId, callbackData, messageId);
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
    static private async Task<bool> HandleStudentCallbackAsync(ITelegramBotClient botClient, long chatId, string callbackData, int messageId)
    {
      var userData = CommonUserModel.GetUserByChatId(chatId);
      if (userData != null)
      {
        var student = new Student(userData.TelegramChatId, userData.FirstName, userData.LastName, userData.Email);
        await student.ProcessCallbackAsync(botClient, chatId, callbackData, messageId);
        return true;
      }
      return false;
    }

  }
}
