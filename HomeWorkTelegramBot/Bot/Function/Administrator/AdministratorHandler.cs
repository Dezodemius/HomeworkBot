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
      if (message.Text == "/start")
      {
        await HandleStartButton(botClient, message.Chat.Id);
      }

      if (Mode.CreateCourse)
      {
        await new CreateCourse().ProcessCourseCreationStep(botClient, message);
      }
    }

    public async Task HandleCallback(ITelegramBotClient botClient, CallbackQuery callbackQuery)
    {
      var commandHandlers = new Dictionary<string, Func<Task>>
      {
        { "/approve_", async () => await new NewUser().HandleCallbackQueryAsync(botClient, callbackQuery) },
        { "/reject_", async () => await new NewUser().HandleCallbackQueryAsync(botClient, callbackQuery) },
        { "/newCource", async () => await new CreateCourse().ProcessCourseCreationStep(botClient, callbackQuery) },
        { "/selectteacher_", async () => await new CreateCourse().HandleTeacherSelection(botClient, callbackQuery) },
        { "/start", async () => await HandleStartButton(botClient, callbackQuery.From.Id) },
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

    public async Task HandleStartButton(ITelegramBotClient botClient, long chatId)
    {
      Mode.AllReset();
      List<CallbackModel> callbackModels = new List<CallbackModel>();
      callbackModels.Add(new CallbackModel("Создать курс", "/newCource"));

      await TelegramBotHandler.SendMessageAsync(botClient, chatId, "Выберите действие:", TelegramBotHandler.GetInlineKeyboardMarkupAsync(callbackModels));
      throw new NotImplementedException();
    }
  }
}
