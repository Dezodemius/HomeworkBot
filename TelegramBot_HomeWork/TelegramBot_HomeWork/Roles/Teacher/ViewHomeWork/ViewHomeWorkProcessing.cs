using Core;
using DataContracts;
using System.Text;
using Telegram.Bot;
using TelegramBot.Model;
using static DataContracts.Models.Submission;

namespace TelegramBot.Roles.Teacher.ViewHomeWork
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
    internal static Dictionary<long, ViewAssigmnetsModel> teacherData = new Dictionary<long, ViewAssigmnetsModel>();

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
    internal struct ViewAssigmnetsModel
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

      ChooseStatusStudent,
      ChooseStatusAssigment,

      Revision,
      Passed,
      Cancellation,

      TeacherMessages,
      ChangeStatus,
      Complited,
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
      CallbackModel callbackModel = new CallbackModel("Отправить коментарии", "/send");

      if (callbackData.ToLower().Contains("/send"))
      {
        if (teacherData.TryGetValue(chatId, out ViewAssigmnetsModel data))
        {
          var result = HomeWorkUpgrate(botClient, chatId, callbackData, messageId, StatusWork.NeedsRevision);
          if (result)
          {
            var assigmentData = CommonHomeWork.GetAssignmentById(data.courseId, data.assigmentId);
            var studentData = CommonUserModel.GetUserByChatId(data.studentId);
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
    /// Изменяет текущий шаг пользователя в процессе обработки заданий, обновляя состояние на новый шаг.
    /// </summary>
    /// <param name="chatId">Идентификатор чата пользователя.</param>
    /// <param name="newStep">Новый шаг, на который необходимо перейти.</param>
    static internal void ChangeStep(long chatId, ViewAssigmentStep newStep)
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
    static internal void ChangeData(long chatId, ViewAssigmnetsModel newData)
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
    static internal void DeleteStep(long chatId)
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
    static internal bool HomeWorkUpgrate(ITelegramBotClient botClient, long chatId, string callbackData, int messageId, StatusWork statusWork)
    {
      if (teacherData.TryGetValue(chatId, out ViewAssigmnetsModel model))
      {
        var homeWork = CommonSubmission.GetSubmissionForAssignment(model.studentId, model.assigmentId);
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
          {
            if (teacherData.TryGetValue(chatId, out ViewAssigmnetsModel data))
            {
              var result = HomeWorkUpgrate(botClient, chatId, callbackData, messageId, StatusWork.Checked);
              if (result)
              {
                var assigmentData = CommonHomeWork.GetAssignmentById(data.courseId, data.assigmentId);
                var studentData = CommonUserModel.GetUserByChatId(data.studentId);

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
              var result = ViewHomeWorkProcessing.HomeWorkUpgrate(botClient, chatId, callbackData, messageId, StatusWork.NeedsRevision);
              if (result)
              {
                var assigmentData = CommonHomeWork.GetAssignmentById(data.courseId, data.assigmentId);
                var studentData = CommonUserModel.GetUserByChatId(data.studentId);

                await TelegramBotHandler.SendMessageAsync(botClient, chatId, $"Введите коментарии. Вы можете отправить несколько штук.", null, messageId);
                await ViewHomeWorkProcessing.SendMessages(botClient, chatId, callbackData, messageId);

                ChangeStep(chatId, ViewAssigmentStep.Revision);
              }
            }
            break;
          }

        case "break":
          {
            ChangeStep(chatId, ViewAssigmentStep.ChooseAssigment);
            if (students)
            { 
            await StudentHomeworkViewer.ShowAssigmentByStudent(botClient, chatId, callbackData, messageId);
            }
            else
            {
              await HomeworkStudentViwer.ShowStudentByAssigment(botClient, chatId, callbackData, messageId);
            }
            break;
          }
      }
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
    static async Task ProcessStep(ITelegramBotClient botClient, long chatId, string message, int messageId, ViewAssigmentStep step)
    {
      switch (step)
      {
        case ViewAssigmentStep.ChooseOption:
          await ShowOptions(botClient, chatId, message, messageId);
          break;

        case ViewAssigmentStep.ComplitedChoose:
          await ChangeOptions(botClient, chatId, message, messageId);
          break;

        case ViewAssigmentStep.ChooseAssigment:
        case ViewAssigmentStep.DisplayAssigmentByStudent:
          await StudentHomeworkViewer.ProcessStep(botClient, chatId, message, messageId, step);
          break;

        case ViewAssigmentStep.ChooseStudent:
        case ViewAssigmentStep.DisplayStudentByAssigment:
          await HomeworkStudentViwer.ProcessStep(botClient, chatId, message, messageId, step);
          break;

        case ViewAssigmentStep.Revision:
          await SendMessages(botClient, chatId, message, messageId);
          break;

        case ViewAssigmentStep.ChooseStatusStudent:
          await ChooseStatusProseccing(botClient, chatId, message, messageId, true);
          break;

        case ViewAssigmentStep.ChooseStatusAssigment:
          await ChooseStatusProseccing(botClient, chatId, message, messageId, false);
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
    static async Task ShowCourses(ITelegramBotClient botClient, long chatId, string callbackData, int messageId)
    {
      InitializeTeacherStep(chatId, ViewAssigmentStep.ChooseOption);

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
      var model = new ViewAssigmnetsModel { courseId = courseId };
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
      ChangeStep(chatId, ViewAssigmentStep.ComplitedChoose);

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
    static void InitializeTeacherStep(long chatId, ViewAssigmentStep step)
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
        await HomeworkStudentViwer.ShowAssigmentForCourse(botClient, chatId, messageId);
      }
      else
      {
        await SendErrorMessage(botClient, chatId, messageId, "Ошибка выбора функции просмотра");
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
    static async Task ShowAssigmentForCourse(ITelegramBotClient botClient, long chatId, int messageId)
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
    #endregion

    #endregion
  }
}
