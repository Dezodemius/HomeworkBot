using Telegram.Bot;
using Telegram.Bot.Types;
using HomeWorkTelegramBot.Models;
using HomeWorkTelegramBot.Core;
using System.Text;
using Telegram.Bot.Types.ReplyMarkups;
using static HomeWorkTelegramBot.Config.Logger;

namespace HomeWorkTelegramBot.Bot.Function.Teacher
{
  /// <summary>
  /// Класс для получения статистики по студентам.
  /// </summary>
  public static class StudentsData
  {
    private static readonly Dictionary<long, Answer> _answerData = new Dictionary<long, Answer>();
    private static readonly Dictionary<long, GetStudentDataStep> _userSteps = new Dictionary<long, GetStudentDataStep>();

    /// <summary>
    /// Этапы создания задания.
    /// </summary>
    private enum GetStudentDataStep
    {
      /// <summary>
      /// Выбор курса.
      /// </summary>
      CourseSelection,

      /// <summary>
      /// Выбор студента.
      /// </summary>
      StudentSelection,

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
    /// Процесс выбора студента для просмотра статистики выполнения им заданий курса.
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
        await InitializeGetCourse(botClient, chatId, callbackQuery);
        return;
      }

      if (data == "/start" && _userSteps.ContainsKey(chatId))
      {
        var messageData = $"Обновление данных об ответе на задание с id {_answerData[chatId].Id} прервано";
        await CompleteStudentCheck(botClient, chatId, _answerData[chatId].UserId, callbackQuery.Message.MessageId, messageData);
      }

      var currentStep = _userSteps[chatId];
      var answer = _answerData[chatId];

      switch (currentStep)
      {
        case GetStudentDataStep.CourseSelection:
          await HandleCourseSelection(botClient, chatId, callbackQuery, answer);
          break;

        case GetStudentDataStep.StudentSelection:
          await HandleStudentSelection(botClient, chatId, callbackQuery, answer);
          break;

        case GetStudentDataStep.AnswerSelection:
          await HandleAnswerSelection(botClient, callbackQuery, currentStep);
          break;

        case GetStudentDataStep.UpdateAnswerStatus:
          await HandleUpdateAnswerStatus(botClient, callbackQuery, currentStep, answer.TaskId);
          break;

        case GetStudentDataStep.Completed:
          await CompleteStudentCheck(botClient, chatId, answer.UserId, callbackQuery.Message.MessageId);
          break;
      }
    }

    /// <summary>
    /// Обрабатывает выбор ответа на задание.
    /// </summary>
    /// <param name="botClient">Экземпляр клиента Telegram бота.</param>
    /// <param name="callbackQuery">Callback-запрос, полученный от пользователя.</param>
    /// <param name="currentStep">Текущий шаг получения данных о студенте.</param>
    /// <returns>Асинхронная задача, представляющая процесс обработки.</returns>
    private static async Task HandleAnswerSelection(ITelegramBotClient botClient, CallbackQuery callbackQuery, GetStudentDataStep currentStep)
    {
      if (callbackQuery.Data.StartsWith("/selectanswer_"))
      {
        var answerId = int.Parse(callbackQuery.Data.Replace("/selectanswer_", string.Empty));
        var foundAnswer = AnswerService.GetAnswerById(answerId);
        if (foundAnswer != null)
        {
          await RateTaskWork.ProcessUpdateAnswer(botClient, callbackQuery, foundAnswer.TaskId);
        }
      }

      currentStep = GetStudentDataStep.UpdateAnswerStatus;
    }

    /// <summary>
    /// Обрабатывает процесс обновления статуса ответа на задание.
    /// </summary>
    /// <param name="botClient">Экземпляр клиента Telegram бота.</param>
    /// <param name="callbackQuery">Callback-запрос, полученный от пользователя.</param>
    /// <param name="currentStep">Текущий шаг получения данных о студенте.</param>
    /// <param name="taskId">Уникальный идентификатор задания.</param>
    /// <returns>Асинхронная задача, представляющая процесс обработки.</returns>
    private static async Task HandleUpdateAnswerStatus(ITelegramBotClient botClient, CallbackQuery callbackQuery, GetStudentDataStep currentStep, int taskId)
    {
      await RateTaskWork.ProcessUpdateAnswer(botClient, callbackQuery, taskId);
      currentStep = GetStudentDataStep.Completed;
      await CompleteStudentCheck(botClient, callbackQuery.From.Id, taskId, callbackQuery.Message.MessageId);
    }

    /// <summary>
    /// Обрабатывает нажатие на выбранный курс.
    /// </summary>
    /// <param name="botClient">Клиент telegram-бота.</param>
    /// <param name="chatId">Уникальный идентификатор чата пользователя.</param>
    /// <param name="callbackQuery">Callback-запрос, полученный от пользователя.</param>
    /// <param name="answer">Объект класса Answer.</param>
    /// <returns>Асинхронная задача, представляющая процесс обработки.</returns>
    private static async Task HandleCourseSelection(ITelegramBotClient botClient, long chatId, CallbackQuery callbackQuery, Answer answer)
    {
      string data = callbackQuery.Data;
      if (data.StartsWith("/selectcourse_"))
      {
        int messageId = callbackQuery.Message.MessageId;
        int courseId = int.Parse(data.Replace("/selectcourse_sd_", string.Empty));
        answer.CourseId = courseId;
        _userSteps[chatId] = GetStudentDataStep.StudentSelection;
        List<Models.User> students = GetCourseStudents(courseId);
        var callbacks = GetCallbackSet.GetStudentsCallbacks(students, "selectuser");
        callbacks.Add(new CallbackModel("В главное меню", $"/start"));
        var keyboard = TelegramBotHandler.GetPaginatedInlineKeyboardMarkup(callbacks);
        LogInformation($"Курс {courseId} выбран для просмотра статистики выполнения заданий студента преподавателем с ChatId {chatId}");
        if (students.Count > 0)
        {
          var message = await TelegramBotHandler.SendMessageAsync(botClient, chatId,
            $"Был выбран курс: {courseId}. Пожалуйста, выберите студента:", keyboard, messageId);
          TelegramBotHandler.InitializePagination(callbackQuery.From.Id, message.MessageId, callbacks);
        }
        else
        {
          _answerData.Remove(chatId);
          _userSteps.Remove(chatId);
          keyboard = TelegramBotHandler.GetInlineKeyboardMarkupAsync(new CallbackModel("В главное меню", $"/start"));
          await TelegramBotHandler.SendMessageAsync(botClient, chatId,
          $"Не найдено студентов, записанных на курс с id {courseId}.", keyboard, messageId);
        }
      }
    }

    /// <summary>
    /// Получает список студентов, записанных на курс.
    /// </summary>
    /// <param name="courseId">Уникальный идентификатор курса.</param>
    /// <returns>Список студентов или null, если не было найдено ни одного студента на курсе.</returns>
    private static List<Models.User> GetCourseStudents(int courseId)
    {
      var studentsId = CourseEnrollmentService.GetAllUsersCourseEnrollments(courseId);
      var students = new List<Models.User>();
      foreach (var student in studentsId)
      {
        // TODO: в программе student.UserId - long и должен быть chatId, в бд внешний ключ к user.id 
        if (student.UserId <= int.MaxValue && student.UserId >= int.MinValue)
        {
          int studentId = (int)student.UserId;
          var foundStudent = UserService.GetUserById(studentId); // student.UserId - int user.id
          if (foundStudent != null)
          {
            students.Add(foundStudent);
          }
        }
        else
        {
          throw new OverflowException("Number is too large for int");
        }
      }

      return students;
    }

    /// <summary>
    /// Обрабатывает нажатие на выбранного студента.
    /// </summary>
    /// <param name="botClient">Экземпляр клиента Telegram бота.</param>
    /// <param name="chatId">Уникальный идентификатор чата пользователя.</param>
    /// <param name="callbackQuery">Callback-запрос, полученный от пользователя.</param>
    /// <param name="answer">Объект класса Answer.</param>
    /// <returns>Асинхронная задача, представляющая процесс обработки.</returns>
    private static async Task HandleStudentSelection(ITelegramBotClient botClient, long chatId, CallbackQuery callbackQuery, Answer answer)
    {
      string data = callbackQuery.Data;
      if (data.StartsWith("/selectuser_"))
      {
        var messageId = callbackQuery.Message.MessageId;
        long userId = long.Parse(data.Replace("/selectuser_", string.Empty));
        answer.UserId = userId;
        _userSteps[chatId] = GetStudentDataStep.AnswerSelection;
        string messageData = GetMessageData(answer);
        // var callbacks = GetInlineKeyboard.GetDataButtons();
        var callbacks = new List<CallbackModel>();
        var messageButtons = GetMessageButtons(answer);
        if (messageButtons != null)
        {
          callbacks = messageButtons;
        }
        callbacks.Add(new CallbackModel("В главное меню", $"/start"));
        var keyboard = TelegramBotHandler.GetPaginatedInlineKeyboardMarkup(callbacks);
        if (keyboard != null)
        {
          LogInformation($"Студент с chatId {userId} выбран для просмотра статистики выполнения заданий студента преподавателем с ChatId {chatId}");
          var message = await TelegramBotHandler.SendMessageAsync(botClient, chatId, messageData, keyboard, messageId);
          TelegramBotHandler.InitializePagination(callbackQuery.From.Id, message.MessageId, callbacks);
        }
        else
        {
          _userSteps[chatId] = GetStudentDataStep.Completed;
          CompleteStudentCheck(botClient, chatId, userId, messageId, messageData);
        }
      }
    }

    /// <summary>
    /// Завершает создание нового задания.
    /// </summary>
    /// <param name="botClient">Экземпляр клиента Telegram бота.</param>
    /// <param name="chatId">Уникальный идентификатор чата пользователя.</param>
    /// <returns>Асинхронная задача, представляющая процесс обработки.</returns>
    private static async Task CompleteStudentCheck(ITelegramBotClient botClient, long chatId, long userId, int messageId, string messageData = null)
    {
      if (_answerData.TryGetValue(chatId, out var answer))
      {
        LogInformation($"Получена статистика выполнения заданий студентом {userId} преподавателем с ChatId {chatId}");
        if (messageData == null)
        {
          messageData = $"Данные об ответе на задание с id {answer.TaskId} изменены";
        }

        var callbackModels = new CallbackModel("В главное меню", "/start");
        var keyboard = TelegramBotHandler.GetInlineKeyboardMarkupAsync(callbackModels);
        await TelegramBotHandler.SendMessageAsync(botClient, chatId, messageData, keyboard, messageId);
        await ClearData();
      }
    }

    /// <summary>
    /// Подготавливает данные по статистике выполнений заданий студентом курса.
    /// </summary>
    /// <param name="answer">Объект класс Answer.</param>
    /// <returns>Строку, с данными о выполнении заданий студентом.</returns>
    private static string GetMessageData(Answer answer)
    {
      var allStudentAnswers = AnswerService.GetAnswersByUserId(answer.UserId);
      var studentAnswers = allStudentAnswers
        .Where(a => a.CourseId == answer.CourseId)
        .ToList();
      if (studentAnswers.Count > 0)
      {
        var sb = new StringBuilder();
        foreach (var answerData in studentAnswers)
        {
          var task = TaskWorkService.GetTaskWorkById(answerData.TaskId);
          sb.AppendLine($"Название: {task.Name}\nСтатус: {EnumExtentions.GetDescription(answerData.Status)}\n");
        }

        return sb.ToString();
      }

      return $"Не найдено заданий на проверку, выполненных студентом";
    }

    /// <summary>
    /// Подготавливает данные по статистике выполнений заданий студентом курса.
    /// </summary>
    /// <param name="answer">Объект класс Answer.</param>
    /// <returns>Строку, с данными о выполнении заданий студентом.</returns>
    private static List<CallbackModel> GetMessageButtons(Answer answer)
    {
      var allStudentAnswers = AnswerService.GetAnswersByUserId(answer.UserId);
      var studentAnswers = allStudentAnswers
        .Where(a => a.CourseId == answer.CourseId && a.Status == Answer.TaskStatus.Answered)
        .ToList();
      var sb = new StringBuilder();
      if (studentAnswers.Count > 0)
      {
        return GetCallbackSet.GetAnswersCallbacks(studentAnswers);
      }

      return null;
    }

    /// <summary>
    /// Инициализирует процесс получения статистики выполнения заданий студентом.
    /// </summary>
    /// <param name="botClient">Экземпляр клиента Telegram бота.</param>
    /// <param name="chatId">Уникальный идентификатор чата пользователя.</param>
    /// <returns>Асинхронная задача, представляющая процесс обработки.</returns>
    private static async Task InitializeGetCourse(ITelegramBotClient botClient, long chatId, CallbackQuery callbackQuery)
    {
      int messageId = callbackQuery.Message.MessageId;
      _userSteps[chatId] = GetStudentDataStep.CourseSelection;
      _answerData[chatId] = new Answer();
      LogInformation($"Начало получения статистики выполнения заданий студента преподавателем с ChatId {chatId}");
      var courses = CourseService.GetAllCoursesByTeacherId(chatId);
      var callbacks = GetCallbackSet.GetCoursesCallbacks(courses, "selectcourse_sd");
      callbacks.Add(new CallbackModel("В главное меню", $"/start"));
      var keyboard = TelegramBotHandler.GetPaginatedInlineKeyboardMarkup(callbacks);
      var message = await TelegramBotHandler.SendMessageAsync(botClient, chatId, "Пожалуйста, выберите курс для просмотра статусов домашних заданий студента:", keyboard, messageId);
      TelegramBotHandler.InitializePagination(callbackQuery.From.Id, message.MessageId, callbacks);
    }

    public static async Task ClearData()
    {
      _answerData.Clear();
      _userSteps.Clear();
    }
  }
}
