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
using static DataContracts.Models.Submission;
using TelegramBot.Model;
using Telegram.Bot.Types;
using DataContracts;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Text.RegularExpressions;

namespace TelegramBot.Roles.Student
{
  /// <summary>
  /// Представляет пользователя с ролью студента в системе.
  /// </summary>
  internal class Student : UserModel, IMessageHandler
  {

    static private readonly Dictionary<long, GitHubLink> _answerHomeWorks = new Dictionary<long, GitHubLink>();

    /// <summary>
    /// Инициализирует новый экземпляр класса Student.
    /// </summary>
    /// <param name="telegramChatId">Идентификатор чата Telegram.</param>
    /// <param name="firstName">Имя студента.</param>
    /// <param name="lastName">Фамилия студента.</param>
    /// <param name="email">Адрес электронной почты студента.</param>
    /// <param name="dbManager">Менеджер базы данных.</param>
    internal Student(long telegramChatId, string firstName, string lastName, string email)
        : base(telegramChatId, firstName, lastName, email, UserRole.Student) { }

    /// <summary>
    /// Обрабатывает входящее сообщение от студента.
    /// </summary>
    /// <param name="message">Текст сообщения от студента.</param>
    public async Task ProcessMessageAsync(ITelegramBotClient botClient, long chatId, string message)
    {
      if (string.IsNullOrEmpty(message))
      {
        return;
      }
      if (_answerHomeWorks.ContainsKey(chatId))
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

      else if (message.ToLower().Contains("/start"))
      {
        var keyboard = new InlineKeyboardMarkup(new[]
        {
          new []
          {
            InlineKeyboardButton.WithCallbackData("Статусы домашних работ", "/homeWork"),
          }
        });
        await TelegramBotHandler.SendMessageAsync(botClient, chatId, "Выберите функцию:", keyboard);

      }
    }

    /// <summary>
    /// Обработка Callback запросов от студента.
    /// </summary>
    /// <param name="callbackData"></param>
    public async Task ProcessCallbackAsync(ITelegramBotClient botClient, long chatId, string callbackData, int messageId)
    {
      if (string.IsNullOrEmpty(callbackData))
      {
        return;
      }

      else if (callbackData.Contains("/homeWork_"))
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
      else if (callbackData.Contains("/homeWork"))
      {
        var keyboard = new InlineKeyboardMarkup(new[]
        {
         new []
         {
           InlineKeyboardButton.WithCallbackData("Не отправленные", "/homeWork_unfulfilled"),
         },
         new []
         {
           InlineKeyboardButton.WithCallbackData("В доработке", "/homeWork_needsRevision")
         }
       });

        await TelegramBotHandler.SendMessageAsync(botClient, chatId, "Выберите статус домашнего задания:", keyboard, messageId);
      }
      else if (callbackData.Contains("/start"))
      {
        await botClient.DeleteMessageAsync(chatId, messageId);
        await ProcessMessageAsync(botClient, chatId, callbackData);
      }
    }

    /// <summary>
    /// Проверяет статусы домашних работ.
    /// </summary>
    /// <param name="botClient"></param>
    /// <param name="chatId"></param>
    /// <param name="callbackData"></param>
    /// <param name="messageId"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
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
    /// <param name="status">Статус домашних заданий для отображения.</param>
    /// <param name="userId">Идентификатор пользователя.</param>
    /// <returns>Строка с списком домашних заданий и их статусами для конкретного пользователя.</returns>
    /// <summary>
    /// Отображает список домашних заданий с их статусами для конкретного пользователя.
    /// </summary>
    /// <param name="status">Статус домашних заданий для отображения.</param>
    /// <param name="userId">Идентификатор пользователя.</param>
    /// <returns>Строка с списком домашних заданий и их статусами для конкретного пользователя.</returns>
    private async Task DisplayHomeWorkStatuses(ITelegramBotClient botClient, long chatId, int messageId, StatusWork status, long userId)
    {
      var filteredHomeWorks = CommonHomeWork.GetHomeworkForStudent(userId)
          .Where(hw => hw.Status == status)
          .ToList();

      var student = CommonUserModel.GetUserById(userId);

      List<CallbackModel> callbackModels = new List<CallbackModel>();
      callbackModels.Add(new CallbackModel("Назад", "/homeWork"));
      if (!filteredHomeWorks.Any())
      {
        await TelegramBotHandler.SendMessageAsync(botClient, chatId, $"Нет домашних заданий со статусом '{status}' для пользователя {student.LastName} {student.FirstName}.", TelegramBotHandler.GetInlineKeyboardMarkupAsync(callbackModels), messageId);
        return;
      }
      else
      {
        foreach (var hw in filteredHomeWorks)
        {
          var homeWorkData = CommonHomeWork.GetHomeWorkById(hw.CourseId, hw.AssignmentId);

          if (homeWorkData == null)
          {
            continue;
          }
          callbackModels.Add(new CallbackModel(homeWorkData.Title, $"/homeWork_id_{hw.CourseId}_{homeWorkData.AssignmentId}"));
        }
        await TelegramBotHandler.SendMessageAsync(botClient, chatId, $"Домашние задания со статусом '{status}' для пользователя {student.LastName} {student.FirstName}:", TelegramBotHandler.GetInlineKeyboardMarkupAsync(callbackModels), messageId);
      }

    }

    private async Task DisplayActiveHomeWork(ITelegramBotClient botClient, long chatId, int messageId, string message)
    {
      var data = message.Split('_');
      int homeWorkId;
      int courseId;

      if (int.TryParse(data[data.Length - 2], out homeWorkId))
      {
        if (int.TryParse(data.Last(), out courseId))
        {
          var homeWorkData = CommonHomeWork.GetHomeWorkById(courseId, homeWorkId);
          var name = homeWorkData.Title;
          var description = homeWorkData.Description;

          StringBuilder stringBuilder = new StringBuilder();
          stringBuilder.Append(name);
          stringBuilder.Append(description);

          List<CallbackModel> callbackModels = new List<CallbackModel>();
          callbackModels.Add(new CallbackModel("Ответить", $"/homeWork_answer_{courseId}_{homeWorkData.AssignmentId}"));
          callbackModels.Add(new CallbackModel("Назад", $"/homeWork"));
          await TelegramBotHandler.SendMessageAsync(botClient, chatId, stringBuilder.ToString(), TelegramBotHandler.GetInlineKeyboardMarkupAsync(callbackModels), messageId);
        }
        else
        {
          Logger.LogError("Не удалось преобразовать courseId.");
        }
      }
      else
      {
        Logger.LogError("Не удалось преобразовать homeWorkId.");
      }
    }

    private async Task HandleHomeworkResponse(ITelegramBotClient botClient, long chatId, int messageId, string message)
    {
      var data = message.Split('_');
      int homeWorkId;
      int courseId;
      if (int.TryParse(data[data.Length - 2], out homeWorkId))
      {
        if (int.TryParse(data.Last(), out courseId))
        {
          GitHubLink gitHubLink = new GitHubLink();
          gitHubLink.CourseId = courseId;
          gitHubLink.HomeWorkId = homeWorkId;
          _answerHomeWorks.Add(chatId, gitHubLink);

          await TelegramBotHandler.SendMessageAsync(botClient, chatId, "Добавьте ссылку на ваш репозиторий GitHub для отправки на проверку.", null, messageId);
        }
        else
        {
          Logger.LogError("Не удалось преобразовать courseId.");
        }
      }
      else
      {
        Logger.LogError("Не удалось преобразовать homeWorkId.");
      }
    }

    /// <summary>
    /// Обрабатывает ссылку на GitHub.
    /// </summary>
    /// <param name="client">Клиент Telegram Bot.</param>
    /// <param name="message">Сообщение Telegram.</param>
    /// <param name="token">Токен отмены.</param>
    private async Task HandleGitHubLink(ITelegramBotClient botClient, long chatId, string message)
    {
      var user = CommonUserModel.GetUserById(chatId);
      var teachers = CommonUserModel.GetAllTeachers();
      _answerHomeWorks.TryGetValue(chatId, out GitHubLink gitLink);
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
    /// Запрашивает корректную ссылку на GitHub.
    /// </summary>
    /// <param name="client">Клиент Telegram Bot.</param>
    /// <param name="chatId">Идентификатор чата.</param>
    /// <param name="token">Токен отмены.</param>
    private async Task RequestValidGitHubLink(ITelegramBotClient botClient, long chatId)
    {
      await TelegramBotHandler.SendMessageAsync(botClient, chatId, "Пожалуйста, добавьте корректную ссылку на GitHub или нажмите 'Назад', чтобы выбрать другое задание.");
    }

    /// <summary>
    /// Проверяет, является ли текст ссылкой на GitHub.
    /// </summary>
    /// <param name="text">Текст для проверки.</param>
    /// <returns>Возвращает true, если текст является корректной ссылкой на GitHub.</returns>
    bool IsGitHubLink(string text)
    {
      var githubLinkPattern = @"^(https?:\/\/)?(www\.)?github\.com\/[\w\-]+\/[\w\-]+(\/.*)?$";
      return Regex.IsMatch(text, githubLinkPattern, RegexOptions.IgnoreCase);
    }
  }
}
