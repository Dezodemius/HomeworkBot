﻿using Telegram.Bot;
using Telegram.Bot.Types;
using HomeWorkTelegramBot.Models;
using HomeWorkTelegramBot.Core;
using HomeWorkTelegramBot.DataBase;
using System.Text;
using static HomeWorkTelegramBot.Config.Logger;

namespace HomeWorkTelegramBot.Bot.Function.Teacher
{
  public static class StudentsData
  {
    private static readonly Dictionary<long, Answer> _answerData = new Dictionary<long, Answer>();
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
      /// Выбор студента.
      /// </summary>
      StudentSelection,

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
      // нужно получить id препода, найти курс который он ведет, по id этого курса получить список студентов и вывести его в виде кнопок
      long chatId = callbackQuery.From.Id;
      string data = callbackQuery.Data;

      if (!_userSteps.ContainsKey(chatId))
      {
        await InitializeGetTasks(botClient, chatId);
        return;
      }

      var currentStep = _userSteps[chatId];
      var answer = _answerData[chatId];

      switch (currentStep)
      {
        case CreationStep.CourseSelection:
          await HandleCourseSelection(botClient, chatId, callbackQuery, answer);
          break;

        case CreationStep.StudentSelection:
          await HandleStudentSelection(botClient, chatId, callbackQuery, answer);
          break;
      }
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
        int courseId = int.Parse(data.Replace("/selectcourse_", string.Empty));
        answer.CourseId = courseId;
        _userSteps[chatId] = CreationStep.StudentSelection;
        List<Models.User> students = GetCourseStudents(courseId);
        var keyboard = GetInlineKeyboard.GetStudentsKeyboard(students);
        LogInformation($"Курс {courseId} выбран для просмотра статистики выполнения заданий студента преподавателем с ChatId {chatId}");
        await TelegramBotHandler.SendMessageAsync(botClient, chatId,
          $"Был выбран курс: {courseId}. Пожалуйста, выберите студента:", keyboard);
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
      var userRepository = new UserRepository();
      foreach (var student in studentsId)
      {
        var foundStudent = userRepository.GetUserByChatId(student.UserId);
        if (foundStudent != null)
        {
          students.Add(foundStudent);
        }
      }

      return students;
    }

    /// <summary>
    /// Обрабатывает нажатие на выбранног студента.
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
        int userId = int.Parse(data.Replace("/selectuser_", string.Empty));
        answer.UserId = userId;
        _userSteps[chatId] = CreationStep.Completed;
        string messageData = GetMessageData(chatId, answer);

        LogInformation($"Студен с chatId {userId} выбран для просмотра статистики выполнения заданий студента преподавателем с ChatId {chatId}");
        await TelegramBotHandler.SendMessageAsync(botClient, chatId, messageData);
      }
    }

    /// <summary>
    /// Подготавливает данные по статистике выполнений заданий студентом курса.
    /// </summary>
    /// <param name="chatId">Идентификатор чата студента.</param>
    /// <param name="answer">Объект класс Answer.</param>
    /// <returns>Строку, с данными о выполнении заданий студентом.</returns>
    private static string GetMessageData(long chatId, Answer answer)
    {
      var allStudentAnswers = AnswerService.GetAnswersByUserId(chatId);
      var studentAnswers = allStudentAnswers
        .Where(a => a.CourseId == answer.CourseId)
        .ToList();
      var sb = new StringBuilder();
      foreach (var answerData in studentAnswers)
      {
        var task = TaskWorkService.GetTaskWorkById(answerData.Id);
        sb.AppendLine($"Название: {task.Name}.\nСтатус: {answerData.Status}");
      }

      return sb.ToString();
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
      _answerData[chatId] = new Answer();
      LogInformation($"Начало получения статистики выполнения заданий студента преподавателем с ChatId {chatId}");
      var courses = CourseService.GetAllCoursesByTeacherId(chatId);
      var keyboard = GetInlineKeyboard.GetCoursesKeyboard(courses);
      await TelegramBotHandler.SendMessageAsync(botClient, chatId, "Пожалуйста, выберите курс:", keyboard);
    }
  }
}
