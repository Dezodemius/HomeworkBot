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

namespace HomeWorkTelegramBot.Bot.Function
{
  static internal class Registration
  {
    static private readonly Dictionary<long, UserRegistration> _registrationData = new Dictionary<long, UserRegistration>();
    static private readonly Dictionary<long, RegistrationStep> _userSteps = new Dictionary<long, RegistrationStep>();

    private enum RegistrationStep
    {
      Name,
      Surname,
      Lastname,
      BirthDate,
      Email,
      CourseSelection,
      Completed
    }

    /// <summary>
    /// Обрабатывает шаг регистрации для пользователя.
    /// </summary>
    static public async Task ProcessRegistrationStep(ITelegramBotClient botClient, Message message)
    {
      long chatId = message.Chat.Id;
      string input = message.Text;

      if (!_userSteps.ContainsKey(chatId))
      {
        await InitializeRegistration(botClient, chatId);
        return;
      }

      var currentStep = _userSteps[chatId];
      var user = _registrationData[chatId];

      switch (currentStep)
      {
        case RegistrationStep.Name:
          await ProcessNameStep(botClient, chatId, input, user);
          break;

        case RegistrationStep.Surname:
          await ProcessSurnameStep(botClient, chatId, input, user);
          break;

        case RegistrationStep.Lastname:
          await ProcessLastnameStep(botClient, chatId, input, user);
          break;

        case RegistrationStep.BirthDate:
          await ProcessBirthDateStep(botClient, chatId, input, user);
          break;

        case RegistrationStep.Email:
          await ProcessEmailStep(botClient, chatId, input, user);
          break;

        case RegistrationStep.CourseSelection:
          await ProcessCourseSelection(botClient, chatId, user);
          break;

        case RegistrationStep.Completed:
          LogInformation($"Регистрация пользователя {chatId} уже завершена.");
          break;
      }
    }

    /// <summary>
    /// Обрабатывает шаг регистрации для пользователя через callback-запрос.
    /// </summary>
    static public async Task ProcessRegistrationStep(ITelegramBotClient botClient, CallbackQuery callbackQuery)
    {
      long chatId = callbackQuery.From.Id;
      string data = callbackQuery.Data;

      if (!_userSteps.ContainsKey(chatId))
      {
        await InitializeRegistration(botClient, chatId);
        return;
      }

      var currentStep = _userSteps[chatId];
      var user = _registrationData[chatId];

      switch (currentStep)
      {
        case RegistrationStep.CourseSelection:
          await HandleCourseSelection(botClient, chatId, callbackQuery, user);
          break;

        case RegistrationStep.Completed:
          LogInformation($"Регистрация пользователя {chatId} уже завершена.");
          break;
      }
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
    /// Обрабатывает ввод имени пользователя.
    /// </summary>
    static private async Task ProcessNameStep(ITelegramBotClient botClient, long chatId, string input, UserRegistration user)
    {
      if (string.IsNullOrWhiteSpace(input) || !Regex.IsMatch(input, @"^[a-zA-Zа-яА-Я]+$"))
      {
        await TelegramBotHandler.SendMessageAsync(botClient, chatId, "Имя должно содержать только буквы. Пожалуйста, введите ваше имя:");
        return;
      }

      user.Name = input;
      _userSteps[chatId] = RegistrationStep.Surname;
      LogInformation($"Имя пользователя {chatId} установлено: {input}");
      await TelegramBotHandler.SendMessageAsync(botClient, chatId, "Пожалуйста, введите вашу фамилию:");
    }

    /// <summary>
    /// Обрабатывает ввод фамилии пользователя.
    /// </summary>
    static private async Task ProcessSurnameStep(ITelegramBotClient botClient, long chatId, string input, UserRegistration user)
    {
      if (string.IsNullOrWhiteSpace(input) || !Regex.IsMatch(input, @"^[a-zA-Zа-яА-Я]+$"))
      {
        await TelegramBotHandler.SendMessageAsync(botClient, chatId, "Фамилия должна содержать только буквы. Пожалуйста, введите вашу фамилию:");
        return;
      }

      user.Surname = input;
      _userSteps[chatId] = RegistrationStep.Lastname;
      LogInformation($"Фамилия пользователя {chatId} установлена: {input}");
      await TelegramBotHandler.SendMessageAsync(botClient, chatId, "Пожалуйста, введите ваше отчество:");
    }

    /// <summary>
    /// Обрабатывает ввод отчества пользователя.
    /// </summary>
    static private async Task ProcessLastnameStep(ITelegramBotClient botClient, long chatId, string input, UserRegistration user)
    {
      if (string.IsNullOrWhiteSpace(input) || !Regex.IsMatch(input, @"^[a-zA-Zа-яА-Я]+$"))
      {
        await TelegramBotHandler.SendMessageAsync(botClient, chatId, "Отчество должно содержать только буквы. Пожалуйста, введите ваше отчество:");
        return;
      }

      user.Lastname = input;
      _userSteps[chatId] = RegistrationStep.BirthDate;
      LogInformation($"Отчество пользователя {chatId} установлено: {input}");
      await TelegramBotHandler.SendMessageAsync(botClient, chatId, "Пожалуйста, введите вашу дату рождения (гггг-мм-дд):");
    }

    /// <summary>
    /// Обрабатывает ввод даты рождения пользователя.
    /// </summary>
    static private async Task ProcessBirthDateStep(ITelegramBotClient botClient, long chatId, string input, UserRegistration user)
    {
      if (DateOnly.TryParse(input, out var birthDate))
      {
        user.BirthDate = birthDate;
        _userSteps[chatId] = RegistrationStep.Email;
        LogInformation($"Дата рождения пользователя {chatId} установлена: {input}");
        await TelegramBotHandler.SendMessageAsync(botClient, chatId, "Пожалуйста, введите вашу электронную почту:");
      }
      else
      {
        LogWarning($"Некорректная дата рождения от пользователя {chatId}: {input}");
        await TelegramBotHandler.SendMessageAsync(botClient, chatId, "Некорректная дата. Пожалуйста, введите дату рождения в формате гггг-мм-дд:");
      }
    }

    static private async Task ProcessEmailStep(ITelegramBotClient botClient, long chatId, string input, UserRegistration user)
    {
      if (string.IsNullOrWhiteSpace(input) || !input.Contains("@"))
      {
        await TelegramBotHandler.SendMessageAsync(botClient, chatId, "Некорректный адрес электронной почты. Пожалуйста, введите вашу электронную почту:");
        return;
      }

      user.Email = input;
      _userSteps[chatId] = RegistrationStep.CourseSelection;
      LogInformation($"Почта пользователя {chatId} установлена: {input}");
      await ProcessCourseSelection(botClient, chatId, user);
    }

    /// <summary>
    /// Обрабатывает выбор курса пользователем.
    /// </summary>
    static private async Task ProcessCourseSelection(ITelegramBotClient botClient, long chatId, UserRegistration user)
    {
      var courses = CourseService.GetAllCourses();
      List<CallbackModel> callbackModels = new List<CallbackModel>();

      foreach (var course in courses)
      {
        string command = $"/selectcourse_{course.Id}";
        callbackModels.Add(new CallbackModel(course.Name, command));
      }

      await TelegramBotHandler.SendMessageAsync(botClient, chatId, "Пожалуйста, выберите курс:", TelegramBotHandler.GetInlineKeyboardMarkupAsync(callbackModels));
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
        UserRegistrationService.AddUserRegistration(user);
        LogInformation($"Регистрация завершена для пользователя с ChatId {chatId}");
        await TelegramBotHandler.SendMessageAsync(botClient, chatId, "Регистрация завершена. Спасибо!");
        _registrationData.Remove(chatId);
        _userSteps.Remove(chatId);
      }
    }
  }
}