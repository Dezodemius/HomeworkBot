using HomeWorkTelegramBot.Models;
using System.Collections.Generic;
using static HomeWorkTelegramBot.Config.Logger;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using HomeWorkTelegramBot.Core;

namespace HomeWorkTelegramBot.Bot.Function.Administrator
{
  internal class CreateCourse
  {
    static private readonly Dictionary<long, Courses> _courseData = new Dictionary<long, Courses>();
    static private readonly Dictionary<long, CourseCreationStep> _courseSteps = new Dictionary<long, CourseCreationStep>();

    private enum CourseCreationStep
    {
      Name,
      Description,
      TeacherSelection,
      Completed
    }

    /// <summary>
    /// Обрабатывает шаг создания курса для администратора.
    /// </summary>
    /// <param name="botClient">Клиент Telegram Bot.</param>
    /// <param name="callback">Callback-запрос от администратора.</param>
    public async Task ProcessCourseCreationStep(ITelegramBotClient botClient, CallbackQuery callback)
    {
      long chatId = callback.From.Id;
      string input = callback.Message.Text;
      await ProcessCourseStep(botClient, chatId, input);
    }

    /// <summary>
    /// Обрабатывает шаг создания курса для администратора.
    /// </summary>
    /// <param name="botClient">Клиент Telegram Bot.</param>
    /// <param name="message">Сообщение от администратора.</param>
    public async Task ProcessCourseCreationStep(ITelegramBotClient botClient, Message message)
    {
      long chatId = message.Chat.Id;
      string input = message.Text;
      await ProcessCourseStep(botClient, chatId, input);
    }

    /// <summary>
    /// Общий метод для обработки шагов создания курса.
    /// </summary>
    /// <param name="botClient">Клиент Telegram Bot.</param>
    /// <param name="chatId">Идентификатор чата администратора.</param>
    /// <param name="input">Входные данные от администратора.</param>
    private async Task ProcessCourseStep(ITelegramBotClient botClient, long chatId, string input)
    {
      if (!_courseSteps.ContainsKey(chatId))
      {
        _courseSteps[chatId] = CourseCreationStep.Name;
        _courseData[chatId] = new Courses();
        Mode.CreateCourse = true;
        LogInformation($"Начало создания курса для администратора с ChatId {chatId}");
        await TelegramBotHandler.SendMessageAsync(botClient, chatId, "Создание курса начато. Пожалуйста, введите название курса:");
        return;
      }

      var currentStep = _courseSteps[chatId];
      var course = _courseData[chatId];

      switch (currentStep)
      {
        case CourseCreationStep.Name:
          course.Name = input;
          _courseSteps[chatId] = CourseCreationStep.Description;
          LogInformation($"Название курса установлено: {input}");
          await TelegramBotHandler.SendMessageAsync(botClient, chatId, "Введите описание курса:");
          break;

        case CourseCreationStep.Description:
          course.Description = input;
          _courseSteps[chatId] = CourseCreationStep.TeacherSelection;
          LogInformation($"Описание курса установлено: {input}");
          await ProcessTeacherSelection(botClient, chatId);
          break;

        case CourseCreationStep.Completed:
          LogInformation($"Создание курса для администратора с ChatId {chatId} уже завершено.");
          break;
      }
    }

    /// <summary>
    /// Обрабатывает выбор преподавателя для курса.
    /// </summary>
    /// <param name="botClient">Клиент Telegram Bot.</param>
    /// <param name="chatId">Идентификатор чата администратора.</param>
    private async Task ProcessTeacherSelection(ITelegramBotClient botClient, long chatId)
    {
      var teachers = UserService.GetAllTeachers();
      List<CallbackModel> callbackModels = new List<CallbackModel>();

      foreach (var teacher in teachers)
      {
        string command = $"/selectteacher_{teacher.Id}";
        callbackModels.Add(new CallbackModel(teacher.Name, command));
      }

      await TelegramBotHandler.SendMessageAsync(botClient, chatId, "Выберите преподавателя для курса:", TelegramBotHandler.GetInlineKeyboardMarkupAsync(callbackModels));
    }

    /// <summary>
    /// Обрабатывает нажатие на выбранного преподавателя.
    /// </summary>
    public async Task HandleTeacherSelection(ITelegramBotClient botClient, CallbackQuery callbackQuery)
    {
      long chatId = callbackQuery.From.Id;
      string data = callbackQuery.Data;

      if (data.StartsWith("/selectteacher_"))
      {
        int teacherId = int.Parse(data.Replace("/selectteacher_", ""));
        var course = _courseData[chatId];
        course.TeacherId = teacherId;
        _courseSteps[chatId] = CourseCreationStep.Completed;
        LogInformation($"Преподаватель {teacherId} выбран для курса.");
        await CompleteCourseCreation(botClient, callbackQuery, chatId);
      }
      else
      {
        LogWarning($"Некорректные данные callback: {data} для пользователя с ChatId {chatId}");
        await TelegramBotHandler.SendMessageAsync(botClient, chatId, "Произошла ошибка при выборе преподавателя. Пожалуйста, попробуйте позже.", null, callbackQuery.Message.Id);
        Mode.CreateCourse = false;
      }
    }

    /// <summary>
    /// Завершает создание курса.
    /// </summary>
    private async Task CompleteCourseCreation(ITelegramBotClient botClient, CallbackQuery callbackQuery, long chatId)
    {
      if (_courseData.TryGetValue(chatId, out var course))
      {
        CourseService.AddCourse(course);
        LogInformation($"Курс создан для администратора с ChatId {chatId}");
        _courseData.Remove(chatId);
        _courseSteps.Remove(chatId);
        await TelegramBotHandler.SendMessageAsync(botClient, chatId, "Курс успешно создан.", null, callbackQuery.Message.Id);
      }
      else
      {
        LogWarning($"Не удалось завершить создание курса: данные курса не найдены для ChatId {chatId}");
        await TelegramBotHandler.SendMessageAsync(botClient, chatId, "Произошла ошибка при завершении создания курса. Пожалуйста, попробуйте позже.");
      }

      Mode.CreateCourse = false;
    }
  }
}