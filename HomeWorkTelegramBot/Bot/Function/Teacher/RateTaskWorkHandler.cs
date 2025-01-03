using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot;

namespace HomeWorkTelegramBot.Bot.Function.Teacher
{
  internal class RateTaskWorkHandler
  {
    public async Task HandleCallbackQueryAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery, int taskId = -1)
    {
      await RateTaskWork.ProcessUpdateAnswer(botClient, callbackQuery, taskId);
    }
  }
}
