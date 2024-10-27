using Core;
using DataContracts;
using DataContracts.Interfaces;
using DataContracts.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBot.Model;
using static DataContracts.Models.Submission;

namespace TelegramBot.Roles.Student
{
  /// <summary>
  /// Представляет пользователя с ролью студента в системе.
  /// </summary>
  internal class Student : UserModel, IMessageHandler
  {
    private static readonly Dictionary<long, GitHubLink> _answerHomeWorks = new();

    /// <summary>
    /// Инициализирует новый экземпляр класса Student.
    /// </summary>
    internal Student(long telegramChatId, string firstName, string lastName, string email)
        : base(telegramChatId, firstName, lastName, email, UserRole.Student) { }

    /// <summary>
    /// Обрабатывает входящее сообщение от студента.
    /// </summary>
    public async Task ProcessMessageAsync(ITelegramBotClient botClient, long chatId, string message)
    {
      if (string.IsNullOrEmpty(message))
        return;

      if (_answerHomeWorks.ContainsKey(chatId))
      {
        await HandleHomeworkSubmission(botClient, chatId, message);
      }
      else if (message.Equals("/start", StringComparison.OrdinalIgnoreCase))
      {
        await ShowStudentOptions(botClient, chatId);
      }
    }

    /// <summary>
    /// Отображает доступные функции студента.
    /// </summary>
    private async Task ShowStudentOptions(ITelegramBotClient botClient, long chatId)
    {
      var keyboard = new InlineKeyboardMarkup(new[]
      {
                new[] { InlineKeyboardButton.WithCallbackData("Статусы домашних работ", "/homeWork") }
            });
      await TelegramBotHandler.SendMessageAsync(botClient, chatId, "Выберите функцию:", keyboard);
    }

    /// <summary>
    /// Обрабатывает входящие callback запросы от студента.
    /// </summary>
    public async Task ProcessCallbackAsync(ITelegramBotClient botClient, long chatId, string callbackData, int messageId)
    {
      if (string.IsNullOrEmpty(callbackData))
        return;

      if (callbackData.StartsWith("/homeWork"))
      {
        await HandleHomeWorkCallback(botClient, chatId, callbackData, messageId);
      }
      else if (callbackData.Contains("/start"))
      {
        await botClient.DeleteMessageAsync(chatId, messageId);
        await ProcessMessageAsync(botClient, chatId, callbackData);
      }
    }

    /// <summary>
    /// Обрабатывает сообщения о домашних заданиях.
    /// </summary>
    private async Task HandleHomeworkSubmission(ITelegramBotClient botClient, long chatId, string message)
    {
      if (IsGitHubLink(message))
      {
        await HandleGitHubLink(botClient, chatId, message);
      }
      else
      {
        await RequestValidGitHubLink(botClient, chatId);
      }
    }

    /// <summary>
    /// Обрабатывает callback запросы, связанные с домашними заданиями.
    /// </summary>
    private async Task HandleHomeWorkCallback(ITelegramBotClient botClient, long chatId, string callbackData, int messageId)
    {
      if (callbackData.Contains("id_"))
      {
        await DisplayActiveHomeWork(botClient, chatId, messageId, callbackData);
      }
      else if (callbackData.Contains("answer_"))
      {
        await HandleHomeworkResponse(botClient, chatId, messageId, callbackData);
      }
      else
      {
        await CheckStatusHomeWork(botClient, chatId, callbackData, messageId);
      }
    }

    /// <summary>
    /// Проверяет статусы домашних работ.
    /// </summary>
    private async Task CheckStatusHomeWork(ITelegramBotClient botClient, long chatId, string callbackData, int messageId)
    {
      StatusWork statusWork = callbackData switch
      {
        "/homeWork_unchecked" => StatusWork.Unchecked,
        "/homeWork_checked" => StatusWork.Checked,
        "/homeWork_needsRevision" => StatusWork.NeedsRevision,
        "/homeWork_unfulfilled" => StatusWork.Unfulfilled,
        _ => throw new NotImplementedException(),
      };

      await DisplayHomeWorkStatuses(botClient, chatId, messageId, statusWork, chatId);
    }

    /// <summary>
    /// Отображает список домашних заданий с их статусами для конкретного пользователя.
    /// </summary>
    private async Task DisplayHomeWorkStatuses(ITelegramBotClient botClient, long chatId, int messageId, StatusWork status, long userId)
    {
      var filteredHomeWorks = CommonHomeWork.GetHomeworkForStudent(userId)
          .Where(hw => hw.Status == status)
          .ToList();

      var student = CommonUserModel.GetUserById(userId);
      var callbackModels = new List<CallbackModel> { new CallbackModel("Назад", "/homeWork") };

      if (!filteredHomeWorks.Any())
      {
        await TelegramBotHandler.SendMessageAsync(botClient, chatId, $"Нет домашних заданий со статусом '{status}' для пользователя {student.LastName} {student.FirstName}.", TelegramBotHandler.GetInlineKeyboardMarkupAsync(callbackModels), messageId);
        return;
      }

      foreach (var hw in filteredHomeWorks)
      {
        var homeWorkData = CommonHomeWork.GetHomeWorkById(hw.CourseId, hw.AssignmentId);
        if (homeWorkData != null)
        {
          callbackModels.Add(new CallbackModel(homeWorkData.Title, $"/homeWork_id_{hw.CourseId}_{homeWorkData.AssignmentId}"));
        }
      }

      await TelegramBotHandler.SendMessageAsync(botClient, chatId, $"Домашние задания со статусом '{status}' для пользователя {student.LastName} {student.FirstName}:", TelegramBotHandler.GetInlineKeyboardMarkupAsync(callbackModels), messageId);
    }

    /// <summary>
    /// Отображает активные домашние задания.
    /// </summary>
    private async Task DisplayActiveHomeWork(ITelegramBotClient botClient, long chatId, int messageId, string message)
    {
      var data = message.Split('_');
      if (int.TryParse(data[^2], out int homeWorkId) && int.TryParse(data[^1], out int courseId))
      {
        var homeWorkData = CommonHomeWork.GetHomeWorkById(courseId, homeWorkId);
        if (homeWorkData != null)
        {
          var stringBuilder = new StringBuilder()
              .Append(homeWorkData.Title)
              .Append(homeWorkData.Description);

          var callbackModels = new List<CallbackModel>
                    {
                        new CallbackModel("Ответить", $"/homeWork_answer_{courseId}_{homeWorkData.AssignmentId}"),
                        new CallbackModel("Назад", "/homeWork")
                    };

          await TelegramBotHandler.SendMessageAsync(botClient, chatId, stringBuilder.ToString(), TelegramBotHandler.GetInlineKeyboardMarkupAsync(callbackModels), messageId);
        }
      }
      else
      {
        Logger.LogError("Не удалось преобразовать идентификаторы домашнего задания.");
      }
    }

    /// <summary>
    /// Обрабатывает ответ на домашнее задание.
    /// </summary>
    private async Task HandleHomeworkResponse(ITelegramBotClient botClient, long chatId, int messageId, string message)
    {
      var data = message.Split('_');
      if (int.TryParse(data[^2], out int homeWorkId) && int.TryParse(data[^1], out int courseId))
      {
        var gitHubLink = new GitHubLink { CourseId = courseId, HomeWorkId = homeWorkId };
        _answerHomeWorks[chatId] = gitHubLink;

        await TelegramBotHandler.SendMessageAsync(botClient, chatId, "Добавьте ссылку на ваш репозиторий GitHub для отправки на проверку.", null, messageId);
      }
      else
      {
        Logger.LogError("Не удалось преобразовать идентификаторы домашнего задания.");
      }
    }

    /// <summary>
    /// Обрабатывает ссылку на GitHub.
    /// </summary>
    private async Task HandleGitHubLink(ITelegramBotClient botClient, long chatId, string message)
    {
      var user = CommonUserModel.GetUserById(chatId);
      var teachers = CommonUserModel.GetAllTeachers();
      _answerHomeWorks.TryGetValue(chatId, out var gitLink);
      var homeWork = CommonHomeWork.GetHomeWorkById(gitLink.CourseId, gitLink.HomeWorkId);
      var submission = CommonSubmission.GetSubmissionForHomeWork(chatId, homeWork.AssignmentId);
      submission.GithubLink = message;
      submission.Status = StatusWork.Unchecked;
      CommonSubmission.UpdateSubmission(submission);

      foreach (var teacher in teachers)
      {
        await TelegramBotHandler.SendMessageAsync(botClient, teacher.TelegramChatId, $"Пользователь: {user.LastName} {user.FirstName} добавил ответ на \"{homeWork.Title}\" : {message}");
      }

      await TelegramBotHandler.SendMessageAsync(botClient, chatId, $"Ссылка получена: {message}. Ваше домашнее задание будет проверено.");
    }

    /// <summary>
    /// Запрашивает действительную ссылку на GitHub у студента.
    /// </summary>
    private async Task RequestValidGitHubLink(ITelegramBotClient botClient, long chatId)
    {
      await TelegramBotHandler.SendMessageAsync(botClient, chatId, "Пожалуйста, добавьте корректную ссылку на GitHub");
    }

    /// <summary>
    /// Проверяет, является ли строка действительной ссылкой на GitHub.
    /// </summary>
    private bool IsGitHubLink(string url)
    {
      var regex = new Regex(@"^https:\/\/github\.com\/.+");
      return regex.IsMatch(url);
    }
  }
}
