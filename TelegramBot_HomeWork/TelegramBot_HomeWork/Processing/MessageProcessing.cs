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
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBot.Model;

namespace TelegramBot.Processing
{
  static internal class MessageProcessing
  {

    static internal async Task HandleMessageAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
      var chatId = message.Chat.Id;
      var messageText = message.Text;

      UserRole? userRole = CommonUserModel.GetUserRoleById(chatId);
      bool responseMessage = userRole switch
      {
        UserRole.Administrator => await HandleAdministratorMessageAsync(botClient, chatId, messageText),
        UserRole.Teacher => await HandleTeacherMessageAsync(botClient, chatId, messageText),
        UserRole.Student => await HandleStudentMessageAsync(botClient, chatId, messageText),
        _ => false
      };

      if (!responseMessage)
      {
        if (TelegramBotHandler.RegistrationRequests.ContainsKey(chatId))
        {
          TelegramBotHandler.RegistrationRequests.TryGetValue(chatId, out RegistrationRequest data);
          if (data != null)
          {
            TelegramBotHandler.RegistrationRequests.Remove(chatId);
            await new RegistrationProcessing(data).ProcessRegistrationStepAsync(botClient, chatId, messageText);
            TelegramBotHandler.RegistrationRequests.Add(chatId, data);
          }
        }
        else
        {
          List<CallbackModel> callbackModels = new List<CallbackModel>()
          {
            new CallbackModel("Регистрация","/registration"),
          };

          var inlineKeyboardMarkup = TelegramBotHandler.GetInlineKeyboardMarkupAsync(callbackModels);
          await TelegramBotHandler.SendMessageAsync(botClient, chatId, "Ошибка: Данные пользователя не найдены!", inlineKeyboardMarkup);
        }
      }
    }

    /// <summary>
    /// Обрабатывает сообщение от администратора.
    /// </summary>
    /// <param name="chatId">Идентификатор чата администратора.</param>
    /// <param name="message">Текст сообщения.</param>
    /// <returns>Ответ на сообщение администратора.</returns>
    static private async Task<bool> HandleAdministratorMessageAsync(ITelegramBotClient botClient, long chatId, string message)
    {
      var userData = CommonUserModel.GetUserById(chatId);
      if (userData != null)
      {
        var admin = new Administrator(userData.TelegramChatId, userData.FirstName, userData.LastName, userData.Email);
        await admin.ProcessMessageAsync(botClient, chatId, message);
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
    static private async Task<bool> HandleTeacherMessageAsync(ITelegramBotClient botClient, long chatId, string message)
    {
      var userData = CommonUserModel.GetUserById(chatId);
      if (userData != null)
      {
        var teacher = new Teacher(userData.TelegramChatId, userData.FirstName, userData.LastName, userData.Email);
        await teacher.ProcessMessageAsync(botClient, chatId, message);
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
    static private async Task<bool> HandleStudentMessageAsync(ITelegramBotClient botClient, long chatId, string message)
    {
      var userData = CommonUserModel.GetUserById(chatId);
      if (userData != null)
      {
        var student = new Student(userData.TelegramChatId, userData.FirstName, userData.LastName, userData.Email);
        await student.ProcessMessageAsync(botClient, chatId, message);
        return true;
      }
      return false;
    }


  }
}
