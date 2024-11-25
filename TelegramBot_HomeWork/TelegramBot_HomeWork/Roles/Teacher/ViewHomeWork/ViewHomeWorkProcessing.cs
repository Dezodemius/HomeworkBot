using Core;
using DataContracts;
using DataContracts.Models;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramBot.Model;
using static DataContracts.Models.Submission;
using static TelegramBot.Roles.Teacher.ViewHomeWork.HomeworkStudentViewer;

namespace TelegramBot.Roles.Teacher.ViewHomeWork
{
  internal static partial class ViewHomeWorkProcessing
  {
    /// <summary>
    /// Текущий шаг преподавателя.
    /// </summary>
    internal static Dictionary<long, ViewAssignmentStep> teacherStep = new Dictionary<long, ViewAssignmentStep>();

    /// <summary>
    /// Текущее состояние выбранных данных.
    /// </summary>
    internal static Dictionary<long, ViewAssignmentsModel> teacherData = new Dictionary<long, ViewAssignmentsModel>();

    /// <summary>
    /// Записи коментариев преподавателей.
    /// </summary>
    internal static Dictionary<long, StringBuilder> teacherMessages = new Dictionary<long, StringBuilder>();

    /// <summary>
    /// Заголовок callBack данных
    /// </summary>
    internal const string functionHeader = "/checkHomework";

    /// <summary>
    /// Структура для хранения информации о выбранных данных,
    /// включая идентификатор курса, идентификатор задания и идентификатор студента.
    /// </summary>
    internal struct ViewAssignmentsModel
    {
      /// <summary>
      /// Идентификатор курса
      /// </summary>
      public int CourseId { get; set; }

      /// <summary>
      /// Идентификатор задания
      /// </summary>
      public int AssignmentId { get; set; }

      /// <summary>
      /// Идентификатор студента
      /// </summary>
      public long StudentId { get; set; }
    }


    /// <summary>
    /// Класс, представляющий информацию о задании.
    /// </summary>
    public class AssignmentInfo
    {
      public string Title { get; set; }
      public string Description { get; set; }
      public string GithubLink { get; set; }
    }

    #region Методы.

    #region Internal
    /// <summary>
    /// Обрабатывает просмотр заданий в зависимости от текущего шага пользователя,
    /// вызывая соответствующие методы для отображения опций, заданий и статусов.
    /// </summary>
    /// <param name="botClient">Экземпляр клиента Telegram Bot.</param>
    /// <param name="chatId">Идентификатор чата пользователя.</param>
    /// <param name="message">Сообщение или данные, полученные от пользователя.</param>
    /// <param name="messageId">Идентификатор сообщения, на которое ответ осуществляется (по умолчанию 0).</param>
    /// <returns>Асинхронная задача.</returns>
    static internal async Task ViewAssigment(ITelegramBotClient botClient, long chatId, string message, int messageId = 0)
    {
      try
      {
        if (teacherStep.TryGetValue(chatId, out ViewAssignmentStep step))
        {
          await ProcessStep(botClient, chatId, message, messageId, step);
        }
        else
        {
          await ShowCourses(botClient, chatId, message, messageId);
        }
      }
      catch (Exception ex)
      {
        Logger.LogError($"Ошибка в ViewAssigment: {ex.Message}");
      }
    }

    /// <summary>
    /// Отправляет сообщение об ошибке пользователю в чат и записывает ошибку в лог.
    /// </summary>
    /// <param name="botClient">Экземпляр клиента Telegram Bot для отправки сообщения.</param>
    /// <param name="chatId">Идентификатор чата, куда будет отправлено сообщение.</param>
    /// <param name="messageId">Идентификатор сообщения, который необходимо обновить (если применимо).</param>
    /// <param name="errorMessage">Сообщение об ошибке, которое будет отправлено пользователю.</param>
    internal static async Task SendErrorMessage(ITelegramBotClient botClient, long chatId, int messageId, string errorMessage)
    {
      await TelegramBotHandler.SendMessageAsync(botClient, chatId, $"Произошла системная ошибка! {errorMessage}");
      Logger.LogError($"Ошибка для чата {chatId}: {errorMessage}");
    }

    /// <summary>
    /// Отправляет сообщение с комментариями по домашнему заданию.
    /// </summary>
    /// <param name="botClient">Клиент Telegram Bot.</param>
    /// <param name="chatId">Идентификатор чата.</param>
    /// <param name="callbackData">Данные обратного вызова.</param>
    /// <param name="messageId">Идентификатор сообщения.</param>
    /// <returns></returns>
    static internal async Task SendMessages(ITelegramBotClient botClient, long chatId, string callbackData, int messageId)
    {
      CallbackModel callbackModel = new CallbackModel("Отправить комментарии", "/send");

      if (callbackData.ToLower().Contains("/send"))
      {
        await ProcessSendRequest(botClient, chatId, callbackData, messageId, callbackModel);
      }
      else
      {
        await AddCommentToStorage(botClient, chatId, callbackData, messageId, callbackModel);
      }
    }

    /// <summary>
    /// Изменяет текущий шаг пользователя в процессе обработки заданий, обновляя состояние на новый шаг.
    /// </summary>
    /// <param name="chatId">Идентификатор чата пользователя.</param>
    /// <param name="newStep">Новый шаг, на который необходимо перейти.</param>
    static internal void ChangeStep(long chatId, ViewAssignmentStep newStep)
    {
      teacherStep[chatId] = newStep;
    }

    /// <summary>
    /// Обновляет данные преподавателя в хранилище, заменяя старые данные новыми для указанного чата.
    /// </summary>
    /// <param name="chatId">Идентификатор чата пользователя.</param>
    /// <param name="newData">Новые данные для обновления.</param>
    static internal void ChangeData(long chatId, ViewAssignmentsModel newData)
    {
      if (teacherData.ContainsKey(chatId))
      {
        teacherData[chatId] = newData;
      }
      else
      {
        teacherData.Add(chatId, newData);
      }
    }

    /// <summary>
    /// Удаляет текущий шаг пользователя из хранилища шагов, если он существует.
    /// </summary>
    /// <param name="chatId">Идентификатор чата пользователя.</param>
    static internal void DeleteStep(long chatId)
    {
      teacherStep.Remove(chatId);
    }

    /// <summary>
    /// Обновляет статус домашней работы в базе данных.
    /// </summary>
    /// <param name="botClient">Клиент Telegram Bot.</param>
    /// <param name="chatId">Идентификатор чата.</param>
    /// <param name="callbackData">Данные обратного вызова.</param>
    /// <param name="messageId">Идентификатор сообщения.</param>
    /// <param name="status">Новый статус работы.</param>
    /// <returns>Возвращает true, если обновление прошло успешно.</returns>
    static internal bool HomeWorkUpgrate(ITelegramBotClient botClient, long chatId, string callbackData, int messageId, StatusWork statusWork)
    {
      if (teacherData.TryGetValue(chatId, out ViewAssignmentsModel model))
      {
        var homeWork = CommonSubmission.GetSubmissionForAssignment(model.StudentId, model.AssignmentId);

        if (homeWork != null)
        {
          homeWork.Status = statusWork;
          CommonSubmission.UpdateSubmission(homeWork);
          return true;
        }
      }

      return false;
    }

    /// <summary>
    /// Обрабатывает выбор статуса домашнего задания, предоставляя возможность отметить его как 'зачтено',
    /// 'на доработку' или вернуться к выбору задания. В зависимости от выбранного статуса,
    /// отправляет соответствующие уведомления и управляет состоянием пользователя.
    /// </summary>
    /// <param name="botClient">Экземпляр клиента Telegram Bot.</param>
    /// <param name="chatId">Идентификатор чата пользователя.</param>
    /// <param name="callbackData">Данные, полученные из нажатия кнопки.</param>
    /// <param name="messageId">Идентификатор сообщения, на которое ответ осуществляется.</param>
    /// <returns>Асинхронная задача.</returns>
    static internal async Task ChooseStatusProseccing(ITelegramBotClient botClient, long chatId, string callbackData, int messageId, bool students)
    {
      var temporaryData = callbackData.Split('_');
      switch (temporaryData.Last())
      {
        case "checked":
          await ProcessCheckedStatus(botClient, chatId, callbackData, messageId);
          break;

        case "needsRevision":
          await ProcessNeedsRevisionStatus(botClient, chatId, callbackData, messageId);
          break;

        case "break":
          await ProcessBreakStatus(botClient, chatId, callbackData, messageId, students);
          break;
      }
    }

    /// <summary>
    /// Пытается получить данные о преподавателе для текущего чата.
    /// </summary>
    /// <param name="chatId">Идентификатор чата.</param>
    /// <param name="data">Возвращает данные о назначенных заданиях, если успешно найдено.</param>
    /// <returns>True, если данные успешно получены; иначе False.</returns>
    static internal bool TryGetTeacherData(long chatId, out ViewAssignmentsModel data)
    {
      data = new ViewAssignmentsModel();
      return teacherStep.TryGetValue(chatId, out _) && teacherData.TryGetValue(chatId, out data);
    }
    /// <summary>
    /// Отправляет преподавателю сообщение с опциями для выбора статуса задания (Зачтено, На доработку, Назад).
    /// </summary>
    /// <param name="botClient">Клиент Telegram Bot.</param>
    /// <param name="chatId">Идентификатор чата.</param>
    /// <param name="assignmentInfo">Информация о задании для отображения.</param>
    /// <param name="messageId">Идентификатор сообщения, к которому привязано сообщение с опциями.</param>
    static internal async Task SendAssignmentOptionsAsync(ITelegramBotClient botClient, long chatId, AssignmentInfo assignmentInfo, int messageId)
    {
      var message = $"{assignmentInfo.Title}\n{assignmentInfo.Description}\nСсылка на домашнюю работу: {assignmentInfo.GithubLink}";

      List<CallbackModel> callbackModels = new List<CallbackModel>
            {
                new CallbackModel("Зачтено", $"{functionHeader}_checked"),
                new CallbackModel("На доработку", $"{functionHeader}_needsRevision"),
                new CallbackModel("Назад", $"{functionHeader}_break")
            };

      await TelegramBotHandler.SendMessageAsync(botClient, chatId, message, TelegramBotHandler.GetInlineKeyboardMarkupAsync(callbackModels), messageId);
    }


    /// <summary>
    /// Получает информацию о задании студента.
    /// </summary>
    /// <param name="dataStudent">Модель данных о студенте и задании.</param>
    /// <returns>Информация о задании в виде строки, если задание найдено; иначе null.</returns>
    static internal AssignmentInfo RetrieveAssignmentInfo(ViewAssignmentsModel dataStudent)
    {
      var assignment = CommonSubmission.GetSubmissionForAssignment(dataStudent.StudentId, dataStudent.AssignmentId);
      if (assignment != null)
      {
        var homeWork = CommonHomeWork.GetAssignmentById(assignment.CourseId, assignment.AssignmentId);

        return new AssignmentInfo
        {
          Title = homeWork.Title,
          Description = homeWork.Description,
          GithubLink = assignment.GithubLink
        };
      }
      return null;
    }
    #endregion

    #region Private
    /// <summary>
    /// Выполнение шага просмотра задания.
    /// </summary>
    /// <param name="botClient"></param>
    /// <param name="chatId"></param>
    /// <param name="message"></param>
    /// <param name="messageId"></param>
    /// <param name="step"></param>
    /// <returns></returns>
    static async Task ProcessStep(ITelegramBotClient botClient, long chatId, string message, int messageId, ViewAssignmentStep step)
    {
      switch (step)
      {
        case ViewAssignmentStep.DisplayCourseOptions:
          await ShowOptions(botClient, chatId, message, messageId);
          break;

        case ViewAssignmentStep.ChooseOptions:
          await ChangeOptions(botClient, chatId, message, messageId);
          break;

        case ViewAssignmentStep.SelectAssignment:
        case ViewAssignmentStep.ShowAssignmentsForStudent:
          await StudentHomeworkViewer.ProcessStep(botClient, chatId, message, messageId, step);
          break;

        case ViewAssignmentStep.SelectStudent:
        case ViewAssignmentStep.ShowStudentsForAssignment:
          await HomeworkStudentViewer.ProcessStep(botClient, chatId, message, messageId, step);
          break;

        case ViewAssignmentStep.RequestRevision:
          await SendMessages(botClient, chatId, message, messageId);
          break;

        case ViewAssignmentStep.SelectStudentStatus:
          await ChooseStatusProseccing(botClient, chatId, message, messageId, true);
          break;

        case ViewAssignmentStep.SelectAssignmentStatus:
          await ChooseStatusProseccing(botClient, chatId, message, messageId, false);
          break;
      }
    }

    /// <summary>
    /// Отображает список курсов, доступных для выбранного преподавателя,
    /// позволяя ему выбрать курс из списка.
    /// </summary>
    /// <param name="botClient">Экземпляр клиента Telegram Bot.</param>
    /// <param name="chatId">Идентификатор чата пользователя.</param>
    /// <param name="callbackData">Данные, полученные из нажатия кнопки.</param>
    /// <param name="messageId">Идентификатор сообщения, на которое ответ осуществляется.</param>
    /// <returns>Асинхронная задача.</returns>
    static async Task ShowCourses(ITelegramBotClient botClient, long chatId, string callbackData, int messageId)
    {
      InitializeTeacherStep(chatId, ViewAssignmentStep.DisplayCourseOptions);

      var teacher = CommonUserModel.GetUserByChatId(chatId);
      var courses = CommonCourseModel.GetUserCourses(teacher.TelegramChatId);
      var callbackModels = courses.Select(course => new CallbackModel(course.CourseName, $"{functionHeader}_course_{course.CourseId}")).ToList();

      await TelegramBotHandler.SendMessageAsync(botClient, chatId, "Выберите курс:", TelegramBotHandler.GetInlineKeyboardMarkupAsync(callbackModels), messageId);
    }

    /// <summary>
    /// Отображает доступные опции для выбранного курса, позволяя преподавателю выбрать функцию для дальнейших действий.
    /// </summary>
    /// <param name="botClient">Экземпляр клиента Telegram Bot.</param>
    /// <param name="chatId">Идентификатор чата пользователя.</param>
    /// <param name="callbackData">Данные, полученные из нажатия кнопки.</param>
    /// <param name="messageId">Идентификатор сообщения, на которое ответ осуществляется.</param>
    /// <returns>Асинхронная задача.</returns>
    static async Task ShowOptions(ITelegramBotClient botClient, long chatId, string callbackData, int messageId)
    {
      var data = callbackData.Split('_');
      if (int.TryParse(data.Last(), out int courseId))
      {
        UpdateViewAssigmnetsModel(chatId, courseId);
        await SendOptions(botClient, chatId, messageId);
      }
      else
      {
        await SendErrorMessage(botClient, chatId, messageId, "Ошибка опций для выбранного курса");
        return;
      }
    }

    /// <summary>
    /// Обновляет модель просмотра заданий для указанного чата и курса.
    /// </summary>
    /// <param name="chatId">Идентификатор чата.</param>
    /// <param name="courseId">Идентификатор курса.</param>
    static void UpdateViewAssigmnetsModel(long chatId, int courseId)
    {
      var model = new ViewAssignmentsModel { CourseId = courseId };
      teacherData[chatId] = model;
      Logger.LogInfo($"Данные обновлены для чата {chatId} с курсом {courseId}.");
    }

    /// <summary>
    /// Отправляет пользователю варианты выбора функции просмотра.
    /// </summary>
    /// <param name="botClient">Клиент Telegram-бота.</param>
    /// <param name="chatId">Идентификатор чата.</param>
    /// <param name="messageId">Идентификатор сообщения для ответа.</param>
    /// <returns>Задача, представляющая асинхронную операцию.</returns>
    static async Task SendOptions(ITelegramBotClient botClient, long chatId, int messageId)
    {
      ChangeStep(chatId, ViewAssignmentStep.ChooseOptions);

      var callbackModels = new List<CallbackModel>
      {
        new CallbackModel("Просмотр по пользователям", $"{functionHeader}_students"),
        new CallbackModel("Просмотр по домашним работам", $"{functionHeader}_homeworks")
      };

      await TelegramBotHandler.SendMessageAsync(botClient, chatId, "Выберите функцию:", TelegramBotHandler.GetInlineKeyboardMarkupAsync(callbackModels), messageId);
    }

    /// <summary>
    /// Инициализирует шаг для преподавателя, связанный с определенным чатом.
    /// </summary>
    /// <param name="chatId">Идентификатор чата.</param>
    /// <param name="step">Шаг просмотра заданий.</param>
    static void InitializeTeacherStep(long chatId, ViewAssignmentStep step)
    {
      if (teacherStep.ContainsKey(chatId))
      {
        teacherStep[chatId] = step;
      }
      else
      {
        teacherStep.Add(chatId, step);
      }
    }

    /// <summary>
    /// Обрабатывает изменения опций в боте Telegram, проверяя и изменяя состояние пользователя,
    /// а также перенаправляя на соответствующие методы для отображения студентов или домашних заданий.
    /// </summary>
    /// <param name="botClient">Экземпляр клиента Telegram Bot.</param>
    /// <param name="chatId">Идентификатор чата пользователя.</param>
    /// <param name="callbackData">Данные, полученные из нажатия кнопки.</param>
    /// <param name="messageId">Идентификатор сообщения, на которое ответ осуществляется.</param>
    /// <returns>Асинхронная задача.</returns>
    static async Task ChangeOptions(ITelegramBotClient botClient, long chatId, string callbackData, int messageId)
    {
      var data = callbackData.Split('_');
      if (data.Last() == "students")
      {
        await StudentHomeworkViewer.ShowStudentsForCourse(botClient, chatId, messageId);
      }
      else if (data.Last() == "homeworks")
      {
        await HomeworkStudentViewer.ShowAssignmentForCourse(botClient, chatId, messageId);
      }
      else
      {
        await SendErrorMessage(botClient, chatId, messageId, "Ошибка выбора функции просмотра");
        return;
      }
    }

    /// <summary>
    /// Обрабатывает статус "зачтено", отправляет уведомления и обновляет состояние.
    /// </summary>
    static private async Task ProcessCheckedStatus(ITelegramBotClient botClient, long chatId, string callbackData, int messageId)
    {
      if (teacherData.TryGetValue(chatId, out ViewAssignmentsModel data))
      {
        var result = HomeWorkUpgrate(botClient, chatId, callbackData, messageId, StatusWork.Checked);
        if (result)
        {
          var assigmentData = CommonHomeWork.GetAssignmentById(data.CourseId, data.AssignmentId);
          var studentData = CommonUserModel.GetUserByChatId(data.StudentId);

          await TelegramBotHandler.SendMessageAsync(botClient, chatId, $"Домашнее задание {assigmentData.Title} студента {studentData.LastName} {studentData.FirstName} зачтено!", null, messageId);
          await TelegramBotHandler.SendMessageAsync(botClient, data.StudentId, $"Домашнее задание {assigmentData.Title} зачтено!");
          DeleteStep(chatId);
        }
      }
    }

    /// <summary>
    /// Обрабатывает статус "на доработку", запрашивая комментарии от пользователя.
    /// </summary>
    static private async Task ProcessNeedsRevisionStatus(ITelegramBotClient botClient, long chatId, string callbackData, int messageId)
    {
      if (teacherData.TryGetValue(chatId, out ViewAssignmentsModel data))
      {
        var result = ViewHomeWorkProcessing.HomeWorkUpgrate(botClient, chatId, callbackData, messageId, StatusWork.NeedsRevision);
        if (result)
        {
          var assigmentData = CommonHomeWork.GetAssignmentById(data.CourseId, data.AssignmentId);
          var studentData = CommonUserModel.GetUserByChatId(data.StudentId);

          await TelegramBotHandler.SendMessageAsync(botClient, chatId, $"Введите комментарии. Вы можете отправить несколько штук.", null, messageId);
          await ViewHomeWorkProcessing.SendMessages(botClient, chatId, callbackData, messageId);

          ChangeStep(chatId, ViewAssignmentStep.RequestRevision);
        }
      }
    }

    /// <summary>
    /// Обрабатывает статус "возврат к выбору задания", перенаправляя на отображение заданий.
    /// </summary>
    static private async Task ProcessBreakStatus(ITelegramBotClient botClient, long chatId, string callbackData, int messageId, bool students)
    {
      ChangeStep(chatId, ViewAssignmentStep.SelectAssignment);
      if (students)
      {
        await StudentHomeworkViewer.ShowAssigmentByStudent(botClient, chatId, callbackData, messageId);
      }
      else
      {
        await HomeworkStudentViewer.ShowStudentByAssignment(botClient, chatId, callbackData, messageId);
      }
    }

    /// <summary>
    /// Обрабатывает запрос на отправку домашнего задания на доработку.
    /// </summary>
    private static async Task ProcessSendRequest(ITelegramBotClient botClient, long chatId, string callbackData, int messageId, CallbackModel callbackModel)
    {
      if (teacherData.TryGetValue(chatId, out ViewAssignmentsModel data))
      {
        var result = HomeWorkUpgrate(botClient, chatId, callbackData, messageId, StatusWork.NeedsRevision);
        if (result)
        {
          var assignmentData = CommonHomeWork.GetAssignmentById(data.CourseId, data.AssignmentId);
          var studentData = CommonUserModel.GetUserByChatId(data.StudentId);

          await NotifyTeacher(botClient, chatId, assignmentData, studentData, messageId);
          await SendCommentsToStudent(botClient, data.StudentId, assignmentData, callbackModel);
          
          DeleteStep(chatId);
        }
      }
    }

    /// <summary>
    /// Отправляет уведомление преподавателю о том, что домашка отправлена на доработку.
    /// </summary>
    private static async Task NotifyTeacher(ITelegramBotClient botClient, long chatId, Assignment assignmentData, UserModel studentData, int messageId)
    {
      await TelegramBotHandler.SendMessageAsync(
          botClient,
          chatId,
          $"Домашнее задание {assignmentData.Title} студента {studentData.LastName} {studentData.FirstName} отправлено на доработку!",
          null,
          messageId
      );
    }

    /// <summary>
    /// Отправляет комментарии студенту.
    /// </summary>
    private static async Task SendCommentsToStudent(ITelegramBotClient botClient, long studentId, Assignment assignmentData, CallbackModel callbackModel)
    {
      if (teacherMessages.TryGetValue(studentId, out StringBuilder stringBuilder))
      {
        await TelegramBotHandler.SendMessageAsync(
            botClient,
            studentId,
            $"Домашнее задание {assignmentData.Title} отправлено на доработку с комментариями: \r\n{stringBuilder}",
            TelegramBotHandler.GetInlineKeyboardMarkupAsync(callbackModel)
        );
      }
    }

    /// <summary>
    /// Добавляет новый комментарий в хранилище комментариев преподавателя.
    /// </summary>
    private static async Task AddCommentToStorage(ITelegramBotClient botClient, long chatId, string callbackData, int messageId, CallbackModel callbackModel)
    {
      if (!teacherMessages.ContainsKey(chatId))
      {
        teacherMessages.Add(chatId, new StringBuilder());
      }

      teacherMessages[chatId].AppendLine("*" + callbackData);
      await TelegramBotHandler.SendMessageAsync(
          botClient,
          chatId,
          $"Введите следующий комментарий. Записанные комментарии: \r\n{teacherMessages[chatId]}",
          TelegramBotHandler.GetInlineKeyboardMarkupAsync(callbackModel)
      );
    }
    #endregion

    #endregion
  }
}
