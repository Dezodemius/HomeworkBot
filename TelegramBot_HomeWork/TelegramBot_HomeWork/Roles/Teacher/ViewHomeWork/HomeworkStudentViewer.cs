using Core;
using static TelegramBot.Roles.Teacher.ViewHomeWork.ViewHomeWorkProcessing;
using Telegram.Bot;
using TelegramBot.Model;
using static System.Net.Mime.MediaTypeNames;

namespace TelegramBot.Roles.Teacher.ViewHomeWork
{
  static internal class HomeworkStudentViewer
  {
    /// <summary>
    /// Выполняет обработку шага в зависимости от текущего состояния.
    /// </summary>
    /// <param name="botClient">Клиент Telegram Bot.</param>
    /// <param name="chatId">Идентификатор чата.</param>
    /// <param name="message">Сообщение от пользователя.</param>
    /// <param name="messageId">Идентификатор сообщения.</param>
    /// <param name="step">Текущий шаг процесса.</param>
    static internal async Task ProcessStep(ITelegramBotClient botClient, long chatId, string message, int messageId, ViewAssignmentStep step)
    {
      switch (step)
      {
        case ViewAssignmentStep.SelectStudent:
          await ShowStudentByAssignment(botClient, chatId, message, messageId);
          break;
        case ViewAssignmentStep.ShowStudentsForAssignment:
          await ChooseStudentByAssignment(botClient, chatId, message, messageId);
          break;
      }
    }

    /// <summary>
    /// Отображает список домашних заданий для выбранного курса, позволяя преподавателю выбрать задание.
    /// </summary>
    /// <param name="botClient">Клиент Telegram Bot.</param>
    /// <param name="chatId">Идентификатор чата.</param>
    /// <param name="messageId">Идентификатор сообщения, на которое ответ отправляется.</param>
    static internal async Task ShowAssignmentForCourse(ITelegramBotClient botClient, long chatId, int messageId)
    {
      if (TryGetTeacherData(chatId, out var data))
      {
        var homeworks = CommonHomeWork.GetAssignmentsByCourseId(data.CourseId);
        var callbackModels = homeworks.Select(h => new CallbackModel(h.Title, $"{functionHeader}_homeworkId_{h.AssignmentId}")).ToList();
        await TelegramBotHandler.SendMessageAsync(botClient, chatId, "Выберите домашнее задание:", TelegramBotHandler.GetInlineKeyboardMarkupAsync(callbackModels), messageId);

        ChangeStep(chatId, ViewAssignmentStep.SelectStudent);
      }
      else
      {
        await SendErrorMessage(botClient, chatId, messageId, "Ошибка при отображении студентов для выбранного курса.");
      }
    }

    /// <summary>
    /// Отображает список студентов для выбранного задания, позволяя преподавателю выбрать студента.
    /// </summary>
    /// <param name="botClient">Клиент Telegram Bot.</param>
    /// <param name="chatId">Идентификатор чата.</param>
    /// <param name="callbackData">Данные, переданные через нажатие кнопки обратного вызова.</param>
    /// <param name="messageId">Идентификатор сообщения для ответа.</param>

    static internal async Task ShowStudentByAssignment(ITelegramBotClient botClient, long chatId, string callbackData, int messageId)
    {
      if (int.TryParse(callbackData.Split('_').Last(), out int assignmentId) && TryGetTeacherData(chatId, out var data))
      {
        data.AssignmentId = assignmentId;
        ChangeData(chatId, data);
        ChangeStep(chatId, ViewAssignmentStep.ShowStudentsForAssignment);

        var callbackModels = GetStudentListByAssignmentAsync(data);
        await TelegramBotHandler.SendMessageAsync(botClient, chatId, "Выберите студента:", TelegramBotHandler.GetInlineKeyboardMarkupAsync(callbackModels), messageId);
      }
      else
      {
        await SendErrorMessage(botClient, chatId, messageId, "Ошибка при отображении списка студентов.");
      }
    }

    /// <summary>
    /// Позволяет преподавателю выбрать конкретного студента для просмотра домашнего задания.
    /// Отображает информацию о задании и предоставляет опции для оценки.
    /// </summary>
    /// <param name="botClient">Клиент Telegram Bot.</param>
    /// <param name="chatId">Идентификатор чата.</param>
    /// <param name="callbackData">Данные, переданные через нажатие кнопки обратного вызова.</param>
    /// <param name="messageId">Идентификатор сообщения для ответа.</param>
    static async Task ChooseStudentByAssignment(ITelegramBotClient botClient, long chatId, string callbackData, int messageId)
    {
      if (long.TryParse(callbackData.Split('_').Last(), out long studentId) && TryGetTeacherData(chatId, out var data))
      {
        data.StudentId = studentId;

        var assignmentInfo = RetrieveAssignmentInfo(data);
        if (assignmentInfo != null)
        {
          await SendAssignmentOptionsAsync(botClient, chatId, assignmentInfo, messageId);
          ChangeStep(chatId, ViewAssignmentStep.SelectAssignmentStatus);
        }
        else
        {
          await SendErrorMessage(botClient, chatId, messageId, "Ошибка при выборе домашнего задания студента для просмотра.");
        }
        ChangeData(chatId, data);
      }
      else
      {
        await SendErrorMessage(botClient, chatId, messageId, "Ошибка при выборе домашнего задания студента для просмотра.");
      }
    }

    /// <summary>
    /// Получает список студентов, выполнивших задание для конкретного курса.
    /// </summary>
    /// <param name="data">Модель данных, содержащая идентификаторы курса и задания.</param>
    /// <returns>Список CallbackModel с информацией о каждом студенте, выполнившем задание.</returns>
    static List<CallbackModel> GetStudentListByAssignmentAsync(ViewAssignmentsModel data)
    {
      var students = CommonSubmission.GetSubmissionsByCourseAndAssignment(data.AssignmentId, data.CourseId);
      return students
          .Select(s => CommonUserModel.GetUserByChatId(s.StudentId))
          .Where(student => student != null)
          .Select(student => new CallbackModel($"{student.LastName} {student.FirstName}", $"{functionHeader}_studentId_{student.TelegramChatId}"))
          .ToList();
    }

  }
}
