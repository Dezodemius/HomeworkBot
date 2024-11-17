using Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static TelegramBot.Roles.Teacher.ViewHomeWork.ViewHomeWorkProcessing;
using Telegram.Bot;
using TelegramBot.Model;
using static DataContracts.Models.Submission;
using DataContracts.Models;
using TelegramBot.Roles.Student;
using static TelegramBot.Roles.Teacher.ViewHomeWork.HomeworkStudentViewer;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TelegramBot.Roles.Teacher.ViewHomeWork
{
  static internal class StudentHomeworkViewer
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
        case ViewAssignmentStep.SelectAssignment:
          await ShowAssigmentByStudent(botClient, chatId, message, messageId);
          break;
        case ViewAssignmentStep.ShowAssignmentsForStudent:
          await ChooseAssigmentByStudent(botClient, chatId, message, messageId);
          break;
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
    static internal async Task ShowStudentsForCourse(ITelegramBotClient botClient, long chatId, int messageId)
    {
      if (teacherStep.TryGetValue(chatId, out ViewAssignmentStep step) && ViewHomeWorkProcessing.teacherData.TryGetValue(chatId, out ViewAssignmentsModel data))
      {
        var students = CommonUserModel.GetStudentsByCourseId(data.CourseId);
        var callbackModels = students.Select(student => new CallbackModel($"{student.LastName} {student.FirstName}", $"{functionHeader}_studentId_{student.TelegramChatId}")).ToList();
        await TelegramBotHandler.SendMessageAsync(botClient, chatId, "Выберите студента:", TelegramBotHandler.GetInlineKeyboardMarkupAsync(callbackModels), messageId);

        ChangeStep(chatId, ViewAssignmentStep.SelectAssignment);
      }
      else
      {
        await SendErrorMessage(botClient, chatId, messageId, "Ошибка при отображени студентов для выбранного курса");
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
    static internal async Task ShowAssigmentByStudent(ITelegramBotClient botClient, long chatId, string callbackData, int messageId)
    {
      if (long.TryParse(callbackData.Split('_').Last(), out long studentId) && TryGetTeacherData(chatId, out var dataStudent))
      {
        dataStudent.StudentId = studentId;
        ChangeData(chatId, dataStudent);
        ChangeStep(chatId, ViewAssignmentStep.ShowAssignmentsForStudent);
        var callbackModels = GetAssignmentListByStudentAsync(dataStudent);

        await TelegramBotHandler.SendMessageAsync(botClient, chatId, "Выберите домашнюю работу:", TelegramBotHandler.GetInlineKeyboardMarkupAsync(callbackModels), messageId);
      }
      else
      {
        await SendErrorMessage(botClient, chatId, messageId, "Ошибка при отображении списка домашних заданий, отправленных студентом для указанного курса.");
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
    static async Task ChooseAssigmentByStudent(ITelegramBotClient botClient, long chatId, string callbackData, int messageId)
    {
      if (int.TryParse(callbackData.Split('_').Last(), out int assigmentId) && TryGetTeacherData(chatId, out var data))
      {
        data.AssignmentId = assigmentId;
        ChangeData(chatId, data);

        var assignmentInfo = RetrieveAssignmentInfo(data);
        if (assignmentInfo != null)
        {
          await SendAssignmentOptionsAsync(botClient, chatId, assignmentInfo, messageId);
          ChangeStep(chatId, ViewAssignmentStep.SelectStudentStatus);
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
    /// Получает список студентов, выполнивших задание для конкретного курса.
    /// </summary>
    /// <param name="data">Модель данных, содержащая идентификаторы курса и задания.</param>
    /// <returns>Список CallbackModel с информацией о каждом студенте, выполнившем задание.</returns>
    static List<CallbackModel?> GetAssignmentListByStudentAsync(ViewAssignmentsModel data)
    {
      var submissions = CommonSubmission.GetSubmissionsByCourse(data.StudentId, data.CourseId);
      return submissions
          .Select(submission => GetCallbackModelForAssignment(submission))
          .Where(callbackModel => callbackModel != null)
          .ToList();
    }

    /// <summary>
    /// Формирует модель кнопки для домашнего задания.
    /// </summary>
    private static CallbackModel? GetCallbackModelForAssignment(Submission submission)
    {
      var homeWork = CommonHomeWork.GetAssignmentById(submission.CourseId, submission.AssignmentId);
      return homeWork != null
          ? new CallbackModel($"{homeWork.Title}", $"{functionHeader}_assignment_{homeWork.AssignmentId}")
          : null;
    }
  }
}
