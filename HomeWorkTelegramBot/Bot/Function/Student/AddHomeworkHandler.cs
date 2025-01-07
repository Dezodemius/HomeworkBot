using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using HomeWorkTelegramBot.Core;
using HomeWorkTelegramBot.Models;
using System.Text;

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
        StringBuilder stringBuilder = new StringBuilder();

        stringBuilder.AppendLine($"Задание: {taskWork.Name}");
        stringBuilder.AppendLine($"\r\nОписание: {taskWork.Description}");
        stringBuilder.AppendLine($"\r\nВаш ответ: {message.Text}");
        stringBuilder.AppendLine($"\r\nВы хотите сохранить этот ответ?");

        List<CallbackModel> callbackModels = new List<CallbackModel>
        {
          new CallbackModel("Сохранить", $"/saveAnswer_{taskId}"),
          new CallbackModel("Отменить", $"/cancelAnswer")
        };

        await TelegramBotHandler.SendMessageAsync(botClient, message.From.Id, stringBuilder.ToString(), TelegramBotHandler.GetInlineKeyboardMarkupAsync(callbackModels));
      }
      else
      {
        await TelegramBotHandler.SendMessageAsync(botClient, message.From.Id, "Ошибка: не удалось найти задание. Пожалуйста, попробуйте снова.");
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
      List<CallbackModel> callbackModels =
      [
        new CallbackModel("Вернуться к списку домашних работ", $"/viewHomework"),
        new CallbackModel("На главную", $"/start"),
      ];

      if (data.StartsWith("/saveAnswer_"))
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
      else if (data == "/cancelAnswer")
      {
        UserTaskMap.Remove(callbackQuery.From.Id);
        UserAnswerMap.Remove(callbackQuery.From.Id);
        await TelegramBotHandler.SendMessageAsync(botClient, callbackQuery.From.Id, "Ответ отменен.", TelegramBotHandler.GetInlineKeyboardMarkupAsync(callbackModels), callbackQuery.Message.Id);
      }
    }
  }
}