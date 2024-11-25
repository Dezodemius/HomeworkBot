using DataContracts.Interfaces;
using DataContracts.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot;
using Core;
using System.Globalization;
using static DataContracts.Models.Submission;
using TelegramBot.Model;
using TelegramBot.Processing;
using System.ComponentModel.DataAnnotations;
using Telegram.Bot.Types;
using System.Text.RegularExpressions;
using TelegramBot.Roles.Student;
using TelegramBot.Roles.Teacher.ViewHomeWork;

namespace TelegramBot.Roles.Teacher
{
  /// <summary>
  /// Представляет пользователя с ролью учителя в системе.
  /// </summary>
  public class Teacher : UserModel, IMessageHandler
  {

    static internal Dictionary<long, Assignment> assignments = new Dictionary<long, Assignment>();

    /// <summary>
    /// Инициализирует новый экземпляр класса Teacher.
    /// </summary>
    /// <param name="telegramChatId">Идентификатор чата Telegram.</param>
    /// <param name="firstName">Имя учителя.</param>
    /// <param name="lastName">Фамилия учителя.</param>
    /// <param name="email">Адрес электронной почты учителя.</param>
    public Teacher(long telegramChatId, string firstName, string lastName, string email)
        : base(telegramChatId, firstName, lastName, email, UserRole.Teacher) { }

    /// <summary>
    /// Обрабатывает входящее сообщение от преподавателя.
    /// </summary>
    /// <param name="message">Текст сообщения от преподавателя.</param>
    /// <returns>Ответ на сообщение учителя.</returns>
    public async Task ProcessMessageAsync(ITelegramBotClient botClient, long chatId, string message)
    {
      if (string.IsNullOrEmpty(message))
      {
        return;
      }

      if (assignments.TryGetValue(chatId, out Assignment? homeworkData))
      {
        await new CreateHomeWorkProcessing(homeworkData).ProcessCreateStepAsync(botClient, chatId, message);
      }
      else if (ViewHomeWorkProcessing.teacherStep.ContainsKey(chatId))
      {
        ViewHomeWorkProcessing.ViewAssigment(botClient, chatId, message);
      }
      else if (message == "/start")
      {
        ViewHomeWorkProcessing.teacherStep.Remove(chatId);
        await ShowTeacherOptions(botClient, chatId);
      }
    }

    /// <summary>
    /// Отображает доступные функции студента.
    /// </summary>
    private async Task ShowTeacherOptions(ITelegramBotClient botClient, long chatId)
    {
      List<CallbackModel> callbackModels = new List<CallbackModel>();
      callbackModels.Add(new CallbackModel("Создать домашнюю работу", "/createHomework"));
      callbackModels.Add(new CallbackModel("Просмотр домашних работ", "/checkHomework"));
      await TelegramBotHandler.SendMessageAsync(botClient, chatId, "Выберите функцию:", TelegramBotHandler.GetInlineKeyboardMarkupAsync(callbackModels));
    }

    /// <summary>
    /// Обработка Callback запросов от преподавателя.
    /// </summary>
    /// <param name="callbackData">Данные обратного вызова.</param>
    /// <returns>Результат обработки запроса.</returns>
    /// <exception cref="NotImplementedException"></exception>
    public async Task ProcessCallbackAsync(ITelegramBotClient botClient, long chatId, string callbackData, int messageId)
    {
      if (string.IsNullOrEmpty(callbackData)) return;
      else if (callbackData.ToLower().Contains("/createhomework"))
      {
        await CreateHomeWork(botClient, chatId, callbackData);
      }
      else if (callbackData.ToLower().Contains("/checkhomework"))
      {
        await ViewHomeWorkProcessing.ViewAssigment(botClient, chatId, callbackData, messageId);
        //await HandleCheckHomeworkCallback(botClient, chatId, callbackData, messageId);
      }
      else if (callbackData.ToLower().Contains("/send"))
      {
        await ViewHomeWorkProcessing.ViewAssigment(botClient, chatId, callbackData, messageId);
      }
      else if (callbackData == "/start")
      {
        ViewHomeWorkProcessing.teacherStep.Remove(chatId);
        await ShowTeacherOptions(botClient, chatId);
        await botClient.DeleteMessageAsync(chatId, messageId);
      }
    }

    /// <summary>
    /// Процесс создания нового домашнего задания.
    /// </summary>
    /// <param name="botClient"></param>
    /// <param name="chatId"></param>
    /// <param name="callbackData"></param>
    /// <returns></returns>
    private async Task CreateHomeWork(ITelegramBotClient botClient, long chatId, string callbackData)
    {
      if (!assignments.ContainsKey(chatId))
      {
        assignments.Add(chatId, new Assignment());
      }

      if (assignments.TryGetValue(chatId, out Assignment? homeworkData))
      {
        await new CreateHomeWorkProcessing(homeworkData).ProcessCreateStepAsync(botClient, chatId, callbackData);
      }
    }

    /// <summary>
    /// Обрабатывает обратные вызовы для проверки домашних работ.
    /// </summary>
    /// <param name="botClient">Клиент Telegram Bot.</param>
    /// <param name="chatId">Идентификатор чата.</param>
    /// <param name="callbackData">Данные обратного вызова.</param>
    /// <param name="messageId">Идентификатор сообщения.</param>
    /// <returns></returns>
    private async Task HandleCheckHomeworkCallback(ITelegramBotClient botClient, long chatId, string callbackData, int messageId)
    {
      if (callbackData.ToLower().Contains("students"))
      {
        if (callbackData.ToLower().Contains("studentid"))
        {
          await ShowHomeworkForStudent(botClient, chatId, callbackData, messageId);
        }
        else
        {
          await ShowStudentsForCourse(botClient, chatId, callbackData, messageId);
        }
      }
      else if (callbackData.ToLower().Contains("homeworks"))
      {
        await ShowHomeworksForCourse(botClient, chatId, callbackData, messageId);
      }
      else if (callbackData.ToLower().Contains("course"))
      {
        await ShowCourseOptions(botClient, chatId, callbackData, messageId);
      }
      else
      {
        await ShowCourses(botClient, chatId, callbackData, messageId);
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
    private async Task ShowStudentsForCourse(ITelegramBotClient botClient, long chatId, string callbackData, int messageId)
    {
      var data = callbackData.Split("_");
      var courseId = data[2];
      var students = CommonUserModel.GetStudentsByCourseId(Convert.ToInt32(courseId));

      List<CallbackModel> callbackModels = new List<CallbackModel>();
      foreach (var student in students)
      {
        callbackModels.Add(new CallbackModel($"{student.LastName} {student.FirstName}", $"{callbackData}_studentId_{student.TelegramChatId}"));
      }

      await TelegramBotHandler.SendMessageAsync(botClient, chatId, "Выберите студента:", TelegramBotHandler.GetInlineKeyboardMarkupAsync(callbackModels), messageId);
    }

    private async Task ShowHomeworkForStudent(ITelegramBotClient botClient, long chatId, string callbackData, int messageId)
    {
      var courseMatch = Regex.Match(callbackData, @"course_(\d+)");
      var studentIdMatch = Regex.Match(callbackData, @"studentId_(\d+)");

      if (courseMatch.Success && studentIdMatch.Success)
      {
        int courseId = int.Parse(courseMatch.Groups[1].Value);
        long studentId = long.Parse(studentIdMatch.Groups[1].Value);

        var data = CommonSubmission.GetSubmissionsByCourse(studentId, courseId);

        List<CallbackModel> callbackModels = new List<CallbackModel>();
        foreach (var item in data)
        {
          var homeWork = CommonHomeWork.GetAssignmentById(item.CourseId, item.AssignmentId);
          if (homeWork != null)
          {
            callbackModels.Add(new CallbackModel($"{homeWork.Title}", $"{callbackData}_studentId_{studentId}_homeWorkId_{homeWork.AssignmentId}"));
          }
        }

        await TelegramBotHandler.SendMessageAsync(botClient, chatId, "Выберите домашнюю работу:", TelegramBotHandler.GetInlineKeyboardMarkupAsync(callbackModels), messageId);
      }
      else
      {
        throw new ArgumentException("Не удалось найти идентификаторы 'course' или 'studentId' в строке.");
      }
    }

    /// <summary>
    /// Отображает домашние работы для выбранного курса.
    /// </summary>
    /// <param name="botClient">Клиент Telegram Bot.</param>
    /// <param name="chatId">Идентификатор чата.</param>
    /// <param name="callbackData">Данные обратного вызова.</param>
    /// <param name="messageId">Идентификатор сообщения.</param>
    /// <returns></returns>
    private async Task ShowHomeworksForCourse(ITelegramBotClient botClient, long chatId, string callbackData, int messageId)
    {
      // Здесь можно добавить логику для отображения домашних работ
      await TelegramBotHandler.SendMessageAsync(botClient, chatId, "Логика для отображения домашних работ еще не реализована.", null, messageId);
    }

    /// <summary>
    /// Отображает варианты курсов для проверки домашних работ.
    /// </summary>
    /// <param name="botClient">Клиент Telegram Bot.</param>
    /// <param name="chatId">Идентификатор чата.</param>
    /// <param name="callbackData">Данные обратного вызова.</param>
    /// <param name="messageId">Идентификатор сообщения.</param>
    /// <returns></returns>
    private async Task ShowCourseOptions(ITelegramBotClient botClient, long chatId, string callbackData, int messageId)
    {
      List<CallbackModel> callbackModels = new List<CallbackModel>
    {
        new CallbackModel("Просмотр по пользователям", $"{callbackData}_students"),
        new CallbackModel("Просмотр по домашним работам", $"{callbackData}_homeworks")
    };

      await TelegramBotHandler.SendMessageAsync(botClient, chatId, "Выберите функцию:", TelegramBotHandler.GetInlineKeyboardMarkupAsync(callbackModels), messageId);
    }

    /// <summary>
    /// Отображает курсы, связанные с преподавателем.
    /// </summary>
    /// <param name="botClient">Клиент Telegram Bot.</param>
    /// <param name="chatId">Идентификатор чата.</param>
    /// <param name="callbackData">Данные обратного вызова.</param>
    /// <param name="messageId">Идентификатор сообщения.</param>
    /// <returns></returns>
    private async Task ShowCourses(ITelegramBotClient botClient, long chatId, string callbackData, int messageId)
    {
      List<CallbackModel> callbackModels = new List<CallbackModel>();
      var teacher = CommonUserModel.GetUserByChatId(chatId);
      var courses = CommonCourseModel.GetUserCourses(teacher.TelegramChatId);

      foreach (var course in courses)
      {
        callbackModels.Add(new CallbackModel(course.CourseName, $"/checkHomework_course_{course.CourseId}"));
      }

      await TelegramBotHandler.SendMessageAsync(botClient, chatId, "Выберите курс:", TelegramBotHandler.GetInlineKeyboardMarkupAsync(callbackModels), messageId);
    }
  }
}
