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
    public enum StudentAction
    {
      None,
      AddingAnswer
    }

    private static readonly Dictionary<long, StudentAction> UserActions = new Dictionary<long, StudentAction>();

    public async Task HandleMessageAsync(ITelegramBotClient botClient, Message message)
    {

      if (UserActions.TryGetValue(message.Chat.Id, out StudentAction action))
      {
        switch (action)
        {
          case StudentAction.AddingAnswer:
            await new AddHomeworkHandler().RequestAnswerConfirmationAsync(botClient, message);
            return;
        }
      }

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
        { "/start", async () => await HandleStartButton(botClient, callbackQuery.From.Id, callbackQuery.Message.Id)},
        { "/viewHomework_id", async () => await new HomeworkHandler().HandleHomeworkSelection(botClient, callbackQuery) },
        { "/viewHomework", async () => await new HomeworkHandler().DisplayHomework(botClient, callbackQuery) },
        { "/addAnswer_id", async () =>
          {
            await new AddHomeworkHandler().RequestAnswerAsync(botClient, callbackQuery);
            UserActions[callbackQuery.From.Id] = StudentAction.AddingAnswer;
          }
        },
        { "/cancelAnswer", async () =>  await new AddHomeworkHandler().ProcessAnswerAsync(botClient, callbackQuery) },
        { "/saveAnswer", async () =>  await new AddHomeworkHandler().ProcessAnswerAsync(botClient, callbackQuery) },

      };

      foreach (var command in commandHandlers.Keys)
      {
        if (callbackQuery.Data.ToLower().Contains(command.ToLower()))
        {
          await commandHandlers[command]();
          return;
        }
      }
    }

    public async Task HandleStartButton(ITelegramBotClient botClient, long chatId, int messgaId = -1)
    {
      string message = $"{Utils.TimeGreeting.GetGreeting()}. Выберите функцию:";
      List<CallbackModel> callbacks = new List<CallbackModel>();
      callbacks.Add(new CallbackModel("Домашние задания", "/viewHomework"));

      if (messgaId != -1)
      {
        await TelegramBotHandler.SendMessageAsync(botClient, chatId, message, TelegramBotHandler.GetInlineKeyboardMarkupAsync(callbacks), messgaId);
      }
      else
      {
        await TelegramBotHandler.SendMessageAsync(botClient, chatId, message, TelegramBotHandler.GetInlineKeyboardMarkupAsync(callbacks));
      }
    }
  }
}
