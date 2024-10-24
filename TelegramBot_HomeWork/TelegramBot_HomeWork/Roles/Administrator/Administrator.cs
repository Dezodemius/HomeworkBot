using Core;
using DataContracts.Interfaces;
using DataContracts.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramBot.Model;
using TelegramBot.Processing;

namespace TelegramBot.Roles.Administrator
{
  internal class Administrator : UserModel, IMessageHandler
  {
    static internal Dictionary<long, Course> course = new Dictionary<long, Course>();
    static internal Dictionary<long, UserModel> changeUserRole = new Dictionary<long, UserModel>();

    /// <summary>
    /// Инициализирует новый экземпляр класса Administrator.
    /// </summary>
    /// <param name="telegramChatId">Идентификатор чата Telegram.</param>
    /// <param name="firstName">Имя администратора.</param>
    /// <param name="lastName">Фамилия администратора.</param>
    /// <param name="email">Адрес электронной почты администратора.</param>
    /// <param name="dbManager">Менеджер базы данных.</param>
    internal Administrator(long telegramChatId, string firstName, string lastName, string email)
        : base(telegramChatId, firstName, lastName, email, UserRole.Administrator) { }


    /// <summary>
    /// Обрабатывает входящее сообщение от администратора.
    /// </summary>
    /// <param name="message">Текст сообщения от администратора.</param>
    /// <returns>Ответ на сообщение администратора.</returns>
    public async Task ProcessMessageAsync(ITelegramBotClient botClient, long chatId, string message)
    {
      if (string.IsNullOrEmpty(message))
      {
        return;
      }
      else if (course.ContainsKey(chatId))
      {
        course.TryGetValue(chatId, out Course? courseData);

        if (courseData != null)
        {
          await new CreateCourseProcessing(courseData).ProcessCreateCourseStepAsync(botClient, chatId, message, 0);
        }
      }
      else if (changeUserRole.ContainsKey(chatId))
      {
        changeUserRole.TryGetValue(chatId, out UserModel? userData);
        if (userData != null)
        {
          await new ChangeUserRole(userData).ChangeRole(botClient, chatId, message);
        }
      }
      else if (message == "/start")
      {
        List<CallbackModel> callbackModels = new List<CallbackModel>();
        callbackModels.Add(new CallbackModel("Создать курс", "/createCourse"));
        callbackModels.Add(new CallbackModel("Сменить роль пользователя", "/changeUserRole"));
        await TelegramBotHandler.SendMessageAsync(botClient, chatId, "Выберите функцию:", TelegramBotHandler.GetInlineKeyboardMarkupAsync(callbackModels));
      }
    }

    /// <summary>
    /// Обработка Callback запросов от администратора.
    /// </summary>
    /// <param name="callbackData"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public async Task ProcessCallbackAsync(ITelegramBotClient botClient, long chatId, string callbackData, int messageId)
    {
      if (string.IsNullOrEmpty(callbackData))
      {
        return;
      }
      else if (callbackData.ToLower().Contains("/registration"))
      {
        var result = RegistrationProcessing.AnswerRegistrationUser(botClient, chatId, messageId, callbackData);
      }
      else if (callbackData.ToLower().Contains("/createcourse"))
      {
        if (!course.ContainsKey(chatId))
        {
          course.Add(chatId, new Course());
        }

        course.TryGetValue(chatId, out Course? courseData);

        if (courseData != null)
        {
          await new CreateCourseProcessing(courseData).ProcessCreateCourseStepAsync(botClient, chatId, callbackData, messageId);
        }
      }

      else if (callbackData.ToLower().Contains("/changeuserrole"))
      {
        if (callbackData.ToLower().Contains("/changeuserrole_id_"))
        {
          var data = callbackData.Split('_');
          long.TryParse(data.Last(), out long id);
          var userModel = CommonUserModel.GetUserById(id);
          changeUserRole.Add(chatId, userModel);

          userModel.SetChangeStep(ChangeRoleStep.Role);
          if (userModel != null)
          {
            await new ChangeUserRole(userModel).ChangeRole(botClient, chatId, callbackData);
          }
        }
        else if (callbackData.ToLower().Contains("/changeuserrole_role_"))
        {
          var data = callbackData.Split('_');
          long.TryParse(data.Last(), out long id);
          changeUserRole.TryGetValue(chatId, out var userModel);

          if (userModel != null)
          {
            await new ChangeUserRole(userModel).ChangeRole(botClient, chatId, callbackData);
          }
        }
        else if (!changeUserRole.ContainsKey(chatId))
        {
          var data = CommonUserModel.GetAllUsers();
          List<CallbackModel> callbackModels = new List<CallbackModel>();
          foreach (var item in data)
          {
            if (item.Role != UserRole.Administrator)
            {
              callbackModels.Add(new CallbackModel($"{item.LastName} {item.FirstName} - {item.Role}", $"/changeuserrole_id_{item.TelegramChatId}"));
            }
          }
          await TelegramBotHandler.SendMessageAsync(botClient, chatId, "Выберите пользователя для смены роли:", TelegramBotHandler.GetInlineKeyboardMarkupAsync(callbackModels));
        }
        else
        {
          changeUserRole.TryGetValue(chatId, out UserModel? userData);
          if (userData != null)
          {
            await new ChangeUserRole(userData).ChangeRole(botClient, chatId, callbackData);
          }
        }
      }
    }
  }
}
