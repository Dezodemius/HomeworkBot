using HomeWorkTelegramBot.Bot.Function.Administrator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace HomeWorkTelegramBot.Bot.Function.Student
{
  internal class StudentHandler : IRoleHandler
  {
    public async Task HandleMessageAsync(ITelegramBotClient botClient, Message message)
    {

      var commandHandlers = new Dictionary<string, Func<Task>>
      {
        { "/start", async () => await HandleStartButton(botClient, message.Chat.Id)},
      };

      foreach (var command in commandHandlers.Keys)
      {
        if (message.Text.StartsWith(command))
        {
          await commandHandlers[command]();
          return;
        }
      }
    }

    public async Task HandleCallback(ITelegramBotClient botClient, CallbackQuery callbackQuery)
    {
      var commandHandlers = new Dictionary<string, Func<Task>>
      {
        { "/start", async () => await HandleStartButton(botClient, callbackQuery.From.Id)},
        { "/viewHomework", async () => await new HomeworkHandler().DisplayHomework(botClient, callbackQuery) },
      };

      foreach (var command in commandHandlers.Keys)
      {
        if (callbackQuery.Data.StartsWith(command))
        {
          await commandHandlers[command]();
          return;
        }
      }
    }

    public async Task HandleStartButton(ITelegramBotClient botClient, long chatId)
    {
      string message = $"{Utils.TimeGreeting.GetGreeting()}. Выберите функцию:";
      List<CallbackModel> callbacks = new List<CallbackModel>();
      callbacks.Add(new CallbackModel("Домашние задания", "/viewHomework"));

      await TelegramBotHandler.SendMessageAsync(botClient, chatId, message, TelegramBotHandler.GetInlineKeyboardMarkupAsync(callbacks));
    }
  }
}
