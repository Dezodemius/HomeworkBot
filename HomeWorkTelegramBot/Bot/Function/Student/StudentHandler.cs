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
      throw new NotImplementedException();
    }

    public async Task HandleCallback(ITelegramBotClient botClient, CallbackQuery callbackQuery)
    {
      throw new NotImplementedException();
    }

    public async Task HandleStartButton(ITelegramBotClient botClient, long chatId)
    {
      throw new NotImplementedException();
    }
  }
}
