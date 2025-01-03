using Telegram.Bot;
using Telegram.Bot.Types;
using HomeWorkTelegramBot.Models;
using HomeWorkTelegramBot.Core;
using System.Text;
using static HomeWorkTelegramBot.Config.Logger;

namespace HomeWorkTelegramBot.Bot.Function.Teacher
{
  public static class TaskWorkData
  {
    private static readonly Dictionary<long, TaskWork> _taskData = new Dictionary<long, TaskWork>();
    private static readonly Dictionary<long, GetTaskStep> _userSteps = new Dictionary<long, GetTaskStep>();

    /// <summary>
    /// Этапы создания задания.
    /// </summary>
    private enum GetTaskStep
    {
      /// <summary>
      /// Выбор курса.
      /// </summary>
      CourseSelection,

      /// <summary>
      /// Выбор задания.
      /// </summary>
      TaskSelection,

      /// <summary>
      /// Выбор ответа на задание.
      /// </summary>
      AnswerSelection,

      /// <summary>
      /// Обновление статуса ответа на задание.
      /// </summary>
      UpdateAnswerStatus,

      /// <summary>
      /// Процесс выбора завершен.
      /// </summary>
      Completed,
    }

    /// <summary>
    /// Процесс выбора задания курса для просмотра статистики их выполнения студентами.
    /// </summary>
    /// <param name="botClient">Клиент telegram-бота.</param>
    /// <param name="callbackQuery">Callback-запрос, полученный от пользователя.</param>
    /// <returns>Асинхронная задача, представляющая процесс обработки.</returns>
    public static async Task ProcessGetTasks(ITelegramBotClient botClient, CallbackQuery callbackQuery)
    {
      long chatId = callbackQuery.From.Id;
      string data = callbackQuery.Data;

      if (!_userSteps.ContainsKey(chatId))
      {
        await InitializeGetTasks(botClient, chatId, callbackQuery);
        return;
      }

      var currentStep = _userSteps[chatId];
      var task = _taskData[chatId];

      switch (currentStep)
      {
        case GetTaskStep.CourseSelection:
          await HandleCourseSelection(botClient, chatId, callbackQuery, task);
          break;

        case GetTaskStep.TaskSelection:
          await HandleTaskSelection(botClient, chatId, callbackQuery, task);
          break;

        case GetTaskStep.AnswerSelection:
          await HandleAnswerSelection(botClient, callbackQuery, currentStep, task);
          break;

        case GetTaskStep.UpdateAnswerStatus:
          await HandleUpdateAnswerStatus(botClient, callbackQuery, currentStep, task);
          break;

        case GetTaskStep.Completed:
          await CompleteStudentCheck(botClient, chatId, task.Id, callbackQuery.Message.MessageId);
          break;
      }
    }

    private static async Task HandleAnswerSelection(ITelegramBotClient botClient, CallbackQuery callbackQuery, GetTaskStep currentStep, TaskWork task)
    {
      await RateTaskWork.ProcessUpdateAnswer(botClient, callbackQuery, task.Id);
      currentStep = GetTaskStep.UpdateAnswerStatus;
    }

    private static async Task HandleUpdateAnswerStatus(ITelegramBotClient botClient, CallbackQuery callbackQuery, GetTaskStep currentStep, TaskWork task)
    {
      await RateTaskWork.ProcessUpdateAnswer(botClient, callbackQuery, task.Id);
      currentStep = GetTaskStep.Completed;
      await CompleteStudentCheck(botClient, callbackQuery.From.Id, task.Id, callbackQuery.Message.MessageId);

    }

    /// <summary>
    /// Обрабатывает нажатие на выбранный курс.
    /// </summary>
    /// <param name="botClient">Клиент telegram-бота.</param>
    /// <param name="chatId">Уникальный идентификатор чата пользователя.</param>
    /// <param name="callbackQuery">Callback-запрос, полученный от пользователя.</param>
    /// <param name="task">Объект класса Answer.</param>
    /// <returns>Асинхронная задача, представляющая процесс обработки.</returns>
    private static async Task HandleCourseSelection(ITelegramBotClient botClient, long chatId, CallbackQuery callbackQuery, TaskWork task)
    {
      string data = callbackQuery.Data;
      if (data.StartsWith("/selectcourse_"))
      {
        int messageId = callbackQuery.Message.MessageId;
        int courseId = int.Parse(data.Replace("/selectcourse_tw_", string.Empty));
        task.CourseId = courseId;
        _userSteps[chatId] = GetTaskStep.TaskSelection;
        var courseTasks = TaskWorkService.GetTaskWorksByCourseId(courseId);
        var keyboard = GetInlineKeyboard.GetTaskKeyboard(courseTasks);
        LogInformation($"Курс {courseId} выбран для просмотра статистики выполнения заданий студентами преподавателем с ChatId {chatId}");
        if (courseTasks.Count > 0)
        {
          await TelegramBotHandler.SendMessageAsync(botClient, chatId,
            $"Был выбран курс: {courseId}. Пожалуйста, выберите задание:", keyboard, messageId);
        }
        else
        {
          await TelegramBotHandler.SendMessageAsync(botClient, chatId,
            $"Не найдено заданий для курса с id {courseId}.", null, messageId);
        }
      }
    }

    /// <summary>
    /// Обрабатывает нажатие на выбранное задание.
    /// </summary>
    /// <param name="botClient">Экземпляр клиента Telegram бота.</param>
    /// <param name="chatId">Уникальный идентификатор чата пользователя.</param>
    /// <param name="callbackQuery">Callback-запрос, полученный от пользователя.</param>
    /// <param name="task">Объект класса TaskWork.</param>
    /// <returns>Асинхронная задача, представляющая процесс обработки.</returns>
    private static async Task HandleTaskSelection(ITelegramBotClient botClient, long chatId, CallbackQuery callbackQuery, TaskWork task)
    {
      string data = callbackQuery.Data;
      if (data.StartsWith("/selecttask_"))
      {
        int messageId = callbackQuery.Message.MessageId;
        int taskId = int.Parse(data.Replace("/selecttask_", string.Empty));
        task.Id = taskId;
        _userSteps[chatId] = GetTaskStep.AnswerSelection;
        string messageData = GetMessageData(chatId, task);
        var users = GetUsersByChatId(task);
        var keyboard = GetInlineKeyboard.GetStudentsKeyboard(users, "useransw");
        // TODO: добавить кнопку на главную
        LogInformation($"Студент с chatId {taskId} выбран для просмотра статистики выполнения заданий студента преподавателем с ChatId {chatId}");

        await TelegramBotHandler.SendMessageAsync(botClient, chatId, messageData, keyboard, messageId);
        _taskData.Remove(chatId);
        _userSteps.Remove(chatId);
      }
    }

    /// <summary>
    /// Получает список студентов, отправивших ответ на задание и не получивших оценку от преподавателя.
    /// </summary>
    /// <param name="task">Объект класса TaskWork.</param>
    /// <returns>Список пользователей, если они были найдены, иначе - null.</returns>
    private static List<Models.User> GetUsersByChatId(TaskWork task)
    {
      var answers = AnswerService.GetAnswersByTaskId(task.Id);
      answers = answers
        .Where(a => a.Status == Answer.TaskStatus.Answered)
        .ToList();
      if (answers != null)
      {
        var users = new List<Models.User>();
        foreach(var answer in answers)
        {
          var user = UserService.GetUserByChatId(answer.UserId);
          if (user != null)
          {
            users.Add(user);
          }
        }

        return users;
      }

      return null;
    }

    /// <summary>
    /// Завершает создание нового задания.
    /// </summary>
    /// <param name="botClient">Экземпляр клиента Telegram бота.</param>
    /// <param name="chatId">Уникальный идентификатор чата пользователя.</param>
    /// <returns>Асинхронная задача, представляющая процесс обработки.</returns>
    private static async Task CompleteStudentCheck(ITelegramBotClient botClient, long chatId, int taskId, int messageId)
    {
      if (_taskData.TryGetValue(chatId, out var task))
      {
        LogInformation($"Получена статистика выполнения задания {taskId} студентами преподавателем с ChatId {chatId}");
        var messageData = $"Данные об ответе на задание с id {task.Id} изменены";
        var callbackModels = new CallbackModel("На главную", "/start");
        var keyboard = TelegramBotHandler.GetInlineKeyboardMarkupAsync(callbackModels);
        await TelegramBotHandler.SendMessageAsync(botClient, chatId, messageData, keyboard, messageId);
        _taskData.Remove(chatId);
        _userSteps.Remove(chatId);
      }
    }

    /// <summary>
    /// Подготавливает данные по статистике выполнений заданий студентом курса.
    /// </summary>
    /// <param name="chatId">Идентификатор чата студента.</param>
    /// <param name="task">Объект класса TaskWork.</param>
    /// <returns>Строку, с данными о выполнении заданий студентом.</returns>
    private static string GetMessageData(long chatId, TaskWork task)
    {
      var allStudentAnswers = AnswerService.GetAnswersByTaskId(task.Id);
      var taskName = TaskWorkService.GetTaskWorkById(task.Id).Name;
      var studentsData = GetTaskStudents(allStudentAnswers);
      if (studentsData.Count > 0)
      {
        var sb = new StringBuilder();
        var status = string.Empty;
        sb.AppendLine($"Статистика выполнения задания \"{taskName}\"\n");
        foreach (var answerData in studentsData)
        {
          status = allStudentAnswers
            .FirstOrDefault(a => a.UserId == answerData.Value.ChatId)
            .Status
            .ToString();
          sb.AppendLine($"Студент: {answerData.Value.Surname} {answerData.Value.Name}.\nСтатус: {status}");
        }

        return sb.ToString();
      }

      return $"Не найдены студенты, записанные на этот курс";
    }

    /// <summary>
    /// Получает данные о студенте по идентификатору его чата и записывает их в словарь.
    /// </summary>
    /// <param name="answers">Список ответов студентов на задание.</param>
    /// <returns>Словарь с данными о студентах.</returns>
    private static Dictionary<long, Models.User> GetTaskStudents(List<Answer> answers)
    {
      var students = new Dictionary<long, Models.User>();
      foreach (var answer in answers)
      {
        var foundStudent = UserService.GetUserByChatId(answer.UserId);
        if (foundStudent != null)
        {
          if (!students.ContainsKey(foundStudent.ChatId))
          {
            students[foundStudent.ChatId] = foundStudent;
          }
        }
      }

      return students;
    }

    /// <summary>
    /// Инициализирует процесс получения статистики выполнения заданий студентом.
    /// </summary>
    /// <param name="botClient">Экземпляр клиента Telegram бота.</param>
    /// <param name="chatId">Уникальный идентификатор чата пользователя.</param>
    /// <returns>Асинхронная задача, представляющая процесс обработки.</returns>
    private static async Task InitializeGetTasks(ITelegramBotClient botClient, long chatId, CallbackQuery callbackQuery)
    {
      int messageId = callbackQuery.Message.MessageId;
      _userSteps[chatId] = GetTaskStep.CourseSelection;
      _taskData[chatId] = new TaskWork();
      LogInformation($"Начало получения статистики выполнения задания преподавателем с ChatId {chatId}");
      var courses = CourseService.GetAllCoursesByTeacherId(chatId);
      var keyboard = GetInlineKeyboard.GetCoursesKeyboard(courses, "selectcourse_tw");
      await TelegramBotHandler.SendMessageAsync(botClient, chatId, "Для получения статистики выполнения задания, пожалуйста, выберите курс:", keyboard, messageId);
    }
  }
}