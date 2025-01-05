using HomeWorkTelegramBot.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace HomeWorkTelegramBot.Bot.Function.UnregisteredUser
{
  internal class UnregisteredUserHandler : IRoleHandler
  {
    public async Task HandleCallback(ITelegramBotClient botClient, CallbackQuery callbackQuery)
    {
      if (!UserRegistrationService.UserRegistrationExists(callbackQuery.From.Id))
      {
        await Registration.ProcessRegistrationStep(botClient, callbackQuery);
      }
      else
      {
        await TelegramBotHandler.SendMessageAsync(botClient, callbackQuery.From.Id, "Вы уже оставили заявку на регистрацию. Пожалуйста, ожидайте рассмотрения заявки.");
      }
    }

    public async Task HandleMessageAsync(ITelegramBotClient botClient, Message message)
    {
      if (!UserRegistrationService.UserRegistrationExists(message.Chat.Id))
      {
        await Registration.ProcessRegistrationStep(botClient, message);
      }
      else
      {
        await TelegramBotHandler.SendMessageAsync(botClient, message.Chat.Id, "Вы уже оставили заявку на регистрацию. Пожалуйста, ожидайте рассмотрения заявки.");
      }
    }

    public async Task HandleStartButton(ITelegramBotClient botClient, long chatId)
    {
      throw new NotImplementedException();
    }
  }
}
