using Telegram.Bot.Types;
using Telegram.Bot;

namespace HomeWorkTelegramBot.Bot.Function.Teacher
{
  internal class GetStudentStatistics
  {
    public async Task HandleCallbackQueryAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery)
    {
      long chatId = callbackQuery.Message.Chat.Id;
      string data = callbackQuery.Data;

      if (data.StartsWith("/approve_"))
      {
        long userId = long.Parse(data.Replace("/approve_", ""));
        //await ApproveUser(botClient, callbackQuery, chatId, userId);
      }
      else if (data.StartsWith("/reject_"))
      {
        long userId = long.Parse(data.Replace("/reject_", ""));
        //await RejectUser(botClient, callbackQuery, chatId, userId);
      }
    }
  }
}
