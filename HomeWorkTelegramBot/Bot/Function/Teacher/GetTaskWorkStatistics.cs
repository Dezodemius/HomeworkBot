using Telegram.Bot.Types;
using Telegram.Bot;

namespace HomeWorkTelegramBot.Bot.Function.Teacher
{
  internal class GetTaskWorkStatistics
  {
    public async Task HandleCallbackQueryAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery)
    {
      await TaskWorkData.ProcessGetTasks(botClient, callbackQuery);
    }
  }
}
