using DataContracts.Interfaces;
using DataContracts.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot;
using Core;
using System.Globalization;
using static DataContracts.Models.Submission;
using TelegramBot.Model;
using TelegramBot.Processing;

namespace TelegramBot.Roles.Teacher
{
  /// <summary>
  /// Представляет пользователя с ролью учителя в системе.
  /// </summary>
  public class Teacher : UserModel, IMessageHandler
  {

    static internal Dictionary<long, Assignment> assigments = new Dictionary<long, Assignment>();

    /// <summary>
    /// Инициализирует новый экземпляр класса Teacher.
    /// </summary>
    /// <param name="telegramChatId">Идентификатор чата Telegram.</param>
    /// <param name="firstName">Имя учителя.</param>
    /// <param name="lastName">Фамилия учителя.</param>
    /// <param name="email">Адрес электронной почты учителя.</param>
    public Teacher(long telegramChatId, string firstName, string lastName, string email)
        : base(telegramChatId, firstName, lastName, email, UserRole.Teacher) { }

    /// <summary>
    /// Обрабатывает входящее сообщение от преподавателя.
    /// </summary>
    /// <param name="message">Текст сообщения от преподавателя.</param>
    /// <returns>Ответ на сообщение учителя.</returns>
    public async Task ProcessMessageAsync(ITelegramBotClient botClient, long chatId, string message)
    {
      if (string.IsNullOrEmpty(message))
      {
        return;
      }
      else if (assigments.ContainsKey(chatId))
      {
        assigments.TryGetValue(chatId, out Assignment? homeworkData);

        if (homeworkData != null)
        {
          await new CreateHomeWorkProcessing(homeworkData).ProcessCreateStepAsync(botClient, chatId, message);
        }
      }
      else if (message == "/start")
      {
        List<CallbackModel> callbackModels = new List<CallbackModel>();
        callbackModels.Add(new CallbackModel("Создать домашнюю работу", "/createHomework"));
        await TelegramBotHandler.SendMessageAsync(botClient, chatId, "Выберите функцию:", TelegramBotHandler.GetInlineKeyboardMarkupAsync(callbackModels));
      }
    }

    /// <summary>
    /// Обработка Callback запросов от преподавателя.
    /// </summary>
    /// <param name="callbackData">Данные обратного вызова.</param>
    /// <returns>Результат обработки запроса.</returns>
    /// <exception cref="NotImplementedException"></exception>
    public async Task ProcessCallbackAsync(ITelegramBotClient botClient, long chatId, string callbackData, int messageId)
    {
      if (string.IsNullOrEmpty(callbackData)) return;
      else if (callbackData.ToLower().Contains("/createhomework"))
      {
        if (!assigments.ContainsKey(chatId))
        {
          assigments.Add(chatId, new Assignment());
        }

        assigments.TryGetValue(chatId, out Assignment? homeworkData);

        if (homeworkData != null)
        {
          await new CreateHomeWorkProcessing(homeworkData).ProcessCreateStepAsync(botClient, chatId, callbackData);
        }
      }
    }
  }
}
