using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace HomeWorkTelegramBot.Bot.Function.Teacher
{
  internal class TeacherHandler : IRoleHandler
  {
    public async Task HandleMessageAsync(ITelegramBotClient botClient, Message message)
    {
      if (CreateTaskWork._creationData.Count != 0)
      {
        await new NewTaskWork().HandleMessageAsync(botClient, message);
      }
      else
      {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("Добро пожаловать в панель преподавателя");
        var keyboard = new InlineKeyboardMarkup(new[]
        {
        new[]
            {
                InlineKeyboardButton.WithCallbackData("Создать новое домашнее задание", "/createhw"),
            },
        new[]
            {
                InlineKeyboardButton.WithCallbackData("Посмотреть статусы домашних заданий студента", "/studhwstat"),
            },
        new[]
            {
                InlineKeyboardButton.WithCallbackData("Посмотреть выполнение домашнего задания", "/hwstatistics"),
            },
        });
        await TelegramBotHandler.SendMessageAsync(botClient, message.Chat.Id, sb.ToString(), keyboard);
      }
    }

    public async Task HandleCallback(ITelegramBotClient botClient, CallbackQuery callbackQuery)
    {
      var commandHandlers = new Dictionary<string, Func<Task>>
      {
        { "/createhw", async () => await new NewTaskWork().HandleCallback(botClient, callbackQuery) },
        { "/studhwstat", async () => await new GetStudentStatistics().HandleCallbackQueryAsync(botClient, callbackQuery) },
        { "/hwstatistics", async () => await new GetTaskWorkStatistics().HandleCallbackQueryAsync(botClient, callbackQuery) },
      };

      if (CreateTaskWork._creationData.Count != 0 && callbackQuery.Data.StartsWith("/selectcourse_"))
      {
        await new NewTaskWork().HandleCallback(botClient, callbackQuery);
      }
      else if (callbackQuery.Data.StartsWith("/selectcourse_"))
      {
        await new GetStudentStatistics().HandleCallbackQueryAsync(botClient, callbackQuery);
      }
      else if (callbackQuery.Data.StartsWith("/selecttask_"))
      {
        await new GetTaskWorkStatistics().HandleCallbackQueryAsync(botClient, callbackQuery);
      }

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
