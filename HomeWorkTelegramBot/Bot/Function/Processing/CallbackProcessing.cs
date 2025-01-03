﻿using HomeWorkTelegramBot.Bot.Function.Administrator;
using HomeWorkTelegramBot.Config;
using HomeWorkTelegramBot.Core;
using Telegram.Bot;
using Telegram.Bot.Types;
using static HomeWorkTelegramBot.Config.Logger;

namespace HomeWorkTelegramBot.Bot
{
  internal class CallbackProcessing
  {
    /// <summary>
    /// Обрабатывает входящие callback-запросы от пользователей.
    /// </summary>
    /// <param name="callbackQuery">Данные callback-запроса.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    static internal async Task HandleCallbackQueryAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
      var chatId = callbackQuery.From.Id;
      var messageText = callbackQuery.Message.Text.ToLower();
      LogInformation($"Получено сообщение от {callbackQuery.Message.From.LastName} {callbackQuery.Message.From.FirstName} - {callbackQuery.Message.Text}");

      if (chatId == ApplicationData.ConfigApp.AdminId)
      {
        new AdministratorHandler().HandleCallback(callbackQuery);
        return;
      }

      try
      {
        var userRole = UserService.GetUserRoleByChatId(chatId);
        var handler = RoleHandlerFactory.CreateHandler(userRole);

        if (handler != null)
        {
          handler.HandleCallback(callbackQuery);
          LogInformation($"Сообщение обработано для роли: {userRole}");
        }
        else
        {
          LogWarning($"Обработчик для роли {userRole} не найден.");
        }
      }
      catch (Exception ex)
      {
        LogError($"Ошибка при обработке сообщения от {chatId}: {ex.Message}");
      }
    }
  }
}
