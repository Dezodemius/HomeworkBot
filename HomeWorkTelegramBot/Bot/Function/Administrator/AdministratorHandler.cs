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
      throw new NotImplementedException();
    }

    public async Task HandleStartButton()
    {
      throw new NotImplementedException();
    }
  }
}
