using Telegram.Bot.Types;
using Telegram.Bot;
using HomeWorkTelegramBot.Models;
using HomeWorkTelegramBot.Core;
using System.Text;
using Telegram.Bot.Types.ReplyMarkups;
using static HomeWorkTelegramBot.Config.Logger;
using SQLitePCL;

namespace HomeWorkTelegramBot.Bot.Function.Teacher
{
  /// <summary>
  /// Выставление оценки домашнему заданию студента.
  /// </summary>
  internal class RateTaskWork
  {
    private static readonly Dictionary<long, Answer> _answerData = new Dictionary<long, Answer>();
    public static readonly Dictionary<long, List<string>> _comentsData = new Dictionary<long, List<string>>();
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
      /// Этап добавления комментариев к ответа.
      /// </summary>
      AddComment,

      /// <summary>
      /// Этап подтверждения сохранения ответа.
      /// </summary>
      SaveAnswerConfirmation,

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

      if (data == "/start" && _userSteps.ContainsKey(chatId))
      {
        var messageData = $"Обновление данных об ответе на задание с id {_answerData[chatId].Id} прервано";
        await CompleteAnswerUpdate(botClient, chatId, taskId, callbackQuery.Message.MessageId, messageData);
      }

      var currentStep = _userSteps[chatId];
      var answer = _answerData[chatId];

      switch (currentStep)
      {
        case UpdateAnswerStatus.ChoseAnswerStatus:
          await HandleAnswerSelection(botClient, callbackQuery);
          break;

        case UpdateAnswerStatus.SaveAnswerConfirmation:
          await HandleSaveOrAddComment(botClient, callbackQuery);
          break;

        case UpdateAnswerStatus.AddComment:
          await HandleAddComment(botClient, callbackQuery);
          break;

        case UpdateAnswerStatus.Completed:
          var messageData = $"Данные об ответе на задание с id {answer.Id} изменены";
          await CompleteAnswerUpdate(botClient, chatId, _answerData[chatId].TaskId, callbackQuery.Message.MessageId, messageData);
          break;
      }
    }

    /// <summary>
    /// Процесс выбора задания курса для просмотра статистики их выполнения студентами.
    /// </summary>
    /// <param name="botClient">Клиент telegram-бота.</param>
    /// <param name="callbackQuery">Callback-запрос, полученный от пользователя.</param>
    /// <returns>Асинхронная задача, представляющая процесс обработки.</returns>
    public static async Task ProcessUpdateAnswer(ITelegramBotClient botClient, Message message, int taskId)
    {
      long chatId = message.From.Id;
      string data = message.Text;

      if (data == "/start" && _userSteps.ContainsKey(chatId))
      {
        var messageData = $"Обновление данных об ответе на задание с id {_answerData[chatId].Id} прервано";
        await CompleteAnswerUpdate(botClient, chatId, taskId, message.MessageId, messageData);
      }

      var currentStep = _userSteps[chatId];
      var answer = _answerData[chatId];

      switch (currentStep)
      {
        case UpdateAnswerStatus.AddComment:
          await HandleAddComment(botClient, message);
          break;

      }
    }

    /// <summary>
    /// Обрабатывает нажатие выбранной кнопки с оценкой задания.
    /// </summary>
    /// <param name="botClient">Экземпляр клиента Telegram бота.</param>
    /// <param name="callbackQuery">Callback-запрос, полученный от пользователя.</param>
    /// <returns>Асинхронная задача, представляющая процесс обработки.</returns>
    private static async Task HandleSaveOrAddComment(ITelegramBotClient botClient, CallbackQuery callbackQuery)
    {
      long chatId = callbackQuery.From.Id;
      string data = callbackQuery.Data;
      int messageId = callbackQuery.Message.MessageId;
      int answerId = -1;
      if (data.StartsWith("/save_"))
      {
        answerId = int.Parse(data.Replace("/save_", string.Empty));
        await TrySaveAnswerMessage(botClient, answerId, chatId, messageId);
      }

      if (data.StartsWith("/addcomment_"))
      {
        answerId = int.Parse(data.Replace("/addcomment_", string.Empty));
        _userSteps[chatId] = UpdateAnswerStatus.AddComment;
        await HandleAddComment(botClient, callbackQuery);
      }
    }

    /// <summary>
    /// Обрабатывает нажатие выбранной кнопки с оценкой задания.
    /// </summary>
    /// <param name="botClient">Экземпляр клиента Telegram бота.</param>
    /// <param name="callbackQuery">Callback-запрос, полученный от пользователя.</param>
    /// <returns>Асинхронная задача, представляющая процесс обработки.</returns>
    private static async Task HandleAnswerSelection(ITelegramBotClient botClient, CallbackQuery callbackQuery)
    {
      long chatId = callbackQuery.From.Id;
      string data = callbackQuery.Data;
      int messageId = callbackQuery.Message.MessageId;
      int answerId = -1;
      var status = Answer.TaskStatus.IncorrectAnswer;
      if (data.StartsWith("/correct_"))
      {
        answerId = int.Parse(data.Replace("/correct_", string.Empty));
        status = Answer.TaskStatus.CorrectAnswer;
      }

      if (data.StartsWith("/incorrect_"))
      {
        answerId = int.Parse(data.Replace("/incorrect_", string.Empty));
      }

      SendConfirmationMessage(botClient, chatId, answerId, status, callbackQuery.Message.MessageId);
      _userSteps[chatId] = UpdateAnswerStatus.SaveAnswerConfirmation;
    }

    /// <summary>
    /// Обрабатывает добавление нового комментария к ответу на задание.
    /// </summary>
    /// <param name="botClient">Экземпляр клиента Telegram бота.</param>
    /// <param name="callbackQuery">Callback-запрос, полученный от пользователя.</param>
    /// <returns>Асинхронная задача, представляющая процесс обработки.</returns>
    private static async Task HandleAddComment(ITelegramBotClient botClient, CallbackQuery callbackQuery)
    {
      string data = callbackQuery.Data;
      int answerId = -1;
      long chatId = callbackQuery.From.Id;
      int messageId = callbackQuery.Message.MessageId;

      if (data.StartsWith("/save_"))
      {
        answerId = int.Parse(data.Replace("/save_", string.Empty));
        await TrySaveAnswerMessage(botClient, answerId, chatId, messageId);
      }

      if (data.StartsWith("/addcomment_"))
      {
        string messageData = "Введите текст комментария";
        if (!_comentsData.ContainsKey(chatId))
        {
          _comentsData.Add(chatId, new List<string>());
        }

        await TelegramBotHandler.SendMessageAsync(botClient, chatId, messageData, null, messageId);
      }
    }

    private static async Task TrySaveAnswerMessage(ITelegramBotClient botClient, int answerId, long chatId, int messageId)
    {
      _userSteps[chatId] = UpdateAnswerStatus.Completed;
      await TryChangeAnswerStatus(botClient, chatId, answerId, _answerData[chatId].Status);
      var messageData = $"Данные об ответе на задание с id {answerId} изменены";
      await CompleteAnswerUpdate(botClient, chatId, _answerData[chatId].TaskId, messageId, messageData);
    }

    /// <summary>
    /// Обрабатывает добавление нового комментария к ответу на задание.
    /// </summary>
    /// <param name="botClient">Экземпляр клиента Telegram бота.</param>
    /// <param name="message">Сообщение, полученное от пользователя.</param>
    /// <returns>Асинхронная задача, представляющая процесс обработки.</returns>
    private static async Task HandleAddComment(ITelegramBotClient botClient, Message message)
    {
      long chatId = message.From.Id;
      int messageId = message.MessageId;
      string messageData = message.Text;

      if (!_comentsData.ContainsKey(chatId))
      {
        _comentsData.Add(chatId, new List<string>());
      }

      _comentsData[chatId].Add(messageData);
      InlineKeyboardMarkup keyboard = GetCommentsKeyboard(chatId);

      await TelegramBotHandler.SendMessageAsync(botClient, chatId, "Добавить еще один комментарий?", keyboard);
    }

    private static InlineKeyboardMarkup GetCommentsKeyboard(long chatId)
    {
      var callbacks = new List<CallbackModel>
      {
        new CallbackModel("Сохранить ответ", $"/save_{_answerData[chatId].Id}"),
        new CallbackModel("Добавить комментарий", $"/addcomment_{_answerData[chatId].Id}"),
        new CallbackModel("В главное меню", "/start"),
      };
      var keyboard = TelegramBotHandler.GetInlineKeyboardMarkupAsync(callbacks);
      return keyboard;
    }

    /// <summary>
    /// Пытается изменить статус ответа.
    /// </summary>
    /// <param name="chatId">Идентификатор чата преподавателя.</param>
    /// <param name="answerId">Идентификатор ответа на задание.</param>
    /// <param name="status">Новый статус ответа на задание.</param>
    private static async void SendConfirmationMessage(ITelegramBotClient botClient, long chatId, int answerId, Answer.TaskStatus status, int messageId)
    {
      if (_answerData[chatId].Id == answerId && answerId != -1)
      {
        _answerData[chatId].Status = status;
        var statusStr = EnumExtentions.GetDescription(_answerData[chatId].Status);

        LogInformation($"Статус ответа на задание с id {answerId} изменен на {statusStr}" +
          $" преподавателем с ChatId {chatId}. Ожидается подтверждение действия.");

        var student = UserService.GetUserByChatId(_answerData[chatId].UserId);
        var messageData = GetMessageData(student, chatId, _answerData[chatId].TaskId);
        InlineKeyboardMarkup keyboard = GetCommentsKeyboard(chatId);

        messageData = $"{messageData}\nОценка: {statusStr}";
        await TelegramBotHandler.SendMessageAsync(botClient, chatId, messageData, keyboard, messageId);
      }
      else
      {
        LogWarning($"Статус ответа на задание не был изменен");
      }
    }

    /// <summary>
    /// Пытается изменить статус ответа.
    /// </summary>
    /// <param name="chatId">Идентификатор чата преподавателя.</param>
    /// <param name="answerId">Идентификатор ответа на задание.</param>
    /// <param name="status">Новый статус ответа на задание.</param>
    private static async Task TryChangeAnswerStatus(ITelegramBotClient botClient, long chatId, int answerId, Answer.TaskStatus status)
    {
      if (_answerData[chatId].Id == answerId && answerId != -1)
      {
        _answerData[chatId].Status = status;
        AnswerService.UpdateAnswer(_answerData[chatId]);
        LogInformation($"Статус ответа на задание с id {answerId} изменен на {EnumExtentions.GetDescription(_answerData[chatId].Status)}" +
          $" преподавателем с ChatId {chatId}");
        await SendNotificationToStudent(botClient, chatId);
        await SendNotificationToTeacher(botClient, chatId);
      }
      else
      {
        LogWarning($"Статус ответа на задание не был изменен");
      }
    }

    /// <summary>
    /// Отправляет студенту уведомление о том, что ответ на задание был проверен.
    /// </summary>
    /// <param name="botClient">Экземпляр клиента Telegram бота.</param>
    /// <param name="chatId">Уникальный идентификатор чата преподавателя.</param>
    /// <returns>Асинхронная задача, представляющая процесс обработки.</returns>
    private static async Task SendNotificationToStudent(ITelegramBotClient botClient, long chatId)
    {
      var task = TaskWorkService.GetTaskWorkById(_answerData[chatId].TaskId);
      if (task != null)
      {
        var messageText = GetMessageData(chatId, task);
        var student = UserService.GetUserByChatId(_answerData[chatId].UserId);
        if (student != null)
        {
          await TelegramBotHandler.SendMessageAsync(botClient, student.ChatId, messageText);
        }
      }
    }

    /// <summary>
    /// Отправляет преподавателю итоговый вариант сообщения-резульатата проверки задания студента.
    /// </summary>
    /// <param name="botClient">Экземпляр клиента Telegram бота.</param>
    /// <param name="chatId">Уникальный идентификатор чата преподавателя.</param>
    /// <returns>Асинхронная задача, представляющая процесс обработки.</returns>
    private static async Task SendNotificationToTeacher(ITelegramBotClient botClient, long chatId)
    {
      var foundStudent = UserService.GetUserByChatId(_answerData[chatId].UserId); // student.UserId - int user.id
      if (foundStudent != null)
      {
        var messageText = GetMessageData(foundStudent, chatId, _answerData[chatId].TaskId);
        messageText.Insert(0, "Задание проверено: \n\n");
        var sb = new StringBuilder();
        if (_comentsData != null && _comentsData.ContainsKey(chatId))
        {
          GetCommentsData(chatId, sb);
        }
        messageText.Insert(messageText.Length - 1, sb.ToString());
        await TelegramBotHandler.SendMessageAsync(botClient, chatId, messageText);
      }
    }

    /// <summary>
    /// Формирует текст сообщения для студента.
    /// </summary>
    /// <param name="chatId">Уникальный идентификатор чата преподавателя.</param>
    /// <param name="task">Объект класса taskWork, представляющий собой задание.</param>
    /// <returns>Текст сообщения, которое нужно отправить пользователю.</returns>
    private static string GetMessageData(long chatId, TaskWork task)
    {
      var status = _answerData[chatId].Status;
      var sb = new StringBuilder();
      sb.AppendLine($"Задание \"{task.Name}\" проверено. \nСтатус задания: {EnumExtentions.GetDescription(status)}");
      if (status == Answer.TaskStatus.IncorrectAnswer)
      {
        sb.AppendLine("Задание требует доработки");
        sb.AppendLine($"Текст ответа: {_answerData[chatId].AnswerText}");
      }

      if (_comentsData != null && _comentsData.ContainsKey(chatId))
      {
        GetCommentsData(chatId, sb);
      }

      return sb.ToString();
    }

    private static void GetCommentsData(long chatId, StringBuilder sb)
    {
      if (_comentsData[chatId].Count > 0)
      {
        var i = 1;
        sb.AppendLine($"Комментарии преподавателя: ");
        foreach (var comment in _comentsData[chatId])
        {
          sb.AppendLine($"{i}. {comment}");
          i++;
        }
      }
    }

    /// <summary>
    /// Инициализирует процесс получения статистики выполнения заданий студентом.
    /// </summary>
    /// <param name="botClient">Экземпляр клиента Telegram бота.</param>
    /// <param name="chatId">Уникальный идентификатор чата пользователя.</param>
    /// <param name="taskId">Уникальный идентификатор задания.</param>
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
        await GetAvailableActions(botClient, callbackQuery, taskId, user, foundAnswers);
      }
    }

    /// <summary>
    /// Формирует клавиатуру с доступными действиями для найденных ответов.
    /// </summary>
    /// <param name="botClient">Экземпляр клиента Telegram бота.</param>
    /// <param name="callbackQuery">Callback-запрос, полученный от пользователя.</param>
    /// <param name="taskId">Уникальный идентификатор задания.</param>
    /// <param name="user">Студент, чей ответ на задание нужно получить.</param>
    /// <param name="foundAnswers">Найденные ответы выбранного студента.</param>
    /// <returns>Асинхронная задача, представляющая процесс обработки.</returns>
    private static async Task GetAvailableActions(ITelegramBotClient botClient, CallbackQuery callbackQuery, int taskId, Models.User user, List<Answer> foundAnswers)
    {
      var chatId = callbackQuery.From.Id;
      var messageId = callbackQuery.Message.MessageId;
      var answer = foundAnswers
                  .Where(a => a.UserId == user.ChatId)
                  .FirstOrDefault();

      if (answer != null)
      {
        _answerData[chatId] = answer;
        LogInformation($"Начало обновления статуса ответа на задание с id {taskId} преподавателем с ChatId {chatId}");
        var messageData = GetMessageData(user, chatId, taskId);
        messageData = $"{messageData}\nОцените ответ:";
        InlineKeyboardMarkup keyboard = GetActionsKeyboard(chatId);
        await TelegramBotHandler.SendMessageAsync(botClient, chatId, messageData, keyboard, messageId);
      }
      else
      {
        var keyboard = TelegramBotHandler.GetInlineKeyboardMarkupAsync(new CallbackModel("В главное меню", "/start"));
        await TelegramBotHandler.SendMessageAsync(botClient, chatId, "Не найден ответ выбранного пользователя", keyboard, messageId);
      }
    }

    /// <summary>
    /// Создает клавиатуру с действиями для оценки задания студента. 
    /// </summary>
    /// <param name="chatId">Уникальный идентификатор чата преподавателя.</param>
    /// <returns>Возвращает Inline-клавиатуру.</returns>
    private static InlineKeyboardMarkup GetActionsKeyboard(long chatId)
    {
      var callbackModels = new List<CallbackModel>
          {
          new ("Правильный ответ", $"/correct_{_answerData[chatId].Id}"),
          new ("Неправильный ответ", $"/incorrect_{_answerData[chatId].Id}"),
          new ("В главное меню", "/start"),
          };
      var keyboard = TelegramBotHandler.GetInlineKeyboardMarkupAsync(callbackModels);
      return keyboard;
    }

    /// <summary>
    /// Получает данные о выбранном студенте.
    /// </summary>
    /// <param name="data">Данные, полученные из callback query.</param>
    /// <returns>Объект класса Models.User, представляющий собой выбранного студента.</returns>
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
        long userId = long.Parse(data.Replace("/useransw_", string.Empty));
        user = UserService.GetUserByChatId(userId);
      }

      return user;
    }

    /// <summary>
    /// Формирует сообщение с данными о непроверенном задании студента.
    /// </summary>
    /// <param name="user">Студент.</param>
    /// <param name="chatId">Уникальный идентификатор чата преподавателя.</param>
    /// <param name="taskId">Уникальный идентификатор задания.</param>
    /// <returns>Строка с текстом сообщения.</returns>
    private static string GetMessageData(Models.User user, long chatId, int taskId)
    {
      var sb = new StringBuilder();
      var answer = _answerData[chatId];
      var task = TaskWorkService.GetTaskWorkById(taskId);
      sb.AppendLine($"Название задания: {task.Name}");
      sb.AppendLine($"Студент: {user.Surname} {user.Name}");
      sb.AppendLine($"Текст ответа: {answer.AnswerText}");
      sb.AppendLine($"Дата загрузки ответа: {answer.Date.ToShortDateString()}");

      return sb.ToString();
    }

    /// <summary>
    /// Завершает создание нового задания.
    /// </summary>
    /// <param name="botClient">Экземпляр клиента Telegram бота.</param>
    /// <param name="chatId">Уникальный идентификатор чата пользователя.</param>
    /// <returns>Асинхронная задача, представляющая процесс обработки.</returns>
    private static async Task CompleteAnswerUpdate(ITelegramBotClient botClient, long chatId, int taskId, int messageId, string messageData)
    {
      if (_answerData.TryGetValue(chatId, out var answer))
      {
        LogInformation($"{messageData} преподавателем с ChatId {chatId}");
        var callbackModels = new CallbackModel("В главное меню", "/start");
        var keyboard = TelegramBotHandler.GetInlineKeyboardMarkupAsync(callbackModels);
        //await TelegramBotHandler.SendMessageAsync(botClient, chatId, messageData, keyboard, messageId);
        await ClearData();
      }
    }

    /// <summary>
    /// Очищает временные данные.
    /// </summary>
    /// <returns>Асинхронная задача, представляющая процесс обработки.</returns>
    public static async Task ClearData()
    {
      _answerData.Clear();
      _comentsData.Clear();
      _userSteps.Clear();
    }
  }
}
