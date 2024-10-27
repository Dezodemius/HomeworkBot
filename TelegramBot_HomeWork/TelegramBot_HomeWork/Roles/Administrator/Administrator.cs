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
    internal Administrator(long telegramChatId, string firstName, string lastName, string email)
        : base(telegramChatId, firstName, lastName, email, UserRole.Administrator) { }

    /// <summary>
    /// Обрабатывает входящее сообщение от администратора.
    /// </summary>
    public async Task ProcessMessageAsync(ITelegramBotClient botClient, long chatId, string message)
    {
      if (string.IsNullOrEmpty(message))
      {
        return;
      }
      else if (course.ContainsKey(chatId))
      {
        await ProcessCourseCreation(botClient, chatId, message);
      }
      else if (changeUserRole.ContainsKey(chatId))
      {
        await ProcessUserRoleChange(botClient, chatId, message);
      }
      else if (message == "/start")
      {
        await ShowAdminOptions(botClient, chatId);
      }
    }

    /// <summary>
    /// Обрабатывает создание курса для администратора.
    /// </summary>
    private async Task ProcessCourseCreation(ITelegramBotClient botClient, long chatId, string message)
    {
      if (course.TryGetValue(chatId, out Course? courseData) && courseData != null)
      {
        await new CreateCourseProcessing(courseData).ProcessCreateCourseStepAsync(botClient, chatId, message, 0);
      }
    }

    /// <summary>
    /// Обрабатывает запрос на изменение роли пользователя.
    /// </summary>
    private async Task ProcessUserRoleChange(ITelegramBotClient botClient, long chatId, string message)
    {
      if (changeUserRole.TryGetValue(chatId, out UserModel? userData) && userData != null)
      {
        await new ChangeUserRole(userData).ChangeRole(botClient, chatId, message);
      }
    }

    /// <summary>
    /// Отображает доступные функции администратора.
    /// </summary>
    private async Task ShowAdminOptions(ITelegramBotClient botClient, long chatId)
    {
      var callbackModels = new List<CallbackModel>
      {
        new CallbackModel("Создать курс", "/createCourse"),
        new CallbackModel("Сменить роль пользователя", "/changeUserRole")
      };
      await TelegramBotHandler.SendMessageAsync(botClient, chatId, "Выберите функцию:", TelegramBotHandler.GetInlineKeyboardMarkupAsync(callbackModels));
    }

    /// <summary>
    /// Обработка Callback запросов от администратора.
    /// </summary>
    public async Task ProcessCallbackAsync(ITelegramBotClient botClient, long chatId, string callbackData, int messageId)
    {
      if (string.IsNullOrEmpty(callbackData)) return;

      if (callbackData.ToLower().Contains("/registration"))
      {
        await ProcessRegistrationCallback(botClient, chatId, messageId, callbackData);
      }
      else if (callbackData.ToLower().Contains("/createcourse"))
      {
        await StartCourseCreation(botClient, chatId, callbackData, messageId);
      }
      else if (callbackData.ToLower().Contains("/changeuserrole"))
      {
        await HandleUserRoleChangeCallback(botClient, chatId, callbackData);
      }
    }

    /// <summary>
    /// Обработка запросов на регистрацию от администратора.
    /// </summary>
    private async Task ProcessRegistrationCallback(ITelegramBotClient botClient, long chatId, int messageId, string callbackData)
    {
      var result = await RegistrationProcessing.AnswerRegistrationUser(botClient, chatId, messageId, callbackData);
    }

    /// <summary>
    /// Инициализирует создание курса.
    /// </summary>
    private async Task StartCourseCreation(ITelegramBotClient botClient, long chatId, string callbackData, int messageId)
    {
      if (!course.ContainsKey(chatId))
      {
        course.Add(chatId, new Course());
      }

      if (course.TryGetValue(chatId, out Course? courseData) && courseData != null)
      {
        await new CreateCourseProcessing(courseData).ProcessCreateCourseStepAsync(botClient, chatId, callbackData, messageId);
      }
    }

    /// <summary>
    /// Обработка Callback запросов на смену роли пользователя.
    /// </summary>
    private async Task HandleUserRoleChangeCallback(ITelegramBotClient botClient, long chatId, string callbackData)
    {
      if (callbackData.ToLower().Contains("/changeuserrole_id_"))
      {
        await StartUserRoleChange(botClient, chatId, callbackData);
      }
      else if (callbackData.ToLower().Contains("/changeuserrole_role_"))
      {
        await ContinueUserRoleChange(botClient, chatId, callbackData);
      }
      else if (!changeUserRole.ContainsKey(chatId))
      {
        await ShowUserRoleOptions(botClient, chatId);
      }
      else
      {
        await ContinueUserRoleChange(botClient, chatId, callbackData);
      }
    }

    /// <summary>
    /// Начинает процесс изменения роли пользователя.
    /// </summary>
    private async Task StartUserRoleChange(ITelegramBotClient botClient, long chatId, string callbackData)
    {
      var data = callbackData.Split('_');
      long.TryParse(data.Last(), out long id);
      var userModel = CommonUserModel.GetUserById(id);

      if (userModel != null)
      {
        changeUserRole[chatId] = userModel;
        userModel.SetChangeStep(ChangeRoleStep.Role);
        await new ChangeUserRole(userModel).ChangeRole(botClient, chatId, callbackData);
      }
    }

    /// <summary>
    /// Продолжает процесс изменения роли пользователя.
    /// </summary>
    private async Task ContinueUserRoleChange(ITelegramBotClient botClient, long chatId, string callbackData)
    {
      if (changeUserRole.TryGetValue(chatId, out UserModel? userModel) && userModel != null)
      {
        await new ChangeUserRole(userModel).ChangeRole(botClient, chatId, callbackData);
      }
    }

    /// <summary>
    /// Отображает доступных пользователей для смены роли.
    /// </summary>
    private async Task ShowUserRoleOptions(ITelegramBotClient botClient, long chatId)
    {
      var users = CommonUserModel.GetAllUsers();
      var callbackModels = users
          .Where(user => user.Role != UserRole.Administrator)
          .Select(user => new CallbackModel($"{user.LastName} {user.FirstName} - {user.Role}", $"/changeuserrole_id_{user.TelegramChatId}"))
          .ToList();

      await TelegramBotHandler.SendMessageAsync(botClient, chatId, "Выберите пользователя для смены роли:", TelegramBotHandler.GetInlineKeyboardMarkupAsync(callbackModels));
    }
  }
}
