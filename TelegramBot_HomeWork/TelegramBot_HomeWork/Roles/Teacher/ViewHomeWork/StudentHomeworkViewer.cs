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

namespace TelegramBot.Roles.Teacher.ViewHomeWork
{
  static internal class StudentHomeworkViewer
  {

    static internal async Task ProcessStep(ITelegramBotClient botClient, long chatId, string message, int messageId, ViewAssigmentStep step)
    {
      switch (step)
      {
        case ViewAssigmentStep.ChooseAssigment:
          await ShowAssigmentByStudent(botClient, chatId, message, messageId);
          break;
        case ViewAssigmentStep.DisplayAssigmentByStudent:
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
      if (teacherStep.TryGetValue(chatId, out ViewAssigmentStep step) && ViewHomeWorkProcessing.teacherData.TryGetValue(chatId, out ViewAssigmnetsModel data))
      {
        var students = CommonUserModel.GetStudentsByCourseId(data.courseId);
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
    /// Отображает список домашних заданий, отправленных студентом для указанного курса, и предоставляет пользователю выбор заданий.
    /// </summary>
    /// <param name="botClient">Экземпляр клиента Telegram Bot.</param>
    /// <param name="chatId">Идентификатор чата пользователя.</param>
    /// <param name="callbackData">Данные, полученные из нажатия кнопки.</param>
    /// <param name="messageId">Идентификатор сообщения, на которое ответ осуществляется.</param>
    /// <returns>Асинхронная задача.</returns>
    static internal async Task ShowAssigmentByStudent(ITelegramBotClient botClient, long chatId, string callbackData, int messageId)
    {
      var temporaryData = callbackData.Split('_');

      if (long.TryParse(temporaryData.Last(), out long studentId) && teacherStep.TryGetValue(chatId, out ViewAssigmentStep step) && teacherData.TryGetValue(chatId, out ViewAssigmnetsModel dataStudent))
      {
        var courseId = dataStudent.courseId;
        dataStudent.studentId = studentId;
        ChangeData(chatId, dataStudent);

        var data = CommonSubmission.GetSubmissionsByCourse(studentId, courseId);

        ChangeStep(chatId, ViewAssigmentStep.DisplayAssigmentByStudent);

        List<CallbackModel> callbackModels = new List<CallbackModel>();
        foreach (var item in data)
        {
          var homeWork = CommonHomeWork.GetAssignmentById(item.CourseId, item.AssignmentId);

          if (homeWork != null)
          {
            callbackModels.Add(new CallbackModel($"{homeWork.Title}", $"{functionHeader}_assignment_{homeWork.AssignmentId}"));
          }
        }

        await TelegramBotHandler.SendMessageAsync(botClient, chatId, "Выберите домашнюю работу:", TelegramBotHandler.GetInlineKeyboardMarkupAsync(callbackModels), messageId);
      }
      else
      {
        await SendErrorMessage(botClient, chatId, messageId, "Ошибка при отображении списка домашних заданий, отправленных студентом для указанного курса.");
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
    static async Task ChooseAssigmentByStudent(ITelegramBotClient botClient, long chatId, string callbackData, int messageId)
    {
      var temporaryData = callbackData.Split('_');
      if (int.TryParse(temporaryData.Last(), out int assigmentId) && teacherData.TryGetValue(chatId, out ViewAssigmnetsModel dataStudent))
      {
        dataStudent.assigmentId = assigmentId;
        ChangeData(chatId, dataStudent);

        var assigment = CommonSubmission.GetSubmissionForAssignment(dataStudent.studentId, dataStudent.assigmentId);
        if (assigment != null)
        {
          var homeWork = CommonHomeWork.GetAssignmentById(assigment.CourseId, assigment.AssignmentId);
          StringBuilder message = new StringBuilder();
          message.AppendLine(homeWork.Title);
          message.AppendLine(homeWork.Description);
          message.AppendLine($"Ссылка на домашнюю работу: {assigment.GithubLink}");
          List<CallbackModel> callbackModels = new List<CallbackModel>();
          callbackModels.Add(new CallbackModel("Зачтено", $"{functionHeader}_checked"));
          callbackModels.Add(new CallbackModel("На доработку", $"{functionHeader}_needsRevision"));
          callbackModels.Add(new CallbackModel("Назад", $"{functionHeader}_break"));
          await TelegramBotHandler.SendMessageAsync(botClient, chatId, message.ToString(), TelegramBotHandler.GetInlineKeyboardMarkupAsync(callbackModels), messageId);

          ChangeStep(chatId, ViewAssigmentStep.ChooseStatusStudent);
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

  
  }
}
