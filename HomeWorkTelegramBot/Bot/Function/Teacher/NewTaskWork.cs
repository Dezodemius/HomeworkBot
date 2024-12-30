using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot;
using HomeWorkTelegramBot.Core;

namespace HomeWorkTelegramBot.Bot.Function.Teacher
{
  public class NewTaskWork : IRoleHandler
  {
    public async Task HandleCallback(ITelegramBotClient botClient, CallbackQuery callbackQuery)
    {
      await CreateTaskWork.ProcessCreationStep(botClient, callbackQuery);
    }

    public async Task HandleMessageAsync(ITelegramBotClient botClient, Message message)
    {
      await CreateTaskWork.ProcessCreationStep(botClient, message);
    }

    public Task HandleStartButton()
    {
      throw new NotImplementedException();
    }
  }
}
