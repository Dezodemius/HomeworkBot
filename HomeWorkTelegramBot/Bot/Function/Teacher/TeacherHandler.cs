using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using static HomeWorkTelegramBot.Config.Logger;

namespace HomeWorkTelegramBot.Bot.Function.Teacher
{
  internal class TeacherHandler : IRoleHandler
  {
    private static int _selectedCourseId = -1;

    public async Task HandleMessageAsync(ITelegramBotClient botClient, Message message)
    {
      if (CreateTaskWork._creationData.Count != 0)
      {
        await new NewTaskWork().HandleMessageAsync(botClient, message);
      }
      else
      {
        if (message.Text == "/start")
        {
          await HandleStartButton(botClient, message.Chat.Id);
        }
      }
    }

    public async Task HandleCallback(ITelegramBotClient botClient, CallbackQuery callbackQuery)
    {
      if (callbackQuery.Data.StartsWith("/selectcourse_"))
      {
        string[] parts = callbackQuery.Data.Split('_');
        if (parts.Length > 2 && int.TryParse(parts[2], out int courseId))
        {
          _selectedCourseId = courseId;
        }
      }

      var commandHandlers = new Dictionary<string, Func<Task>>
      {
        { "/createhw", async () => await new NewTaskWork().HandleCallback(botClient, callbackQuery) },
        { "/selectcourse_nt", async () => await new NewTaskWork().HandleCallback(botClient, callbackQuery) },

        { "/studhwstat", async () => await new GetStudentStatistics().HandleCallbackQueryAsync(botClient, callbackQuery) },
        { "/selectcourse_sd", async () => await new GetStudentStatistics().HandleCallbackQueryAsync(botClient, callbackQuery) },
        { "/selectuser_", async () => await new GetStudentStatistics().HandleCallbackQueryAsync(botClient, callbackQuery) },
        { "/selectanswer_", async () => await new GetStudentStatistics().HandleCallbackQueryAsync(botClient, callbackQuery) },

        { "/hwstatistics", async () => await new GetTaskWorkStatistics().HandleCallbackQueryAsync(botClient, callbackQuery) },
        { "/selectcourse_tw", async () => await new GetTaskWorkStatistics().HandleCallbackQueryAsync(botClient, callbackQuery) },
        { "/selecttask_", async () => await new GetTaskWorkStatistics().HandleCallbackQueryAsync(botClient, callbackQuery) },
        { "/useransw_", async () => await new GetTaskWorkStatistics().HandleCallbackQueryAsync(botClient, callbackQuery) },

        { "/correct_", async () => await new RateTaskWorkHandler().HandleCallbackQueryAsync(botClient, callbackQuery) },
        { "/incorrect_", async () => await new RateTaskWorkHandler().HandleCallbackQueryAsync(botClient, callbackQuery) },

        { "/start", async () => await HandleStartButton(botClient, callbackQuery.From.Id) },
        { "/page:", async () => await TelegramBotHandler.HandlePaginationCallbackAsync(botClient, callbackQuery) },

      };

      foreach (var command in commandHandlers.Keys)
      {
        if (callbackQuery.Data.StartsWith(command))
        {
          LogInformation($"Выполняется обработчик для команды: {command}");
          await commandHandlers[command]();
          return;
        }
      }
    }


    public async Task HandleStartButton(ITelegramBotClient botClient, long chatId)
    {
      StringBuilder sb = new StringBuilder();
      sb.AppendLine("Добро пожаловать в панель преподавателя. Выберите действие:");
      List<CallbackModel> callbacks = GetDefaultButtonsCallbacks();
      InlineKeyboardMarkup keyboard = CreateDefaultKeyboard(callbacks);
      await new RateTaskWorkHandler().ClearData();
      await new GetStudentStatistics().ClearData();
      await new GetTaskWorkStatistics().ClearData();
      await new NewTaskWork().ClearData();
      var message = await TelegramBotHandler.SendMessageAsync(botClient, chatId, sb.ToString(), keyboard);
      TelegramBotHandler.InitializePagination(chatId, message.MessageId, callbacks);
    }

    private static List<CallbackModel> GetDefaultButtonsCallbacks()
    {
      return new List<CallbackModel>
      {
        new CallbackModel("Создать новое домашнее задание", "/createhw"),
        new CallbackModel("Посмотреть статусы домашних заданий студента", "/studhwstat"),
        new CallbackModel("Посмотреть выполнение домашнего задания", "/hwstatistics")
      };
    }

    private static InlineKeyboardMarkup CreateDefaultKeyboard(List<CallbackModel> callbacks)
    {
      return TelegramBotHandler.GetPaginatedInlineKeyboardMarkup(callbacks);
    }

    private async Task HandleMenuCommand(ITelegramBotClient botClient, CallbackQuery callbackQuery)
    {
      StringBuilder sb = new StringBuilder();
      sb.AppendLine("Выберите действие:");
      List<CallbackModel> callbacks = GetDefaultButtonsCallbacks();
      InlineKeyboardMarkup keyboard = CreateDefaultKeyboard(callbacks);
      await new RateTaskWorkHandler().ClearData();
      await new GetStudentStatistics().ClearData();
      await new GetTaskWorkStatistics().ClearData();
      await new NewTaskWork().ClearData();

      await TelegramBotHandler.SendMessageAsync(botClient, callbackQuery.From.Id, sb.ToString(), keyboard, callbackQuery.Message.Id);
    }
  }
}
