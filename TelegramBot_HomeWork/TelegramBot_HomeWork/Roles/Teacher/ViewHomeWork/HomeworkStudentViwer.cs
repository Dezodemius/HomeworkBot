using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static TelegramBot.Roles.Teacher.ViewHomeWork.ViewHomeWorkProcessing;
using Telegram.Bot;
using Core;
using TelegramBot.Model;
using TelegramBot.Roles.Student;

namespace TelegramBot.Roles.Teacher.ViewHomeWork
{
  static internal class HomeworkStudentViwer
  {
    static internal async Task ProcessStep(ITelegramBotClient botClient, long chatId, string message, int messageId, ViewAssigmentStep step)
    {
      switch (step)
      {
        case ViewAssigmentStep.ChooseStudent:
          await ShowStudentByAssigment(botClient, chatId, message, messageId);
          break;
        case ViewAssigmentStep.DisplayStudentByAssigment:
          await ChooseStudentByAssigment(botClient, chatId, message, messageId);
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
    static internal async Task ShowAssigmentForCourse(ITelegramBotClient botClient, long chatId, int messageId)
    {
      if (teacherStep.TryGetValue(chatId, out ViewAssigmentStep step) && teacherData.TryGetValue(chatId, out ViewAssigmnetsModel data))
      {
        var homeworks = CommonHomeWork.GetAssignmentsByCourseId(data.courseId);
        var callbackModels = homeworks.Select(homework => new CallbackModel($"{homework.Title}", $"{functionHeader}_homeworkId_{homework.AssignmentId}")).ToList();

        ChangeStep(chatId, ViewAssigmentStep.ChooseStudent);
        await TelegramBotHandler.SendMessageAsync(botClient, chatId, "Выберите домашнее задание:", TelegramBotHandler.GetInlineKeyboardMarkupAsync(callbackModels), messageId);
      }
      else
      {
        await SendErrorMessage(botClient, chatId, messageId, "Ошибка при отображени студентов для выбранного курса");
        return;
      }
    }

    /// <summary>
    /// Отображает список студентов, по домашнему заданию для указанного курса, и предоставляет пользователю выбор студентов.
    /// </summary>
    /// <param name="botClient">Экземпляр клиента Telegram Bot.</param>
    /// <param name="chatId">Идентификатор чата пользователя.</param>
    /// <param name="callbackData">Данные, полученные из нажатия кнопки.</param>
    /// <param name="messageId">Идентификатор сообщения, на которое ответ осуществляется.</param>
    /// <returns>Асинхронная задача.</returns>
    static internal async Task ShowStudentByAssigment(ITelegramBotClient botClient, long chatId, string callbackData, int messageId)
    {
      var temporaryData = callbackData.Split('_');
      if (int.TryParse(temporaryData.Last(), out int assigmentId) && teacherStep.TryGetValue(chatId, out ViewAssigmentStep step) && teacherData.TryGetValue(chatId, out ViewAssigmnetsModel dataStudent))
      {
        var courseId = dataStudent.courseId;
        dataStudent.assigmentId = assigmentId;
        ChangeData(chatId, dataStudent);

        // TODO : тут кривые данные. Надо написать метод получения по assigmentId 
        var data = CommonSubmission.GetSubmissionsByCourseAndAssigment(assigmentId, courseId);
        ChangeStep(chatId, ViewAssigmentStep.DisplayStudentByAssigment);

        List<CallbackModel> callbackModels = new List<CallbackModel>();
        foreach (var item in data)
        {
          var student = CommonUserModel.GetUserByChatId(item.StudentId);
          if (student != null)
          {
            callbackModels.Add(new CallbackModel($"{student.LastName} {student.FirstName}", $"{functionHeader}_studentId_{student.TelegramChatId}"));
          }

          await TelegramBotHandler.SendMessageAsync(botClient, chatId, "Выберите студента:", TelegramBotHandler.GetInlineKeyboardMarkupAsync(callbackModels), messageId);
        }
      }
      else
      {
        await SendErrorMessage(botClient, chatId, messageId, "Ошибка при отображении списка студентов, по домашнему заданию для указанного курса.");
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
    static async Task ChooseStudentByAssigment(ITelegramBotClient botClient, long chatId, string callbackData, int messageId)
    {
      var temporaryData = callbackData.Split('_');
      if (long.TryParse(temporaryData.Last(), out long studentId) && teacherData.TryGetValue(chatId, out ViewAssigmnetsModel dataStudent))
      {
        dataStudent.studentId = studentId;
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

          ChangeStep(chatId, ViewAssigmentStep.ChooseStatusAssigment);
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
