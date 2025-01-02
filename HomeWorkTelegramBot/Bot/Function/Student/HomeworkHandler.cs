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
    public async Task DisplayHomework(ITelegramBotClient botClient, CallbackQuery callbackQuery)
    {
      var answers = AnswerService.GetAnswersByChatId(callbackQuery.From.Id);
      var taskIds = answers.Select(a => a.TaskId).Distinct().ToList();

      var callbackModels = new List<CallbackModel>();

      foreach (var taskId in taskIds)
      {
        var taskWork = TaskWorkService.GetTaskWorkById(taskId);
        if (taskWork != null)
        {
          string command = $"/viewHomework_id{taskWork.Id}";
          callbackModels.Add(new CallbackModel(taskWork.Name, command));
        }
      }

      await TelegramBotHandler.SendMessageAsync(botClient, callbackQuery.From.Id, "Выберите домашнее задание:", TelegramBotHandler.GetInlineKeyboardMarkupAsync(callbackModels), callbackQuery.Message.Id);
    }

    public async Task HandleHomeworkSelection(ITelegramBotClient botClient, CallbackQuery callbackQuery)
    {
      string data = callbackQuery.Data;
      if (data.StartsWith("/viewHomework_id"))
      {
        int taskId = int.Parse(data.Replace("/viewHomework_id", ""));
        var taskWork = TaskWorkService.GetTaskWorkById(taskId);

        if (taskWork != null)
        {
          string message = $"Задание: {taskWork.Name}\nОписание: {taskWork.Description}";
          await TelegramBotHandler.SendMessageAsync(botClient, callbackQuery.From.Id, message, null, callbackQuery.Message.Id);
        }
        else
        {
          await TelegramBotHandler.SendMessageAsync(botClient, callbackQuery.From.Id, "Задание не найдено.", null, callbackQuery.Message.Id);
        }
      }
    }
  }
}
