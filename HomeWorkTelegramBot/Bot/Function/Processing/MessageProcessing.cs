using HomeWorkTelegramBot.Bot.Function.Administrator;
using HomeWorkTelegramBot.Config;
using HomeWorkTelegramBot.Core;
using Telegram.Bot;
using Telegram.Bot.Types;
using static HomeWorkTelegramBot.Config.Logger;

namespace HomeWorkTelegramBot.Bot.Function.Processing
{
  internal class MessageProcessing
  {
    /// <summary>
    /// Обрабатывает входящие сообщения от пользователей.
    /// </summary>
    /// <param name="botClient">Клиент Telegram бота.</param>
    /// <param name="message">Сообщение от пользователя.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    internal static async Task HandleMessageAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
      var chatId = message.From.Id;
      var messageText = message.Text.ToLower();
      LogInformation($"Получено сообщение от {message.From.LastName} {message.From.FirstName} - {message.Text}");

      if (chatId == ApplicationData.ConfigApp.AdminId)
      {
        new AdministratorHandler().HandleMessageAsync(botClient, message);
        return;
      }

      try
      {
        var userRole = UserService.GetUserRoleByChatId(chatId);
        var handler = RoleHandlerFactory.CreateHandler(userRole);

        if (handler != null)
        {
          handler.HandleMessageAsync(botClient, message);
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
