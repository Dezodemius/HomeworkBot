using Telegram.Bot.Types;
using Telegram.Bot;

namespace HomeWorkTelegramBot.Bot.Function.Teacher
{
  internal class GetStudentStatistics
  {
    public async Task HandleCallbackQueryAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery)
    {
      await StudentsData.ProcessGetTasks(botClient, callbackQuery);
    }
  }
}
