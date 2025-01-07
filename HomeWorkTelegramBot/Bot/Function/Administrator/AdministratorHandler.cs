using HomeWorkTelegramBot.Bot.Function.Student;
using HomeWorkTelegramBot.Config;
using Microsoft.AspNetCore.Identity;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;
using static HomeWorkTelegramBot.Bot.Function.Student.StudentHandler;

namespace HomeWorkTelegramBot.Bot.Function.Administrator
{
  internal class AdministratorHandler : IRoleHandler
  {

    public enum AdminAction
    {
      None,
      CreateCourse,
    }

    internal static Dictionary<long, AdminAction> AdminActions = new Dictionary<long, AdminAction>();

    public async Task HandleMessageAsync(ITelegramBotClient botClient, Message message)
    {
      if (AdminActions.TryGetValue(message.Chat.Id, out AdminAction action))
      {
        switch (action)
        {
          case AdminAction.CreateCourse:
            await new CreateCourse().ProcessCourseCreationStep(botClient, message);
            return;
        }
      }

      var commandHandlers = new Dictionary<string, Func<Task>>
      {
        { "/start", async () => await HandleStartButton(botClient, message.Chat.Id)},
      };

      foreach (var command in commandHandlers.Keys)
      {
        if (message.Text.StartsWith(command))
        {
          await commandHandlers[command]();
          return;
        }
      }
    }

    public async Task HandleCallback(ITelegramBotClient botClient, CallbackQuery callbackQuery)
    {
      var commandHandlers = new Dictionary<string, Func<Task>>
      {
        { "/approve_", async () => await new NewUser().HandleCallbackQueryAsync(botClient, callbackQuery) },
        { "/reject_", async () => await new NewUser().HandleCallbackQueryAsync(botClient, callbackQuery) },
        { "/start", async () => await HandleStartButton(botClient, callbackQuery.From.Id) },
        { "/changeRole", async () => await new ChangeUserRole().ShowUserButtonsAsync(botClient, callbackQuery) },
        { "/select_", async () => await new ChangeUserRole().HandleCallbackQueryAsync(botClient, callbackQuery) },
        { "/createCourse_select_teacher", async () =>  await new CreateCourse().ProcessCourseCreationStep(botClient, callbackQuery)},
        { "/createCourse", async () =>
          {
            AdminActions[callbackQuery.From.Id] = AdminAction.CreateCourse;
            await new CreateCourse().ProcessCourseCreationStep(botClient, callbackQuery);
          }
        },
        { "/deleteCourse:", async () => await new DeleteCourse().DeleteCourseAsync(botClient, callbackQuery) },
        { "/deleteCourse", async () => await new DeleteCourse().ShowCoursesForDeletionAsync(botClient, callbackQuery) },
        { "/log", async () => { if (callbackQuery.From.Id != ApplicationData.ConfigApp.AdminId) { await SendAccessDeniedMessageAsync(botClient, callbackQuery.From.Id); return; } await LogViewer.DisplayLogFilesAsync(botClient, callbackQuery.From.Id); }},
        { "/viewLog", () => LogViewer.SendErrorLogsAsync(botClient, callbackQuery) },
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

    public async Task HandleStartButton(ITelegramBotClient botClient, long chatId, int? messgaId = null)
    {
      StringBuilder sb = new StringBuilder();
      sb.AppendLine("Добро пожаловать в административную панель");

      sb.AppendLine($"{Utils.TimeGreeting.GetGreeting()}. Выберите функцию:");

      List<CallbackModel> callbacks = new List<CallbackModel>();
      callbacks.Add(new CallbackModel("Создать курс", "/createCourse"));
      callbacks.Add(new CallbackModel("Сменить роль пользователя", "/changeRole"));
      callbacks.Add(new CallbackModel("Удалить курс", "/deleteCourse"));
      callbacks.Add(new CallbackModel("Просмотреть логи", "/log"));

      await TelegramBotHandler.SendMessageAsync(botClient, chatId, sb.ToString(), TelegramBotHandler.GetInlineKeyboardMarkupAsync(callbacks), messgaId);
    }

    private static async Task SendAccessDeniedMessageAsync(ITelegramBotClient botClient, long chatId)
    {
      await TelegramBotHandler.SendMessageAsync(botClient, chatId, "У вас не достаточно прав для данной команды. Введите /help для просмотора доступынх вам команд.");
    }
  }
}
