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
using static DataContracts.Models.Submission;
using TelegramBot.Model;

namespace TelegramBot.Roles.Student
{
  /// <summary>
  /// Представляет пользователя с ролью студента в системе.
  /// </summary>
  internal class Student : UserModel, IMessageHandler
  {


    /// <summary>
    /// Инициализирует новый экземпляр класса Student.
    /// </summary>
    /// <param name="telegramChatId">Идентификатор чата Telegram.</param>
    /// <param name="firstName">Имя студента.</param>
    /// <param name="lastName">Фамилия студента.</param>
    /// <param name="email">Адрес электронной почты студента.</param>
    /// <param name="dbManager">Менеджер базы данных.</param>
    internal Student(long telegramChatId, string firstName, string lastName, string email)
        : base(telegramChatId, firstName, lastName, email, UserRole.Student) { }

    /// <summary>
    /// Обрабатывает входящее сообщение от студента.
    /// </summary>
    /// <param name="message">Текст сообщения от студента.</param>
    public async Task ProcessMessageAsync(ITelegramBotClient botClient, long chatId, string message)
    {
      if (string.IsNullOrEmpty(message))
      {
        return;
      }

      if (message.ToLower().Contains("/start"))
      {
        var keyboard = new InlineKeyboardMarkup(new[]
        {
          new []
          {
            InlineKeyboardButton.WithCallbackData("Статусы домашних работ", "/homeWork"),
          }
        });
        await TelegramBotHandler.SendMessageAsync(botClient, chatId, "Выберите функцию:", keyboard);

      }
    }

    /// <summary>
    /// Обработка Callback запросов от студента.
    /// </summary>
    /// <param name="callbackData"></param>
    public async Task ProcessCallbackAsync(ITelegramBotClient botClient, long chatId, string callbackData, int messageId)
    {
      if (string.IsNullOrEmpty(callbackData))
      {
        return;
      }

      else if (callbackData.Contains("/homeWork_"))
      {
        if (callbackData.Contains("id_"))
        {

        }
        else
        {
          await CheckStatusHomeWork(botClient, chatId, callbackData, messageId);
        }
      }
      else if (callbackData.Contains("/homeWork"))
      {
        var keyboard = new InlineKeyboardMarkup(new[]
        {
         new []
         {
           InlineKeyboardButton.WithCallbackData("Не отправленные", "/homeWork_unfulfilled"),
         },
         new []
         {
           InlineKeyboardButton.WithCallbackData("В доработке", "/homeWork_needsRevision")
         }
       });

        await TelegramBotHandler.SendMessageAsync(botClient, chatId, "Выберите статус домашнего задания:", keyboard, messageId);
      }
      else if (callbackData.Contains("/start"))
      {
        await botClient.DeleteMessageAsync(chatId, messageId);
        await ProcessMessageAsync(botClient, chatId, callbackData);
      }

    }

    /// <summary>
    /// Проверяет статусы домашних работ.
    /// </summary>
    /// <param name="botClient"></param>
    /// <param name="chatId"></param>
    /// <param name="callbackData"></param>
    /// <param name="messageId"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    private async Task CheckStatusHomeWork(ITelegramBotClient botClient, long chatId, string callbackData, int messageId)
    {
      StatusWork statusWork = callbackData switch
      {
        "/homeWork_unchecked" => StatusWork.Unchecked,
        "/homeWork_checked" => StatusWork.Checked,
        "/homeWork_needsRevision" => StatusWork.NeedsRevision,
        "/homeWork_unfulfilled" => StatusWork.Unfulfilled,
        _ => throw new NotImplementedException(),
      };

     await DisplayHomeWorkStatuses(botClient,chatId, messageId, statusWork, chatId);
    }

    /// <summary>
    /// Отображает список домашних заданий с их статусами для конкретного пользователя.
    /// </summary>
    /// <param name="status">Статус домашних заданий для отображения.</param>
    /// <param name="userId">Идентификатор пользователя.</param>
    /// <returns>Строка с списком домашних заданий и их статусами для конкретного пользователя.</returns>
    /// <summary>
    /// Отображает список домашних заданий с их статусами для конкретного пользователя.
    /// </summary>
    /// <param name="status">Статус домашних заданий для отображения.</param>
    /// <param name="userId">Идентификатор пользователя.</param>
    /// <returns>Строка с списком домашних заданий и их статусами для конкретного пользователя.</returns>
    private async Task DisplayHomeWorkStatuses(ITelegramBotClient botClient, long chatId, int messageId, StatusWork status, long userId)
    {
      var filteredHomeWorks = CommonHomeWork.GetHomeworkForStudent(userId)
          .Where(hw => hw.Status == status)
          .ToList();

      var student = CommonUserModel.GetUserById(userId);

      List<CallbackModel> callbackModels = new List<CallbackModel>();
      callbackModels.Add(new CallbackModel("Назад", "/homeWork"));
      if (!filteredHomeWorks.Any())
      {
        await TelegramBotHandler.SendMessageAsync(botClient, chatId, $"Нет домашних заданий со статусом '{status}' для пользователя {student.LastName} {student.FirstName}.", TelegramBotHandler.GetInlineKeyboardMarkupAsync(callbackModels), messageId);
        return;
      }
      else
      {
        foreach (var hw in filteredHomeWorks)
        {
          var homeWorkData = CommonHomeWork.GetHomeWorkById(hw.CourseId, hw.AssignmentId);

          if (homeWorkData == null)
          {
            continue;
          }
          callbackModels.Add(new CallbackModel(homeWorkData.Title, $"/homeWork_id_{homeWorkData.AssignmentId}"));
        }
        await TelegramBotHandler.SendMessageAsync(botClient, chatId, $"Домашние задания со статусом '{status}' для пользователя {student.LastName} {student.FirstName}:", TelegramBotHandler.GetInlineKeyboardMarkupAsync(callbackModels), messageId);
      }

    }
  }
}
