using Telegram.Bot.Types;
using Telegram.Bot;
using HomeWorkTelegramBot.Models;
using HomeWorkTelegramBot.Core;
using System.Text;
using static HomeWorkTelegramBot.Config.Logger;

namespace HomeWorkTelegramBot.Bot.Function.Teacher
{
  /// <summary>
  /// Выставление оценки домашнему заданию студента.
  /// </summary>
  internal class RateTaskWork
  {
    private static readonly Dictionary<long, Answer> _answerData = new Dictionary<long, Answer>();
    private static readonly Dictionary<long, UpdateAnswerStatus> _userSteps = new Dictionary<long, UpdateAnswerStatus>();

    /// <summary>
    /// Этапы создания задания.
    /// </summary>
    private enum UpdateAnswerStatus
    {
      /// <summary>
      /// Этап изменения статуса ответа.
      /// </summary>
      ChoseAnswerStatus,

      /// <summary>
      /// Процесс выбора завершен.
      /// </summary>
      Completed,
    }

    /// <summary>
    /// Процесс выбора задания курса для просмотра статистики их выполнения студентами.
    /// </summary>
    /// <param name="botClient">Клиент telegram-бота.</param>
    /// <param name="callbackQuery">Callback-запрос, полученный от пользователя.</param>
    /// <returns>Асинхронная задача, представляющая процесс обработки.</returns>
    public static async Task ProcessUpdateAnswer(ITelegramBotClient botClient, CallbackQuery callbackQuery, int taskId)
    {
      
      long chatId = callbackQuery.From.Id;
      string data = callbackQuery.Data;
      if (taskId == -1)
      {
        taskId = _answerData[chatId].TaskId;
      }

      if (!_userSteps.ContainsKey(chatId))
      {
        await InitializeUpdateAnswer(botClient, chatId, callbackQuery, taskId);
        return;
      }

      var currentStep = _userSteps[chatId];
      var answer = _answerData[chatId];

      switch (currentStep)
      {
        case UpdateAnswerStatus.ChoseAnswerStatus:
          await HandleAnswerSelection(botClient, callbackQuery);
          currentStep = UpdateAnswerStatus.Completed;
          break;

        case UpdateAnswerStatus.Completed:
          await CompleteAnswerUpdate(botClient, chatId, _answerData[chatId].TaskId, callbackQuery.Message.MessageId);
          break;
      }
    }

    private static async Task HandleAnswerSelection(ITelegramBotClient botClient, CallbackQuery callbackQuery)
    {
      long chatId = callbackQuery.From.Id;
      string data = callbackQuery.Data;
      int messageId = callbackQuery.Message.MessageId;
      int answerId = -1;
      if (data.StartsWith("/correct_"))
      {
        answerId = int.Parse(data.Replace("/correct_", string.Empty));
      }

      if (data.StartsWith("/incorrect_"))
      {
        answerId = int.Parse(data.Replace("/incorrect_", string.Empty));
      }

      if (_answerData[chatId].Id == answerId && answerId != -1)
      {
        _answerData[chatId].Status = Answer.TaskStatus.IncorrectAnswer;
        AnswerService.UpdateAnswer(_answerData[chatId]);
        LogInformation($"Статус ответа на вопрос с id {answerId} изменен на {_answerData[chatId].Status} преподавателем с ChatId {chatId}");
      }

      LogWarning($"Статус ответа на вопрос не был изменен");
      await CompleteAnswerUpdate(botClient, chatId, _answerData[chatId].TaskId, messageId);
    }

    /// <summary>
    /// Инициализирует процесс получения статистики выполнения заданий студентом.
    /// </summary>
    /// <param name="botClient">Экземпляр клиента Telegram бота.</param>
    /// <param name="chatId">Уникальный идентификатор чата пользователя.</param>
    /// <returns>Асинхронная задача, представляющая процесс обработки.</returns>
    private static async Task InitializeUpdateAnswer(ITelegramBotClient botClient, long chatId, CallbackQuery callbackQuery, int taskId)
    {
      int messageId = callbackQuery.Message.MessageId;
      var user = GetUser(callbackQuery.Data);
      _userSteps[chatId] = UpdateAnswerStatus.ChoseAnswerStatus;
      _answerData[chatId] = new Answer();
      _answerData[chatId].TaskId = taskId;
      var foundAnswers = AnswerService.GetAnswersByTaskId(taskId);
      if (foundAnswers != null && foundAnswers.Count > 0)
      {
        await GetAvailableActions(botClient, chatId, taskId, messageId, user, foundAnswers);
      }
    }

    private static async Task GetAvailableActions(ITelegramBotClient botClient, long chatId, int taskId, int messageId, Models.User user, List<Answer> foundAnswers)
    {
      var answer = foundAnswers
                  .Where(a => a.UserId == user.ChatId)
                  .FirstOrDefault();

      if (answer != null)
      {
        _answerData[chatId] = answer;
        LogInformation($"Начало обновления статуса ответа на задание с id {taskId} преподавателем с ChatId {chatId}");
        var messageData = GetMessageData(user, chatId, taskId);
        var callbackModels = new List<CallbackModel>
          {
          new ("Правильный ответ", $"/correct_{_answerData[chatId].Id}"),
          new ("Неправильный ответ", $"/incorrect_{_answerData[chatId].Id}"),
          new ("В главное меню", "/menu"),
          };
        var keyboard = TelegramBotHandler.GetInlineKeyboardMarkupAsync(callbackModels);
        await TelegramBotHandler.SendMessageAsync(botClient, chatId, messageData, keyboard, messageId);
      }
      else
      {
        var keyboard = TelegramBotHandler.GetInlineKeyboardMarkupAsync(new CallbackModel("В главное меню", "/menu"));
        await TelegramBotHandler.SendMessageAsync(botClient, chatId, "Не найден ответ выбранного пользователя", keyboard, messageId);
      }
    }

    private static Models.User GetUser(string data)
    {
      Models.User user = new Models.User();
      if (data.StartsWith("/selectanswer"))
      {
        int answerId = int.Parse(data.Replace("/selectanswer_", string.Empty));
        var foundAnswer = AnswerService.GetAnswerById(answerId);
        if (foundAnswer != null)
        {
          user = UserService.GetUserByChatId(foundAnswer.UserId);
        }
      }

      if (data.StartsWith("/useransw"))
      {
        long userId = int.Parse(data.Replace("/useransw_", string.Empty));
        user = UserService.GetUserByChatId(userId);
      }

      return user;
    }

    private static string GetMessageData(Models.User user, long chatId, int taskId)
    {
      var sb = new StringBuilder();
      var answer = _answerData[chatId];
      var task = TaskWorkService.GetTaskWorkById(taskId);
      sb.AppendLine($"{task.Name}");
      sb.AppendLine($"{user.Surname} {user.Name}");
      sb.AppendLine(answer.AnswerText);
      sb.AppendLine(answer.Date.ToShortDateString());
      sb.AppendLine("\nОцените ответ");

      return sb.ToString();
    }

    /// <summary>
    /// Завершает создание нового задания.
    /// </summary>
    /// <param name="botClient">Экземпляр клиента Telegram бота.</param>
    /// <param name="chatId">Уникальный идентификатор чата пользователя.</param>
    /// <returns>Асинхронная задача, представляющая процесс обработки.</returns>
    private static async Task CompleteAnswerUpdate(ITelegramBotClient botClient, long chatId, int taskId, int messageId)
    {
      if (_answerData.TryGetValue(chatId, out var task))
      {
        var messageData = $"Данные об ответе на задание с id {task.Id} изменены";
        LogInformation($"{messageData} преподавателем с ChatId {chatId}");
        var callbackModels = new CallbackModel("В главное меню", "/menu");
        var keyboard = TelegramBotHandler.GetInlineKeyboardMarkupAsync(callbackModels);
        await TelegramBotHandler.SendMessageAsync(botClient, chatId, messageData, keyboard, messageId);
        _answerData.Remove(chatId);
        _userSteps.Remove(chatId);
      }
    }
  }
}
