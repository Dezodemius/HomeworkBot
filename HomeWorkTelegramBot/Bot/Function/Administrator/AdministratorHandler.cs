using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace HomeWorkTelegramBot.Bot.Function.Administrator
{
  internal class AdministratorHandler : IRoleHandler
  {
    public async Task HandleMessageAsync(ITelegramBotClient botClient, Message message)
    {
      StringBuilder sb = new StringBuilder();
      sb.AppendLine("Добро пожаловать в административную панель");
      await TelegramBotHandler.SendMessageAsync(botClient, message.Chat.Id, sb.ToString());
    }

    public async Task HandleCallback(ITelegramBotClient botClient, CallbackQuery callbackQuery)
    {
      var commandHandlers = new Dictionary<string, Func<Task>>
      {
        { "/approve_", async () => await new NewUser().HandleCallbackQueryAsync(botClient, callbackQuery) },
        { "/reject_", async () => await new NewUser().HandleCallbackQueryAsync(botClient, callbackQuery) },
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

    public async Task HandleStartButton()
    {
      throw new NotImplementedException();
    }
  }
}
