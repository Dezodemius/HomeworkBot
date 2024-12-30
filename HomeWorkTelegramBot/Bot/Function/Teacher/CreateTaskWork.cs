using HomeWorkTelegramBot.Core;
using HomeWorkTelegramBot.Models;
using Telegram.Bot;
using Telegram.Bot.Types;
using static HomeWorkTelegramBot.Config.Logger;

namespace HomeWorkTelegramBot.Bot.Function.Teacher
{
  internal class CreateTaskWork
  {
    public static readonly Dictionary<long, TaskWork> _creationData = new Dictionary<long, TaskWork>();
    private static readonly Dictionary<long, CreationStep> _userSteps = new Dictionary<long, CreationStep>();

    /// <summary>
    /// Этапы создания задания.
    /// </summary>
    private enum CreationStep
    {
      /// <summary>
      /// Выбор курса.
      /// </summary>
      CourseSelection,

      /// <summary>
      /// Этап задания названия задания.
      /// </summary>
      Name,

      /// <summary>
      /// Этап задания описания задания.
      /// </summary>
      Description,

      /// <summary>
      /// Завершено создание задания.
      /// </summary>
      Completed,
    }

    /// <summary>
    /// Обрабатывает шаги создания нового задания на основе callback-запроса от пользователя.
    /// </summary>
    /// <param name="botClient">Экземпляр клиента Telegram бота.</param>
    /// <param name="callbackQuery">Callback-запрос, полученный от пользователя.</param>
    /// <returns>Асинхронная задача, представляющая процесс обработки.</returns>
    public static async Task ProcessCreationStep(ITelegramBotClient botClient, CallbackQuery callbackQuery)
    {
      long chatId = callbackQuery.From.Id;
      string data = callbackQuery.Data;

      if (!_userSteps.ContainsKey(chatId))
      {
        await InitializeCreation(botClient, chatId);
        return;
      }

      var currentStep = _userSteps[chatId];
      var task = _creationData[chatId];

      switch (currentStep)
      {
        case CreationStep.CourseSelection:
          await HandleCourseSelection(botClient, chatId, callbackQuery, task);
          break;

        case CreationStep.Completed:
          LogInformation($"Создание нового задания \"{task.Name}\" уже завершено.");
          break;
      }
    }

    /// <summary>
    /// Обрабатывает шаги создания нового задания на основе callback-запроса от пользователя.
    /// </summary>
    /// <param name="botClient">Экземпляр клиента Telegram бота.</param>
    /// <param name="message">Сообщение, полученное от пользователя.</param>
    /// <returns>Асинхронная задача, представляющая процесс обработки.</returns>
    public static async Task ProcessCreationStep(ITelegramBotClient botClient, Message message)
    {
      long chatId = message.Chat.Id;
      string input = message.Text;

      if (!_userSteps.ContainsKey(chatId))
      {
        await InitializeCreation(botClient, chatId);
        return;
      }

      var currentStep = _userSteps[chatId];
      var task = _creationData[chatId];

      switch (currentStep)
      {
        case CreationStep.Name:
          await ProcessNameStep(botClient, chatId, input, task);
          break;

        case CreationStep.Description:
          await ProcessDescriptionStep(botClient, chatId, input, task);
          break;

        case CreationStep.Completed:
          LogInformation($"Создание нового задания \"{task.Name}\" уже завершено.");
          break;
      }
    }

    /// <summary>
    /// Процесс задания названия задачи.
    /// </summary>
    /// <param name="botClient">Экземпляр клиента Telegram бота.</param>
    /// <param name="chatId">Уникальный идентификатор чата пользователя.</param>
    /// <param name="input">Текст входящего сообщения от пользователя.</param>
    /// <param name="task">Объект класса TaskWork.</param>
    /// <returns>Асинхронная задача, представляющая процесс обработки.</returns>
    private static async Task ProcessNameStep(ITelegramBotClient botClient, long chatId, string? input, TaskWork task)
    {
      if (string.IsNullOrWhiteSpace(input) || input.Contains("/"))
      {
        await TelegramBotHandler.SendMessageAsync(botClient, chatId, "Название не должно содержать \"/\". Пожалуйста, введите название курса:");
        return;
      }

      task.Name = input;
      _userSteps[chatId] = CreationStep.Description;
      LogInformation($"Название нового задания для курса {task.CourseId}, создаваемого преподавателем с ChatId {chatId} установлено: {input}");
      await TelegramBotHandler.SendMessageAsync(botClient, chatId, "Пожалуйста, введите описание задания:");
    }


    /// <summary>
    /// Завершает создание нового задания.
    /// </summary>
    /// <param name="botClient">Экземпляр клиента Telegram бота.</param>
    /// <param name="chatId">Уникальный идентификатор чата пользователя.</param>
    /// <returns>Асинхронная задача, представляющая процесс обработки.</returns>
    private static async Task CompleteCreation(ITelegramBotClient botClient, long chatId)
    {
      if (_creationData.TryGetValue(chatId, out var task))
      {
        TaskWorkService.AddTaskWork(task);
        LogInformation($"Создание нового задания с названием {task.Name} курса {task.CourseId} завершено преподавателем с ChatId {chatId}");
        var callbackModels = new CallbackModel("На главную", "/start");
        var keyboard = TelegramBotHandler.GetInlineKeyboardMarkupAsync(callbackModels);
        await TelegramBotHandler.SendMessageAsync(botClient, chatId, "Создание нового задания завершено!", keyboard);
        _creationData.Remove(chatId);
        _userSteps.Remove(chatId);
      }
    }

    /// <summary>
    /// Процесс задания описания задания.
    /// </summary>
    /// <param name="botClient">Экземпляр клиента Telegram бота.</param>
    /// <param name="chatId">Уникальный идентификатор чата пользователя.</param>
    /// <param name="input">Входящее сообщение от пользователя.</param>
    /// <param name="task">Экземпляр класса TaskWork.</param>
    /// <returns>Асинхронная задача, представляющая процесс обработки.</returns>
    private static async Task ProcessDescriptionStep(ITelegramBotClient botClient, long chatId, string? input, TaskWork task)
    {
      if (string.IsNullOrWhiteSpace(input) || input.Contains("/"))
      {
        await TelegramBotHandler.SendMessageAsync(botClient, chatId, "Название не должно содержать \"/\". Пожалуйста, введите название курса:");
        return;
      }

      task.Description = input;
      _userSteps[chatId] = CreationStep.Completed;
      LogInformation($"Описание нового задания для курса {task.CourseId}, создаваемого преподавателем с ChatId {chatId} установлено: {input}");
      await CompleteCreation(botClient, chatId);
    }

    /// <summary>
    /// Обрабатывает нажатие на выбранный курс.
    /// </summary>
    /// <param name="botClient">Клиент telegram-бота.</param>
    /// <param name="chatId">Уникальный идентификатор чата пользователя.</param>
    /// <param name="callbackQuery">Callback-запрос, полученный от пользователя.</param>
    /// <param name="task">Объект класса TaskWork.</param>
    /// <returns>Асинхронная задача, представляющая процесс обработки.</returns>
    private static async Task HandleCourseSelection(ITelegramBotClient botClient, long chatId, CallbackQuery callbackQuery, TaskWork task)
    {
      string data = callbackQuery.Data;
      if (data.StartsWith("/selectcourse_"))
      {
        int courseId = int.Parse(data.Replace("/selectcourse_", string.Empty));
        task.CourseId = courseId;
        _userSteps[chatId] = CreationStep.Name;
        LogInformation($"Курс {courseId} выбран для нового задания, которое создает преподаватель с ChatId {chatId}");
        await TelegramBotHandler.SendMessageAsync(botClient, chatId, $"Был выбран курс: {courseId}. " +
          $"Пожалуйста, введите название для нового задания.", null, callbackQuery.Message.Id);
      }
    }

    /// <summary>
    /// Инициализирует процесс добавления нового задания.
    /// </summary>
    /// <param name="botClient">Экземпляр клиента Telegram бота.</param>
    /// <param name="chatId">Уникальный идентификатор чата пользователя.</param>
    /// <returns>Асинхронная задача, представляющая процесс обработки.</returns>
    private static async Task InitializeCreation(ITelegramBotClient botClient, long chatId)
    {
      _userSteps[chatId] = CreationStep.CourseSelection;
      _creationData[chatId] = new TaskWork();
      LogInformation($"Начало создания нового задания преподавателем с ChatId {chatId}");
      var courses = CourseService.GetAllCoursesByTeacherId(chatId);
      var keyboard = GetInlineKeyboard.GetCoursesKeyboard(courses);
      await TelegramBotHandler.SendMessageAsync(botClient, chatId, "Пожалуйста, выберите курс:", keyboard);
    }
  }
}
