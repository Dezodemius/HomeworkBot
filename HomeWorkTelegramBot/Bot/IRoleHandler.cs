using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace HomeWorkTelegramBot.Bot.Function
{
  /// <summary>
  /// Интерфейс для обработки взаимодействий с пользователем в зависимости от роли.
  /// </summary>
  public interface IRoleHandler
  {
    /// <summary>
    /// Обрабатывает нажатие кнопки "Старт".
    /// </summary>
    Task HandleStartButton();

    /// <summary>
    /// Обрабатывает входящие сообщения.
    /// </summary>
    /// <param name="message">Сообщение для обработки.</param>
    Task HandleMessageAsync(ITelegramBotClient botClient, Message message);

    /// <summary>
    /// Обрабатывает входящие запросы.
    /// </summary>
    /// <param name="request">Запрос для обработки.</param>
    Task HandleCallback(ITelegramBotClient botClient, CallbackQuery callbackQuery);
  }
}
