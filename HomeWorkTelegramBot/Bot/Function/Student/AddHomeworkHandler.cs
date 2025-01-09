using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using HomeWorkTelegramBot.Core;
using HomeWorkTelegramBot.Models;
using System.Text;
using HomeWorkTelegramBot.Bot.Function.GitUtilities;
using System.IO;

namespace HomeWorkTelegramBot.Bot.Function.Student
{
  internal class AddHomeworkHandler
  {
    // Словарь для хранения taskId для каждого пользователя
    private static readonly Dictionary<long, int> UserTaskMap = new Dictionary<long, int>();

    // Словарь для хранения текста ответа для каждого пользователя
    private static readonly Dictionary<long, string> UserAnswerMap = new Dictionary<long, string>();

    /// <summary>
    /// Запрашивает у пользователя ввод ответа на домашнее задание.
    /// Сохраняет идентификатор задания в словаре, чтобы использовать его позже при обработке ответа.
    /// </summary>
    /// <param name="botClient">Клиент Telegram-бота для отправки сообщений.</param>
    /// <param name="callbackQuery">Запрос обратного вызова, содержащий данные о пользователе и команде.</param>
    /// <returns>Задача, представляющая асинхронную операцию.</returns>
    public async Task RequestAnswerAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery)
    {
      string data = callbackQuery.Data;
      int taskId = int.Parse(data.Replace("/addAnswer_id", ""));
      UserTaskMap[callbackQuery.From.Id] = taskId;
      await TelegramBotHandler.SendMessageAsync(botClient, callbackQuery.From.Id, "Пожалуйста, введите ваш ответ:", null, callbackQuery.Message.Id);
    }

    /// <summary>
    /// Запрашивает у пользователя ввод ответа на домашнее задание и отображает кнопки "Сохранить" и "Отменить".
    /// </summary>
    /// <param name="botClient">Клиент Telegram-бота для отправки сообщений.</param>
    /// <param name="message">Сообщение, содержащее текст ответа от пользователя.</param>
    /// <returns>Задача, представляющая асинхронную операцию.</returns>
    public async Task RequestAnswerConfirmationAsync(ITelegramBotClient botClient, Message message)
    {
      if (UserTaskMap.TryGetValue(message.From.Id, out int taskId))
      {
        UserAnswerMap[message.From.Id] = message.Text;
        var taskWork = TaskWorkService.GetTaskWorkById(taskId);
        var course = CourseService.GetCourseById(taskWork.CourseId);
        var courseEnrollment = CourseEnrollmentService.GetCourseEnrollmentByCourseAndUser(course.Id, message.Chat.Id);
        var answer = AnswerService.GetAnswerByChatIdAndTaskId(message.Chat.Id, taskId);

        var messageLast = await SendProcessingMessageAsync(botClient, message.From.Id);
        var result = new GitRepositoryManager().CloneAndPrepareRepository(message.Text, courseEnrollment, answer);

        var responseMessage = BuildResponseMessage(taskWork, message.Text, result);
        var callbackModels = GetCallbackModels(result, taskId);

        await Task.Delay(1000);
        await botClient.DeleteMessageAsync(message.From.Id, messageLast.MessageId);

        if (result == ErrorCode.CompiletedError)
        {
          await SendErrorFileAsync(botClient, message.From.Id, course, taskWork);
        }

        await TelegramBotHandler.SendMessageAsync(botClient, message.From.Id, responseMessage, TelegramBotHandler.GetInlineKeyboardMarkupAsync(callbackModels));
      }
      else
      {
        await TelegramBotHandler.SendMessageAsync(botClient, message.From.Id, "Ошибка: не удалось найти задание. Пожалуйста, попробуйте снова.");
      }
    }

    /// <summary>
    /// Отправляет сообщение о начале проверки ответа.
    /// </summary>
    /// <param name="botClient">Клиент Telegram-бота для отправки сообщений.</param>
    /// <param name="chatId">Идентификатор чата пользователя.</param>
    /// <returns>Задача, представляющая асинхронную операцию отправки сообщения.</returns>
    private async Task<Message> SendProcessingMessageAsync(ITelegramBotClient botClient, long chatId)
    {
      return await TelegramBotHandler.SendMessageAsync(botClient, chatId, "Идёт проверка ответа. Пожалуйста, подождите...");
    }

    /// <summary>
    /// Формирует сообщение с результатом проверки.
    /// </summary>
    /// <param name="taskWork">Информация о задании.</param>
    /// <param name="userAnswer">Ответ пользователя.</param>
    /// <param name="result">Результат проверки.</param>
    /// <returns>Строка с сообщением для пользователя.</returns>
    private string BuildResponseMessage(TaskWork taskWork, string userAnswer, ErrorCode result)
    {
      string statusEmoji = result != ErrorCode.None ? "🔴" : "🟢";
      var stringBuilder = new StringBuilder();
      stringBuilder.AppendLine($"Задание: {taskWork.Name}");
      stringBuilder.AppendLine($"\r\nОписание: {taskWork.Description}");
      stringBuilder.AppendLine($"\r\nВаш ответ: {userAnswer}");
      stringBuilder.AppendLine($"\r\n{statusEmoji} Статус проверки: <b>{GitLinkValidator.GetErrorMessage(result)}</b>");
      stringBuilder.AppendLine($"\r\nВы хотите сохранить этот ответ?");
      return stringBuilder.ToString();
    }

    /// <summary>
    /// Получает модели обратного вызова в зависимости от результата проверки.
    /// </summary>
    /// <param name="result">Результат проверки.</param>
    /// <param name="taskId">Идентификатор задания.</param>
    /// <returns>Список моделей обратного вызова.</returns>
    private List<CallbackModel> GetCallbackModels(ErrorCode result, int taskId)
    {
      if (result != ErrorCode.None && result != ErrorCode.CompiletedError)
      {
        return null;
      }

      return new List<CallbackModel>
      {
          new CallbackModel("Сохранить", $"/saveAnswer_{taskId}"),
          new CallbackModel("Отменить", $"/cancelAnswer"),
      };
    }

    /// <summary>
    /// Отправляет файл с ошибками пользователю, если он существует.
    /// </summary>
    /// <param name="botClient">Клиент Telegram-бота для отправки сообщений.</param>
    /// <param name="chatId">Идентификатор чата пользователя.</param>
    /// <param name="course">Информация о курсе.</param>
    /// <param name="taskWork">Информация о задании.</param>
    /// <returns>Задача, представляющая асинхронную операцию отправки файла.</returns>
    private async Task SendErrorFileAsync(ITelegramBotClient botClient, long chatId, Courses course, TaskWork taskWork)
    {
      var user = UserService.GetUserByChatId(chatId);
      string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "HomeWorkAnswers", course.Name, $"{user.Name} {user.Surname} {user.Lastname}", $"{taskWork.Name}.txt");

      Console.WriteLine($"Путь к файлу: {filePath}");

      if (System.IO.File.Exists(filePath))
      {
        try
        {
          using (var errorStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
          {
            var errorInputFile = InputFile.FromStream(errorStream);

            await botClient.SendDocument(
                chatId: user.ChatId,
                document: errorInputFile,
                caption: "Файл с ошибками");
          }
        }
        catch (Exception ex)
        {
          Console.WriteLine($"Ошибка при открытии файла: {ex.Message}");
          await TelegramBotHandler.SendMessageAsync(botClient, user.ChatId, "Не удалось открыть файл с ошибками. Пожалуйста, попробуйте позже.");
        }
      }
      else
      {
        Console.WriteLine("Файл не найден.");
        await TelegramBotHandler.SendMessageAsync(botClient, user.ChatId, "Файл с ошибками не найден.");
      }
    }

    /// <summary>
    /// Обрабатывает выбор пользователя и сохраняет ответ, если выбрано "Сохранить".
    /// </summary>
    /// <param name="botClient">Клиент Telegram-бота для отправки сообщений.</param>
    /// <param name="callbackQuery">Запрос обратного вызова, содержащий данные о пользователе и команде.</param>
    /// <returns>Задача, представляющая асинхронную операцию.</returns>
    public async Task ProcessAnswerAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery)
    {
      string data = callbackQuery.Data;
      var callbackModels = GetDefaultCallbackModels();

      if (data.StartsWith("/saveAnswer_"))
      {
        await SaveAnswerAsync(botClient, callbackQuery, data, callbackModels);
      }
      else if (data == "/cancelAnswer")
      {
        await CancelAnswerAsync(botClient, callbackQuery, callbackModels);
      }
    }

    /// <summary>
    /// Получает стандартные модели обратного вызова.
    /// </summary>
    /// <returns>Список моделей обратного вызова.</returns>
    private List<CallbackModel> GetDefaultCallbackModels()
    {
      return new List<CallbackModel>
      {
          new CallbackModel("Вернуться к списку домашних работ", $"/viewHomework"),
          new CallbackModel("На главную", $"/start"),
      };
    }

    /// <summary>
    /// Сохраняет ответ пользователя, если выбрано "Сохранить".
    /// </summary>
    /// <param name="botClient">Клиент Telegram-бота для отправки сообщений.</param>
    /// <param name="callbackQuery">Запрос обратного вызова, содержащий данные о пользователе и команде.</param>
    /// <param name="data">Данные из запроса обратного вызова.</param>
    /// <param name="callbackModels">Список моделей обратного вызова.</param>
    /// <returns>Задача, представляющая асинхронную операцию.</returns>
    private async Task SaveAnswerAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery, string data, List<CallbackModel> callbackModels)
    {
      int taskId = int.Parse(data.Replace("/saveAnswer_", ""));
      if (UserTaskMap.TryGetValue(callbackQuery.From.Id, out int storedTaskId) && storedTaskId == taskId)
      {
        if (UserAnswerMap.TryGetValue(callbackQuery.From.Id, out string answerText))
        {
          var answer = AnswerService.GetAnswerByChatIdAndTaskId(callbackQuery.From.Id, taskId);
          answer.AnswerText = answerText;
          answer.Status = Answer.TaskStatus.Answered;

          AnswerService.UpdateAnswer(answer);

          var taskWork = TaskWorkService.GetTaskWorkById(taskId);

          StringBuilder stringBuilder = new StringBuilder();
          stringBuilder.AppendLine($"Задание: {taskWork.Name}");
          stringBuilder.AppendLine($"\r\nОписание: {taskWork.Description}");
          stringBuilder.AppendLine($"\r\nСтатус: {answer.Status}");
          stringBuilder.AppendLine($"\r\nОтвет: {answer.AnswerText}");
          stringBuilder.AppendLine($"\r\nВаш ответ сохранен.");

          await TelegramBotHandler.SendMessageAsync(botClient, callbackQuery.From.Id, stringBuilder.ToString(), TelegramBotHandler.GetInlineKeyboardMarkupAsync(callbackModels), callbackQuery.Message.Id);

          UserTaskMap.Remove(callbackQuery.From.Id);
          UserAnswerMap.Remove(callbackQuery.From.Id);
        }
      }
    }

    /// <summary>
    /// Отменяет ответ пользователя, если выбрано "Отменить".
    /// </summary>
    /// <param name="botClient">Клиент Telegram-бота для отправки сообщений.</param>
    /// <param name="callbackQuery">Запрос обратного вызова, содержащий данные о пользователе и команде.</param>
    /// <param name="callbackModels">Список моделей обратного вызова.</param>
    /// <returns>Задача, представляющая асинхронную операцию.</returns>
    private async Task CancelAnswerAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery, List<CallbackModel> callbackModels)
    {
      UserTaskMap.Remove(callbackQuery.From.Id);
      UserAnswerMap.Remove(callbackQuery.From.Id);
      await TelegramBotHandler.SendMessageAsync(botClient, callbackQuery.From.Id, "Ответ отменен.", TelegramBotHandler.GetInlineKeyboardMarkupAsync(callbackModels), callbackQuery.Message.Id);
    }
  }
}