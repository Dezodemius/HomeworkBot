using HomeWorkTelegramBot.Core;
using HomeWorkTelegramBot.Models;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot;
using static HomeWorkTelegramBot.Config.Logger;
using Telegram.Bot.Types.ReplyMarkups;
using HomeWorkTelegramBot.Config;
using System.Globalization;

namespace HomeWorkTelegramBot.Bot.Function.Registration
{
  static internal partial class Registration
  {
    #region Data.

    /// <summary>
    /// Хранит данные регистрации пользователей, где ключ - это идентификатор чата пользователя.
    /// </summary>
    static private readonly Dictionary<long, UserRegistration> _registrationData = new Dictionary<long, UserRegistration>();

    /// <summary>
    /// Хранит текущий шаг регистрации для каждого пользователя, где ключ - это идентификатор чата пользователя.
    /// </summary>
    static private readonly Dictionary<long, RegistrationStep> _userSteps = new Dictionary<long, RegistrationStep>();

    /// <summary>
    /// Хранит информацию о том, были ли изменения в данных пользователя, где ключ - это идентификатор чата пользователя.
    /// </summary>
    static private readonly Dictionary<long, bool> _hasChanges = new Dictionary<long, bool>();

    /// <summary>
    /// Словарь обработчиков команд для редактирования данных пользователя через callback-запросы.
    /// </summary>
    private static readonly Dictionary<string, Func<ITelegramBotClient, CallbackQuery, Task>> commandHandlers = new()
{
    { "/editName", async (botClient, callbackQuery) => await EditName(botClient, callbackQuery, callbackQuery.From.Id) },
    { "/editSurname", async (botClient, callbackQuery) => await EditSurname(botClient, callbackQuery, callbackQuery.From.Id) },
    { "/editLastname", async (botClient, callbackQuery) => await EditLastname(botClient, callbackQuery, callbackQuery.From.Id) },
    { "/editEmail", async (botClient, callbackQuery) => await EditEmail(botClient, callbackQuery, callbackQuery.From.Id) },
    { "/editCourse", async (botClient, callbackQuery) => await EditCourse(botClient, callbackQuery.From.Id) }
};

    /// <summary>
    /// Перечисление, представляющее возможные шаги в процессе регистрации пользователя.
    /// </summary>
    private enum RegistrationStep
    {
      /// <summary>Шаг ввода имени.</summary>
      Name,

      /// <summary>Шаг ввода фамилии.</summary>
      Surname,

      /// <summary>Шаг ввода отчества.</summary>
      Lastname,

      /// <summary>Шаг выбора года рождения.</summary>
      BirthYear,

      /// <summary>Шаг выбора месяца рождения.</summary>
      BirthMonth,

      /// <summary>Шаг выбора дня рождения.</summary>
      BirthDay,

      /// <summary>Шаг ввода даты рождения.</summary>
      BirthDate,

      /// <summary>Шаг ввода электронной почты.</summary>
      Email,

      /// <summary>Шаг выбора курса.</summary>
      CourseSelection,

      /// <summary>Шаг завершения регистрации.</summary>
      Completed,

      /// <summary>Шаг редактирования данных.</summary>
      Edit,
    }

    #endregion

    static private async Task CheckAndCompleteRegistration(ITelegramBotClient botClient, long chatId, string input)
    {
      var currentStep = _userSteps[chatId];
      var user = _registrationData[chatId];

      switch (currentStep)
      {
        case RegistrationStep.Name:
          user.Name = input;
          LogInformation($"Имя пользователя {chatId} обновлено: {input}");
          break;
        case RegistrationStep.Surname:
          user.Surname = input;
          LogInformation($"Фамилия пользователя {chatId} обновлена: {input}");
          break;
        case RegistrationStep.Lastname:
          user.Lastname = input;
          LogInformation($"Отчество пользователя {chatId} обновлено: {input}");
          break;
        case RegistrationStep.Email:
          user.Email = input;
          LogInformation($"Email пользователя {chatId} обновлен: {input}");
          break;
        case RegistrationStep.CourseSelection:
          // Обработка выбора курса может быть сложнее, так как это может быть не просто текст
          break;
      }

      _userSteps[chatId] = RegistrationStep.Completed;
      _hasChanges[chatId] = false; // Сбрасываем флаг изменений
      await CompleteRegistration(botClient, chatId);
    }

    /// <summary>
    /// Инициализирует процесс регистрации для нового пользователя.
    /// </summary>
    static private async Task InitializeRegistration(ITelegramBotClient botClient, long chatId)
    {
      _userSteps[chatId] = RegistrationStep.Name;
      _registrationData[chatId] = new UserRegistration { ChatId = chatId };
      LogInformation($"Начало регистрации для пользователя с ChatId {chatId}");
      await TelegramBotHandler.SendMessageAsync(botClient, chatId, "Пожалуйста, введите ваше имя:");
    }

    /// <summary>
    /// Обрабатывает нажатие на выбранный курс.
    /// </summary>
    static private async Task HandleCourseSelection(ITelegramBotClient botClient, long chatId, CallbackQuery callbackQuery, UserRegistration user)
    {
      string data = callbackQuery.Data;
      if (data.StartsWith("/selectcourse_"))
      {
        int courseId = int.Parse(data.Replace("/selectcourse_", ""));
        user.CourseId = courseId;
        _userSteps[chatId] = RegistrationStep.Completed;
        LogInformation($"Курс {courseId} выбран для пользователя с ChatId {chatId}");
        await TelegramBotHandler.SendMessageAsync(botClient, chatId, $"Был выбран курс: {courseId}", null, callbackQuery.Message.Id);
        await CompleteRegistration(botClient, chatId);
      }
    }

    /// <summary>
    /// Завершает регистрацию пользователя.
    /// </summary>
    static private async Task CompleteRegistration(ITelegramBotClient botClient, long chatId)
    {
      if (_registrationData.TryGetValue(chatId, out var user))
      {
        string userInfo = $"Проверьте ваши данные:\n" +
                          $"Имя: {user.Name}\n" +
                          $"Фамилия: {user.Surname}\n" +
                          $"Отчество: {user.Lastname}\n" +
                          $"Дата рождения: {user.BirthDate}\n" +
                          $"Email: {user.Email}\n" +
                          $"Курс: {user.CourseId}";

        List<CallbackModel> callbacks = new List<CallbackModel>
        {
            new CallbackModel("Отправить", $"/submit_{chatId}"),
            new CallbackModel("Изменить", $"/edit_{chatId}")
        };

        await TelegramBotHandler.SendMessageAsync(botClient, chatId, userInfo, TelegramBotHandler.GetInlineKeyboardMarkupAsync(callbacks));
      }
    }

    /// <summary>
    /// Обрабатывает отправку данных пользователя на сервер.
    /// </summary>
    static private async Task HandleSubmit(ITelegramBotClient botClient, CallbackQuery callbackQuery, long chatId)
    {
      if (_registrationData.TryGetValue(chatId, out var user))
      {
        UserRegistrationService.AddUserRegistration(user);
        LogInformation($"Регистрация завершена для пользователя с ChatId {chatId}");
        await TelegramBotHandler.SendMessageAsync(botClient, chatId, "Регистрация завершена. Ожидайте подтверждение от администратора!", null, callbackQuery.Message.Id);
        _registrationData.Remove(chatId);
        _userSteps.Remove(chatId);
        await SendRegistrationToAdmin(botClient, user);
      }
    }

    /// <summary>
    /// Обрабатывает запрос на редактирование данных пользователя.
    /// </summary>
    static private async Task HandleEdit(ITelegramBotClient botClient, CallbackQuery callbackQuery, long chatId)
    {
      if (callbackQuery.Data.Contains("/edit_"))
      {
        List<CallbackModel> editOptions = new List<CallbackModel>
        {
          new CallbackModel("Имя", $"/editName_{chatId}"),
          new CallbackModel("Фамилия", $"/editSurname_{chatId}"),
          new CallbackModel("Отчество", $"/editLastname_{chatId}"),
          new CallbackModel("Email", $"/editEmail_{chatId}"),
          new CallbackModel("Курс", $"/editCourse_{chatId}")
        };

        await TelegramBotHandler.SendMessageAsync(botClient, chatId, "Выберите, что вы хотите изменить:", TelegramBotHandler.GetInlineKeyboardMarkupAsync(editOptions), callbackQuery.Message.Id);
        _userSteps[chatId] = RegistrationStep.Edit;
      }
      else
      {
        foreach (var command in commandHandlers.Keys)
        {
          if (callbackQuery.Data.StartsWith(command))
          {
            await commandHandlers[command](botClient, callbackQuery);
            return;
          }
        }
      }
    }

    /// <summary>
    /// Отправляет информацию о пользователе администратору для подтверждения.
    /// </summary>
    static private async Task SendRegistrationToAdmin(ITelegramBotClient botClient, UserRegistration user)
    {
      long adminChatId = ApplicationData.ConfigApp.AdminId;

      string userInfo = $"Новая регистрация:\n" +
                        $"Имя: {user.Name}\n" +
                        $"Фамилия: {user.Surname}\n" +
                        $"Отчество: {user.Lastname}\n" +
                        $"Дата рождения: {user.BirthDate}\n" +
                        $"Email: {user.Email}\n" +
                        $"Курс: {user.CourseId}";

      List<CallbackModel> callbacks = new List<CallbackModel>();
      callbacks.Add(new CallbackModel("Принять", $"/approve_{user.ChatId}"));
      callbacks.Add(new CallbackModel("Отказать", $"/reject_{user.ChatId}"));

      await TelegramBotHandler.SendMessageAsync(botClient, adminChatId, userInfo, TelegramBotHandler.GetInlineKeyboardMarkupAsync(callbacks));
    }
  }
}