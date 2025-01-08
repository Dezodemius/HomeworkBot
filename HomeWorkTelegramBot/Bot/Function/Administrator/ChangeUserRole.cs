using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types;
using Telegram.Bot;
using HomeWorkTelegramBot.Core;
using System.Collections.Concurrent;

namespace HomeWorkTelegramBot.Bot.Function.Administrator
{

  /// <summary>
  /// Класс для изменения роли пользователя в системе.
  /// </summary>
  internal class ChangeUserRole
  {
    /// <summary>
    /// Отображает кнопки с пользователями для выбора.
    /// </summary>
    /// <param name="botClient">Клиент Telegram бота.</param>
    /// <param name="callbackQuery">Запрос обратного вызова от Telegram.</param>
    public async Task ShowUserButtonsAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery)
    {
      List<Models.User> users = UserService.GetAllUsers();

      var buttons = new List<List<InlineKeyboardButton>>();

      List<CallbackModel> callbacks = new List<CallbackModel>();

      foreach (var user in users)
      {
        callbacks.Add(new CallbackModel($"{user.Surname} {user.Name} {user.Lastname}", $"/select_user:{user.ChatId}"));
      }

      var inlineKeyboard = TelegramBotHandler.GetPaginatedInlineKeyboardMarkup(callbacks);
      var message = await TelegramBotHandler.SendMessageAsync(botClient, callbackQuery.From.Id, "Выберите пользователя:", inlineKeyboard, callbackQuery.Message.Id);
      TelegramBotHandler.InitializePagination(callbackQuery.From.Id, message.MessageId, callbacks);
    }

    /// <summary>
    /// Обрабатывает запросы обратного вызова для выбора пользователя и изменения его роли.
    /// </summary>
    /// <param name="botClient">Клиент Telegram бота.</param>
    /// <param name="callbackQuery">Запрос обратного вызова от Telegram.</param>
    public async Task HandleCallbackQueryAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery)
    {
      var data = callbackQuery.Data;
      if (data.StartsWith("/select_user:"))
      {
        var selectedUserChatId = long.Parse(data.Split(':')[1]);
        var adminChatId = callbackQuery.From.Id;

        var selectedUser = UserService.GetUserByChatId(selectedUserChatId);
        await TelegramBotHandler.SendMessageAsync(botClient, callbackQuery.Message.Chat.Id, $"Вы выбрали пользователя: {selectedUser.Surname} {selectedUser.Name} {selectedUser.Name}", null, callbackQuery.Message.Id);
        await ShowRoleButtonsAsync(botClient, callbackQuery.Message.Chat.Id, selectedUserChatId);
      }
      else if (data.StartsWith("/select_role:"))
      {
        var parts = data.Split(':');
        var selectedUserChatId = long.Parse(parts[1]);
        var selectedRole = (Models.User.Role)Enum.Parse(typeof(Models.User.Role), parts[2]);

        var selectedUser = UserService.GetUserByChatId(selectedUserChatId);
        selectedUser.UserRole = selectedRole;
        UserService.UpdateUser(selectedUser);

        await TelegramBotHandler.SendMessageAsync(botClient, callbackQuery.Message.Chat.Id, $"Роль пользователя {selectedUser.Name} изменена на {selectedRole}", null, callbackQuery.Message.Id);
        await TelegramBotHandler.SendMessageAsync(botClient, selectedUser.ChatId, $"Ваша роль изменена на {selectedRole}");
      }
    }

    /// <summary>
    /// Отображает кнопки с возможными ролями для выбранного пользователя.
    /// </summary>
    /// <param name="botClient">Клиент Telegram бота.</param>
    /// <param name="chatId">Идентификатор чата, в который отправляется сообщение.</param>
    /// <param name="userChatId">Идентификатор чата выбранного пользователя.</param>
    private async Task ShowRoleButtonsAsync(ITelegramBotClient botClient, long chatId, long userChatId)
    {
      var buttons = new List<List<InlineKeyboardButton>>();

      foreach (Models.User.Role role in Enum.GetValues(typeof(Models.User.Role)))
      {
        if (role != Models.User.Role.UnregisteredUser)
        {
          buttons.Add(new List<InlineKeyboardButton> { InlineKeyboardButton.WithCallbackData(role.ToString(), $"/select_role:{userChatId}:{role}") });
        }
      }

      var inlineKeyboard = new InlineKeyboardMarkup(buttons);
      await TelegramBotHandler.SendMessageAsync(botClient, chatId, "Выберите новую роль для пользователя:", inlineKeyboard);
    }
  }
}
