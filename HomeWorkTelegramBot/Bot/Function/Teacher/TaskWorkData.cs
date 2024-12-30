using Telegram.Bot;
using Telegram.Bot.Types;
using HomeWorkTelegramBot.Models;
using HomeWorkTelegramBot.Core;
using HomeWorkTelegramBot.DataBase;
using System.Text;
using static HomeWorkTelegramBot.Config.Logger;

namespace HomeWorkTelegramBot.Bot.Function.Teacher
{
  public static class TaskWorkData
  {
    private static readonly Dictionary<long, TaskWork> _taskData = new Dictionary<long, TaskWork>();
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
      /// Выбор задания.
      /// </summary>
      TaskSelection,

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
        await InitializeGetTasks(botClient, chatId);
        return;
      }

      var currentStep = _userSteps[chatId];
      var answer = _taskData[chatId];

      switch (currentStep)
      {
        case CreationStep.CourseSelection:
          await HandleCourseSelection(botClient, chatId, callbackQuery, answer);
          break;

        case CreationStep.TaskSelection:
          await HandleTaskSelection(botClient, chatId, callbackQuery, answer);
          break;
      }
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
        int courseId = int.Parse(data.Replace("/selectcourse_", string.Empty));
        task.CourseId = courseId;
        _userSteps[chatId] = CreationStep.TaskSelection;
        var courseTasks = TaskWorkService.GetTaskWorksByCourseId(courseId);
        var keyboard = GetInlineKeyboard.GetTaskKeyboard(courseTasks);
        LogInformation($"Курс {courseId} выбран для просмотра статистики выполнения заданий студентами преподавателем с ChatId {chatId}");
        await TelegramBotHandler.SendMessageAsync(botClient, chatId,
          $"Был выбран курс: {courseId}. Пожалуйста, выберите задание:", keyboard);
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
        int taskId = int.Parse(data.Replace("/selecttask_", string.Empty));
        task.Id = taskId;
        _userSteps[chatId] = CreationStep.Completed;
        string messageData = GetMessageData(chatId, task);

        LogInformation($"Студен с chatId {taskId} выбран для просмотра статистики выполнения заданий студента преподавателем с ChatId {chatId}");
        await TelegramBotHandler.SendMessageAsync(botClient, chatId, messageData);
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
      var studentsData = GetTaskStudents(allStudentAnswers);
      var sb = new StringBuilder();
      var status = string.Empty;
      sb.AppendLine($"Статистика выполнения задания \"{task.Name}\"\n");
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

    /// <summary>
    /// Получает данные о студенте по идентификатору его чата и записывает их в словарь.
    /// </summary>
    /// <param name="answers">Список ответов студентов на задание.</param>
    /// <returns>Словарь с данными о студентах.</returns>
    private static Dictionary<long, Models.User> GetTaskStudents(List<Answer> answers)
    {
      var students = new Dictionary<long, Models.User>();
      var userRepository = new UserRepository();
      foreach (var answer in answers)
      {
        var foundStudent = userRepository.GetUserByChatId(answer.UserId);
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
    private static async Task InitializeGetTasks(ITelegramBotClient botClient, long chatId)
    {
      _userSteps[chatId] = CreationStep.CourseSelection;
      _taskData[chatId] = new TaskWork();
      LogInformation($"Начало получения статистики выполнения задания преподавателем с ChatId {chatId}");
      var courses = CourseService.GetAllCoursesByTeacherId(chatId);
      var keyboard = GetInlineKeyboard.GetCoursesKeyboard(courses);
      await TelegramBotHandler.SendMessageAsync(botClient, chatId, "Пожалуйста, выберите курс:", keyboard);
    }
  }
}