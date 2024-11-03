using Core;
using DataContracts;
using System.Text;
using Telegram.Bot;
using TelegramBot.Model;
using static DataContracts.Models.Submission;

namespace TelegramBot.Roles.Teacher
{
  internal static class ViewHomeWorkProcessing
  {
    /// <summary>
    /// Текущий шаг преподавателя.
    /// </summary>
    internal static Dictionary<long, ViewAssigmentStep> teacherStep = new Dictionary<long, ViewAssigmentStep>();

    /// <summary>
    /// Текущее состояние выбранных данных.
    /// </summary>
    private static Dictionary<long, ViewAssigmnetsModel> teacherData = new Dictionary<long, ViewAssigmnetsModel>();

    /// <summary>
    /// Записи коментариев преподавателей.
    /// </summary>
    private static Dictionary<long, StringBuilder> teacherMessages = new Dictionary<long, StringBuilder>();

    const string functionHeader = "/checkHomework";

    /// <summary>
    /// Структура для хранения информации о выбранных данных,
    /// включая идентификатор курса, идентификатор задания и идентификатор студента.
    /// </summary>
    struct ViewAssigmnetsModel
    {
      /// <summary>
      /// Идентификатор курса
      /// </summary>
      public int courseId;

      /// <summary>
      /// Идентификатор задания
      /// </summary>
      public int assigmentId;

      /// <summary>
      /// Идентификатор студента
      /// </summary>
      public long studentId;
    }

    /// <summary>
    /// Шаг просмотра дз. 
    /// </summary>
    internal enum ViewAssigmentStep
    {
      ChooseOption,
      ComplitedChoose,

      DisplayStudent,
      DisplayAssigment,

      ChooseAssigment,
      ChooseStudent,

      DisplayAssigmentByStudent,
      DisplayStudentByAssigment,

      ChooseStatus,

      Revision,
      Passed,
      Cancellation,

      TeacherMessages,
      ChangeStatus,
      Complited,
    }

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
        if (teacherStep.TryGetValue(chatId, out ViewAssigmentStep step))
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
    /// Выполнение шага просмотра задания.
    /// </summary>
    /// <param name="botClient"></param>
    /// <param name="chatId"></param>
    /// <param name="message"></param>
    /// <param name="messageId"></param>
    /// <param name="step"></param>
    /// <returns></returns>
    private static async Task ProcessStep(ITelegramBotClient botClient, long chatId, string message, int messageId, ViewAssigmentStep step)
    {
      switch (step)
      {
        case ViewAssigmentStep.ChooseOption:
          await ShowOptions(botClient, chatId, message, messageId);
          break;

        case ViewAssigmentStep.ComplitedChoose:
          await ChangeOptions(botClient, chatId, message, messageId);
          break;

        case ViewAssigmentStep.ChooseStudent:
          break;
        case ViewAssigmentStep.ChooseAssigment:
          await ShowAssigmentByStudent(botClient, chatId, message, messageId);
          break;
        case ViewAssigmentStep.DisplayAssigmentByStudent:
          await ChooseAssigmentByStudent(botClient, chatId, message, messageId);
          break;
        case ViewAssigmentStep.DisplayStudentByAssigment:
          break;
        case ViewAssigmentStep.ChooseStatus:
          await ChooseStatusProseccing(botClient, chatId, message, messageId);
          break;
        case ViewAssigmentStep.Revision:
          await SendMessages(botClient, chatId, message, messageId);
          break;
        case ViewAssigmentStep.Passed:
          break;
        case ViewAssigmentStep.Cancellation:
          break;
        case ViewAssigmentStep.TeacherMessages:
          break;
        case ViewAssigmentStep.ChangeStatus:
          break;
        case ViewAssigmentStep.Complited:
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
    static private async Task ShowCourses(ITelegramBotClient botClient, long chatId, string callbackData, int messageId)
    {
      InitializeTeacherStep(chatId, ViewAssigmentStep.ChooseOption);

      var teacher = CommonUserModel.GetUserById(chatId);
      var courses = CommonCourseModel.GetAllUserCourses(teacher.TelegramChatId);
      var callbackModels = courses.Select(course => new CallbackModel(course.CourseName, $"{functionHeader}_course_{course.CourseId}")).ToList();

      await TelegramBotHandler.SendMessageAsync(botClient, chatId, "Выберите курс:", TelegramBotHandler.GetInlineKeyboardMarkupAsync(callbackModels), messageId);
    }

    private static void InitializeTeacherStep(long chatId, ViewAssigmentStep step)
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
    /// Отображает доступные опции для выбранного курса, позволяя преподавателю выбрать функцию для дальнейших действий.
    /// </summary>
    /// <param name="botClient">Экземпляр клиента Telegram Bot.</param>
    /// <param name="chatId">Идентификатор чата пользователя.</param>
    /// <param name="callbackData">Данные, полученные из нажатия кнопки.</param>
    /// <param name="messageId">Идентификатор сообщения, на которое ответ осуществляется.</param>
    /// <returns>Асинхронная задача.</returns>
    static private async Task ShowOptions(ITelegramBotClient botClient, long chatId, string callbackData, int messageId)
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

    private static void UpdateViewAssigmnetsModel(long chatId, int courseId)
    {
      var model = new ViewAssigmnetsModel { courseId = courseId };
      teacherData[chatId] = model;
      Logger.LogInfo($"Данные обновлены для чата {chatId} с курсом {courseId}.");
    }
    private static async Task SendOptions(ITelegramBotClient botClient, long chatId, int messageId)
    {
      ChangeStep(chatId, ViewAssigmentStep.ComplitedChoose);

      var callbackModels = new List<CallbackModel>
      {
        new CallbackModel("Просмотр по пользователям", $"{functionHeader}_students"),
        // TODO : позже раскоментить, одни проблемы пока
        //new CallbackModel("Просмотр по домашним работам", $"{functionHeader}_homeworks")
      };

      await TelegramBotHandler.SendMessageAsync(botClient, chatId, "Выберите функцию:", TelegramBotHandler.GetInlineKeyboardMarkupAsync(callbackModels), messageId);
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
    static private async Task ChangeOptions(ITelegramBotClient botClient, long chatId, string callbackData, int messageId)
    {
      var data = callbackData.Split('_');
      if (data.Last() == "students")
      {
        await ShowStudentsForCourse(botClient, chatId, messageId);
      }
      else if (data.Last() == "homeworks")
      {
        await ShowAssigmentForCourse(botClient, chatId, messageId);
      }
      else
      {
        await SendErrorMessage(botClient, chatId, messageId, "Ошибка выбора функции просмотра");
        return;
      }
    }

    /// <summary>
    /// Отображает студентов для выбранного курса.
    /// </summary>
    /// <param name="botClient">Клиент Telegram Bot.</param>
    /// <param name="chatId">Идентификатор чата.</param>
    /// <param name="callbackData">Данные обратного вызова.</param>
    /// <param name="messageId">Идентификатор сообщения.</param>
    /// <returns></returns>
    static private async Task ShowStudentsForCourse(ITelegramBotClient botClient, long chatId, int messageId)
    {
      if (teacherStep.TryGetValue(chatId, out ViewAssigmentStep step) && teacherData.TryGetValue(chatId, out ViewAssigmnetsModel data))
      {
        var students = CommonUserModel.GetAllStudentsByCourse(data.courseId);
        var callbackModels = students.Select(student => new CallbackModel($"{student.LastName} {student.FirstName}", $"{functionHeader}_studentId_{student.TelegramChatId}")).ToList();

        ChangeStep(chatId, ViewAssigmentStep.ChooseAssigment);
        await TelegramBotHandler.SendMessageAsync(botClient, chatId, "Выберите студента:", TelegramBotHandler.GetInlineKeyboardMarkupAsync(callbackModels), messageId);
      }
      else
      {
        await SendErrorMessage(botClient, chatId, messageId, "Ошибка при отображени студентов для выбранного курса");
        return;
      }
    }

    /// <summary>
    /// Отображает задания для выбранного курса.
    /// </summary>
    /// <param name="botClient">Клиент Telegram Bot.</param>
    /// <param name="chatId">Идентификатор чата.</param>
    /// <param name="callbackData">Данные обратного вызова.</param>
    /// <param name="messageId">Идентификатор сообщения.</param>
    /// <returns></returns>
    private static async Task ShowAssigmentForCourse(ITelegramBotClient botClient, long chatId, int messageId)
    {
      if (teacherStep.TryGetValue(chatId, out ViewAssigmentStep step))
      {
        ChangeStep(chatId, ViewAssigmentStep.DisplayAssigment);
      }
      else
      {
        await SendErrorMessage(botClient, chatId, messageId, "Ошибка при отображении заданий для выбранного курса");
        return;
      }
    }

    /// <summary>
    /// Отображает список домашних заданий, отправленных студентом для указанного курса, и предоставляет пользователю выбор заданий.
    /// </summary>
    /// <param name="botClient">Экземпляр клиента Telegram Bot.</param>
    /// <param name="chatId">Идентификатор чата пользователя.</param>
    /// <param name="callbackData">Данные, полученные из нажатия кнопки.</param>
    /// <param name="messageId">Идентификатор сообщения, на которое ответ осуществляется.</param>
    /// <returns>Асинхронная задача.</returns>
    static private async Task ShowAssigmentByStudent(ITelegramBotClient botClient, long chatId, string callbackData, int messageId)
    {
      var temporaryData = callbackData.Split('_');

      if (long.TryParse(temporaryData.Last(), out long studentId) && teacherStep.TryGetValue(chatId, out ViewAssigmentStep step) && teacherData.TryGetValue(chatId, out ViewAssigmnetsModel dataStudent))
      {
        var courseId = dataStudent.courseId;
        dataStudent.studentId = studentId;
        ChangeData(chatId, dataStudent);

        var data = CommonSubmission.GetSubmissionByCourse(studentId, courseId);

        ChangeStep(chatId, ViewAssigmentStep.DisplayAssigmentByStudent);

        List<CallbackModel> callbackModels = new List<CallbackModel>();
        foreach (var item in data)
        {
          var homeWork = CommonHomeWork.GetHomeWorkById(item.CourseId, item.AssignmentId);

          if (homeWork != null)
          {
            callbackModels.Add(new CallbackModel($"{homeWork.Title}", $"{functionHeader}_assignment_{homeWork.AssignmentId}"));
          }
        }

        await TelegramBotHandler.SendMessageAsync(botClient, chatId, "Выберите домашнюю работу:", TelegramBotHandler.GetInlineKeyboardMarkupAsync(callbackModels), messageId);
      }
      else
      {
        await SendErrorMessage(botClient, chatId, messageId, "Ошибка при отображении списка домашних заданий, отправленных студентом для указанного курса, и предоставляет пользователю выбор заданий.");
        return;
      }
    }

    /// <summary>
    /// Позволяет преподавателю выбрать домашнее задание студента для просмотра, отображая информацию и предоставляя опции для дальнейших действий.
    /// </summary>
    /// <param name="botClient">Экземпляр клиента Telegram Bot.</param>
    /// <param name="chatId">Идентификатор чата пользователя.</param>
    /// <param name="callbackData">Данные, полученные из нажатия кнопки.</param>
    /// <param name="messageId">Идентификатор сообщения, на которое ответ осуществляется.</param>
    /// <returns>Асинхронная задача.</returns>
    static private async Task ChooseAssigmentByStudent(ITelegramBotClient botClient, long chatId, string callbackData, int messageId)
    {
      var temporaryData = callbackData.Split('_');
      if (int.TryParse(temporaryData.Last(), out int assigmentId) && teacherData.TryGetValue(chatId, out ViewAssigmnetsModel dataStudent))
      {
        dataStudent.assigmentId = assigmentId;
        ChangeData(chatId, dataStudent);

        var assigment = CommonSubmission.GetSubmissionForHomeWork(dataStudent.studentId, dataStudent.assigmentId);
        if (assigment != null)
        {
          var homeWork = CommonHomeWork.GetHomeWorkById(assigment.CourseId, assigment.AssignmentId);
          StringBuilder message = new StringBuilder();
          message.AppendLine(homeWork.Title);
          message.AppendLine(homeWork.Description);
          message.AppendLine($"Ссылка на домашнюю работу: {assigment.GithubLink}");
          List<CallbackModel> callbackModels = new List<CallbackModel>();
          callbackModels.Add(new CallbackModel("Зачтено", $"{functionHeader}_checked"));
          callbackModels.Add(new CallbackModel("На доработку", $"{functionHeader}_needsRevision"));
          callbackModels.Add(new CallbackModel("Назад", $"{functionHeader}_break"));
          await TelegramBotHandler.SendMessageAsync(botClient, chatId, message.ToString(), TelegramBotHandler.GetInlineKeyboardMarkupAsync(callbackModels), messageId);

          ChangeStep(chatId, ViewAssigmentStep.ChooseStatus);
        }
        else
        {
          await SendErrorMessage(botClient, chatId, messageId, "Ошибка при выборе домашнего задания студента для просмотра.");
        }
      }
      else
      {
        await SendErrorMessage(botClient, chatId, messageId, "Ошибка при выборе домашнего задания студента для просмотра.");
      }
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
    static private async Task ChooseStatusProseccing(ITelegramBotClient botClient, long chatId, string callbackData, int messageId)
    {
      var temporaryData = callbackData.Split('_');
      switch (temporaryData.Last())
      {
        case "checked":
          {
            if (teacherData.TryGetValue(chatId, out ViewAssigmnetsModel data))
            {
              var result = HomeWorkUpgrate(botClient, chatId, callbackData, messageId, StatusWork.Checked);
              if (result)
              {
                var assigmentData = CommonHomeWork.GetHomeWorkById(data.courseId, data.assigmentId);
                var studentData = CommonUserModel.GetUserById(data.studentId);

                await TelegramBotHandler.SendMessageAsync(botClient, chatId, $"Домашнее задание {assigmentData.Title} студента {studentData.LastName} {studentData.FirstName} зачтено!", null, messageId);
                await TelegramBotHandler.SendMessageAsync(botClient, data.studentId, $"Домашнее задание {assigmentData.Title} зачтено!");
                DeleteStep(chatId);
              }
            }
            break;
          }

        case "needsRevision":
          {
            if (teacherData.TryGetValue(chatId, out ViewAssigmnetsModel data))
            {
              var result = HomeWorkUpgrate(botClient, chatId, callbackData, messageId, StatusWork.NeedsRevision);
              if (result)
              {
                var assigmentData = CommonHomeWork.GetHomeWorkById(data.courseId, data.assigmentId);
                var studentData = CommonUserModel.GetUserById(data.studentId);

                await TelegramBotHandler.SendMessageAsync(botClient, chatId, $"Введите коментарии. Вы можете отправить несколько штук.", null, messageId);
                await SendMessages(botClient, chatId, callbackData, messageId);

                ChangeStep(chatId, ViewAssigmentStep.Revision);
              }
            }
            break;
          }

        case "break":
          {
            ChangeStep(chatId, ViewAssigmentStep.ChooseAssigment);
            await ShowAssigmentByStudent(botClient, chatId, callbackData, messageId);
            break;
          }
      }
    }

    /// <summary>
    /// Изменяет текущий шаг пользователя в процессе обработки заданий, обновляя состояние на новый шаг.
    /// </summary>
    /// <param name="chatId">Идентификатор чата пользователя.</param>
    /// <param name="newStep">Новый шаг, на который необходимо перейти.</param>
    static private void ChangeStep(long chatId, ViewAssigmentStep newStep)
    {
      if (teacherStep.ContainsKey(chatId))
      {
        DeleteStep(chatId);
        teacherStep.Add(chatId, newStep);
      }
    }

    /// <summary>
    /// Обновляет данные преподавателя в хранилище, заменяя старые данные новыми для указанного чата.
    /// </summary>
    /// <param name="chatId">Идентификатор чата пользователя.</param>
    /// <param name="newData">Новые данные для обновления.</param>
    static private void ChangeData(long chatId, ViewAssigmnetsModel newData)
    {
      if (teacherData.ContainsKey(chatId))
      {
        teacherData.Remove(chatId);
        teacherData.Add(chatId, newData);
      }
    }

    /// <summary>
    /// Удаляет текущий шаг пользователя из хранилища шагов, если он существует.
    /// </summary>
    /// <param name="chatId">Идентификатор чата пользователя.</param>
    static private void DeleteStep(long chatId)
    {
      if (teacherStep.ContainsKey(chatId))
      {
        teacherStep.Remove(chatId);
      }
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
    static private bool HomeWorkUpgrate(ITelegramBotClient botClient, long chatId, string callbackData, int messageId, StatusWork statusWork)
    {
      if (teacherData.TryGetValue(chatId, out ViewAssigmnetsModel model))
      {
        var homeWork = CommonSubmission.GetSubmissionForHomeWork(model.studentId, model.assigmentId);
        if (homeWork == null)
        {
          return false;
        }

        homeWork.Status = statusWork;
        CommonSubmission.UpdateSubmission(homeWork);
        return true;
      }
      else
      {
        return false;
      }
    }

    /// <summary>
    /// Отправляет сообщение с комментариями по домашнему заданию.
    /// </summary>
    /// <param name="botClient">Клиент Telegram Bot.</param>
    /// <param name="chatId">Идентификатор чата.</param>
    /// <param name="callbackData">Данные обратного вызова.</param>
    /// <param name="messageId">Идентификатор сообщения.</param>
    /// <returns></returns>
    static private async Task SendMessages(ITelegramBotClient botClient, long chatId, string callbackData, int messageId)
    {
      CallbackModel callbackModel = new CallbackModel("Отправить коментарии", "/send");

      if (callbackData.ToLower().Contains("/send"))
      {
        if (teacherData.TryGetValue(chatId, out ViewAssigmnetsModel data))
        {
          var result = HomeWorkUpgrate(botClient, chatId, callbackData, messageId, StatusWork.NeedsRevision);
          if (result)
          {
            var assigmentData = CommonHomeWork.GetHomeWorkById(data.courseId, data.assigmentId);
            var studentData = CommonUserModel.GetUserById(data.studentId);
            await TelegramBotHandler.SendMessageAsync(botClient, chatId, $"Домашнее задание {assigmentData.Title} студента {studentData.LastName} {studentData.FirstName} отправлено на доработку!", null, messageId);

            if (teacherMessages.TryGetValue(chatId, out StringBuilder stringBuilder))
            {
              await TelegramBotHandler.SendMessageAsync(botClient, data.studentId, $"Домашнее задание {assigmentData.Title} отправлено на доработку с коментариями: \r\n{stringBuilder.ToString()}", TelegramBotHandler.GetInlineKeyboardMarkupAsync(callbackModel));
            }

            DeleteStep(chatId);
          }
        }
      }
      else
      {
        if (teacherMessages.TryGetValue(chatId, out StringBuilder stringBuilder))
        {
          stringBuilder.AppendLine("*" + callbackData);
          await TelegramBotHandler.SendMessageAsync(botClient, chatId, $"Введите следующий комментарий. Записанные коментарии: \r\n{stringBuilder.ToString()}", TelegramBotHandler.GetInlineKeyboardMarkupAsync(callbackModel));
        }
        else
        {
          teacherMessages.Add(chatId, new StringBuilder());
        }
      }
    }

    /// <summary>
    /// Отправляет сообщение об ошибке пользователю в чат и записывает ошибку в лог.
    /// </summary>
    /// <param name="botClient">Экземпляр клиента Telegram Bot для отправки сообщения.</param>
    /// <param name="chatId">Идентификатор чата, куда будет отправлено сообщение.</param>
    /// <param name="messageId">Идентификатор сообщения, который необходимо обновить (если применимо).</param>
    /// <param name="errorMessage">Сообщение об ошибке, которое будет отправлено пользователю.</param>
    private static async Task SendErrorMessage(ITelegramBotClient botClient, long chatId, int messageId, string errorMessage)
    {
      await TelegramBotHandler.SendMessageAsync(botClient, chatId, $"Произошла системная ошибка! {errorMessage}");
      Logger.LogError($"Ошибка для чата {chatId}: {errorMessage}");
    }
  }
}
