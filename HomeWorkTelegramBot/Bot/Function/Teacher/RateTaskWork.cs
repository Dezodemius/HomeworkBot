using Telegram.Bot.Types;
using Telegram.Bot;

namespace HomeWorkTelegramBot.Bot.Function.Teacher
{
  /// <summary>
  /// Выставление оценки домашнему заданию студента.
  /// </summary>
  internal class RateTaskWork
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
    }
  }
}
