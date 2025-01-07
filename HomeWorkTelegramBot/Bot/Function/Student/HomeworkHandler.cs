using HomeWorkTelegramBot.Core;
using HomeWorkTelegramBot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace HomeWorkTelegramBot.Bot.Function.Student
{
  internal class HomeworkHandler
  {
    /// <summary>
    /// Отображает список доступных домашних заданий для пользователя.
    /// </summary>
    /// <param name="botClient">Клиент Telegram-бота.</param>
    /// <param name="callbackQuery">Запрос обратного вызова от пользователя.</param>
    public async Task DisplayHomework(ITelegramBotClient botClient, CallbackQuery callbackQuery)
    {
      var answers = AnswerService.GetAnswersByChatId(callbackQuery.From.Id);
      var taskIds = answers.Select(a => a.TaskId).Distinct().ToList();

      StringBuilder stringBuilder = new StringBuilder();
      stringBuilder.AppendLine($"Выберите домашнее задание:\r\n");

      var callbackModels = new List<CallbackModel>();

      foreach (var taskId in taskIds)
      {
        var taskWork = TaskWorkService.GetTaskWorkById(taskId);
        if (taskWork != null)
        {
          string command = $"/viewHomework_id{taskWork.Id}";
          callbackModels.Add(new CallbackModel(taskWork.Name, command));

          var answer = AnswerService.GetAnswerByChatIdAndTaskId(callbackQuery.From.Id, taskId);
          stringBuilder.AppendLine($"Задание: {taskWork.Name}");
          stringBuilder.AppendLine($"Статус: {answer.Status}\r\n");
        }
      }

      await TelegramBotHandler.SendMessageAsync(botClient, callbackQuery.From.Id, stringBuilder.ToString(), TelegramBotHandler.GetInlineKeyboardMarkupAsync(callbackModels), callbackQuery.Message.Id);
    }

    /// <summary>
    /// Обрабатывает выбор домашнего задания и отправляет его описание пользователю.
    /// </summary>
    /// <param name="botClient">Клиент Telegram-бота.</param>
    /// <param name="callbackQuery">Запрос обратного вызова от пользователя.</param>
    public async Task HandleHomeworkSelection(ITelegramBotClient botClient, CallbackQuery callbackQuery)
    {
      string data = callbackQuery.Data;
      if (data.StartsWith("/viewHomework_id"))
      {
        int taskId = int.Parse(data.Replace("/viewHomework_id", ""));
        var taskWork = TaskWorkService.GetTaskWorkById(taskId);

        if (taskWork != null)
        {

          var answer = AnswerService.GetAnswerByChatIdAndTaskId(callbackQuery.From.Id, taskId);

          StringBuilder stringBuilder = new StringBuilder();
          stringBuilder.AppendLine($"Задание: {taskWork.Name}");
          stringBuilder.AppendLine($"\r\nОписание: {taskWork.Description}");
          stringBuilder.AppendLine($"\r\nСтатус: {answer.Status}");
          if (answer.Status != Answer.TaskStatus.NotAnswered)
          {
            stringBuilder.AppendLine($"Ответ: {answer.AnswerText}");
          }

          List<CallbackModel> callbackModels = new List<CallbackModel>();
          if (answer.Status == Answer.TaskStatus.NotAnswered || answer.Status == Answer.TaskStatus.IncorrectAnswer)
          {
            callbackModels.Add(new CallbackModel("Добавить ответ", $"/addAnswer_id{taskId}"));
          }

          callbackModels.Add(new CallbackModel("Вернуться к списку домашних работ", $"/viewHomework"));
          callbackModels.Add(new CallbackModel("На главную", $"/start"));

          await TelegramBotHandler.SendMessageAsync(botClient, callbackQuery.From.Id, stringBuilder.ToString(), TelegramBotHandler.GetInlineKeyboardMarkupAsync(callbackModels), callbackQuery.Message.Id);
        }
        else
        {
          await TelegramBotHandler.SendMessageAsync(botClient, callbackQuery.From.Id, "Задание не найдено.", null, callbackQuery.Message.Id);
        }
      }
    }
  }
}