using HomeWorkTelegramBot.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types;
using Telegram.Bot;
using HomeWorkTelegramBot.Models;
using System.Text.RegularExpressions;

namespace HomeWorkTelegramBot.Bot.Function.Administrator
{
  /// <summary>
  /// Класс для управления процессом создания курса.
  /// </summary>
  internal class CreateCourse
  {
    /// <summary>
    /// Словарь для хранения данных о курсе, который создается.
    /// </summary>
    private static readonly Dictionary<long, Courses> CourseCreationData = new Dictionary<long, Courses>();

    /// <summary>
    /// Словарь для отслеживания текущего шага создания курса для каждого пользователя.
    /// </summary>
    private static readonly Dictionary<long, CourseCreationStep> UserSteps = new Dictionary<long, CourseCreationStep>();

    /// <summary>
    /// Перечисление для определения текущего шага создания курса.
    /// </summary>
    private enum CourseCreationStep
    {
      Name,
      Description,
      TeacherSelection,
      Completed,
    }

    /// <summary>
    /// Обрабатывает шаги создания курса на основе текстового сообщения.
    /// </summary>
    /// <param name="botClient">Клиент Telegram бота.</param>
    /// <param name="message">Сообщение от пользователя.</param>
    public async Task ProcessCourseCreationStep(ITelegramBotClient botClient, Message message)
    {
      var chatId = message.Chat.Id;
      var input = message.Text;

      var currentStep = UserSteps[chatId];
      var courseData = CourseCreationData[chatId];

      switch (currentStep)
      {
        case CourseCreationStep.Name:
          await ProcessNameStep(botClient, chatId, input, courseData);
          break;

        case CourseCreationStep.Description:
          await ProcessDescriptionStep(botClient, chatId, input, courseData);
          break;

        case CourseCreationStep.TeacherSelection:
          await ShowTeacherSelectionAsync(botClient, chatId);
          break;
      }
    }

    /// <summary>
    /// Обрабатывает шаги создания курса на основе callback-запроса.
    /// </summary>
    /// <param name="botClient">Клиент Telegram бота.</param>
    /// <param name="callbackQuery">Callback-запрос от пользователя.</param>
    public async Task ProcessCourseCreationStep(ITelegramBotClient botClient, CallbackQuery callbackQuery)
    {
      var chatId = callbackQuery.Message.Chat.Id;
      var data = callbackQuery.Data;

      if (!UserSteps.ContainsKey(chatId))
      {
        await StartCourseCreationAsync(botClient, chatId, callbackQuery.Message.Id);
        return;
      }

      if (UserSteps[chatId] == CourseCreationStep.TeacherSelection && data.StartsWith("/createCourse_select_teacher:"))
      {
        await HandleCallbackQueryAsync(botClient, callbackQuery, callbackQuery.Message.Id);
      }
    }

    /// <summary>
    /// Инициализирует процесс создания курса.
    /// </summary>
    /// <param name="botClient">Клиент Telegram бота.</param>
    /// <param name="chatId">Идентификатор чата пользователя.</param>
    /// <param name="messageId">Идентификатор сообщения (опционально).</param>
    private async Task StartCourseCreationAsync(ITelegramBotClient botClient, long chatId, int? messageId = null)
    {
      CourseCreationData[chatId] = new Courses();
      UserSteps[chatId] = CourseCreationStep.Name;
      await TelegramBotHandler.SendMessageAsync(botClient, chatId, "Введите название курса:", null, messageId);
    }

    /// <summary>
    /// Обрабатывает ввод названия курса.
    /// </summary>
    /// <param name="botClient">Клиент Telegram бота.</param>
    /// <param name="chatId">Идентификатор чата пользователя.</param>
    /// <param name="input">Введенное пользователем название курса.</param>
    /// <param name="courseData">Данные о курсе.</param>
    private async Task ProcessNameStep(ITelegramBotClient botClient, long chatId, string input, Courses courseData)
    {
      if (string.IsNullOrWhiteSpace(input) || !Regex.IsMatch(input, @"^[a-zA-Zа-яА-Я]+$"))
      {
        await TelegramBotHandler.SendMessageAsync(botClient, chatId, "Название курса должно содержать только буквы. Пожалуйста, введите название курса:");
        return;
      }

      courseData.Name = input;
      UserSteps[chatId] = CourseCreationStep.Description;
      await TelegramBotHandler.SendMessageAsync(botClient, chatId, "Введите описание курса:");
    }

    /// <summary>
    /// Обрабатывает ввод описания курса.
    /// </summary>
    /// <param name="botClient">Клиент Telegram бота.</param>
    /// <param name="chatId">Идентификатор чата пользователя.</param>
    /// <param name="input">Введенное пользователем описание курса.</param>
    /// <param name="courseData">Данные о курсе.</param>
    private async Task ProcessDescriptionStep(ITelegramBotClient botClient, long chatId, string input, Courses courseData)
    {
      if (string.IsNullOrWhiteSpace(input))
      {
        await TelegramBotHandler.SendMessageAsync(botClient, chatId, "Описание курса не может быть пустым. Пожалуйста, введите описание курса:");
        return;
      }

      courseData.Description = input;
      UserSteps[chatId] = CourseCreationStep.TeacherSelection;
      await ShowTeacherSelectionAsync(botClient, chatId);
    }

    /// <summary>
    /// Отображает кнопки для выбора преподавателя.
    /// </summary>
    /// <param name="botClient">Клиент Telegram бота.</param>
    /// <param name="chatId">Идентификатор чата пользователя.</param>
    private async Task ShowTeacherSelectionAsync(ITelegramBotClient botClient, long chatId)
    {
      var teachers = UserService.GetAllUsers().Where(u => u.UserRole == Models.User.Role.Teacher).ToList();
      var buttons = teachers.Select(teacher => new List<InlineKeyboardButton>
      { InlineKeyboardButton.WithCallbackData($"{teacher.Name} {teacher.Surname}", $"/createCourse_select_teacher:{teacher.ChatId}")}).ToList();

      var inlineKeyboard = new InlineKeyboardMarkup(buttons);
      await TelegramBotHandler.SendMessageAsync(botClient, chatId, "Выберите преподавателя для курса:", inlineKeyboard);
    }

    /// <summary>
    /// Обрабатывает выбор преподавателя и завершает создание курса.
    /// </summary>
    /// <param name="botClient">Клиент Telegram бота.</param>
    /// <param name="callbackQuery">Callback-запрос от пользователя.</param>
    /// <param name="messageId">Идентификатор сообщения (опционально).</param>
    public async Task HandleCallbackQueryAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery, int? messageId = null)
    {
      var data = callbackQuery.Data;
      var chatId = callbackQuery.Message.Chat.Id;

      if (data.StartsWith("/createCourse_select_teacher:") && CourseCreationData.TryGetValue(chatId, out var courseData))
      {
        var teacherChatId = long.Parse(data.Split(':')[1]);
        courseData.TeacherId = teacherChatId;
        CourseService.AddCourse(courseData);

        await TelegramBotHandler.SendMessageAsync(botClient, chatId, "Курс успешно создан!", null, messageId);
        await TelegramBotHandler.SendMessageAsync(botClient, courseData.TeacherId, $"Вас добавили преподавателем на курс: {courseData.Name}");

        AdministratorHandler.AdminActions[chatId] = AdministratorHandler.AdminAction.None;
        CourseCreationData.Remove(chatId);
        UserSteps.Remove(chatId);
      }
    }
  }
}