using DataContracts.Interfaces;
using DataContracts.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DataContracts.Models.StudentHomeWorkModel;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot;
using Core;

namespace TelegramBot.Roles.Student
{
  /// <summary>
  /// Представляет пользователя с ролью студента в системе.
  /// </summary>
  internal class Student : UserModel, IMessageHandler
  {

    private readonly List<StudentHomeWorkModel> homeWorkModels;

    /// <summary>
    /// Инициализирует новый экземпляр класса Student.
    /// </summary>
    /// <param name="telegramChatId">Идентификатор чата Telegram.</param>
    /// <param name="firstName">Имя студента.</param>
    /// <param name="lastName">Фамилия студента.</param>
    /// <param name="email">Адрес электронной почты студента.</param>
    /// <param name="dbManager">Менеджер базы данных.</param>
    internal Student(long telegramChatId, string firstName, string lastName, string email)
        : base(telegramChatId, firstName, lastName, email, UserRole.Student)
    {
      homeWorkModels = CommonHomeWork.GetHomeworkForStudent(telegramChatId);
    }

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
            InlineKeyboardButton.WithCallbackData("Статусы домашних работ", "/view_homework_statuses"),
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

      else
      {
        if (callbackData.Contains("/view_homework_statuses"))
        {
          var keyboard = new InlineKeyboardMarkup(new[]
          {
          new []
          {
            InlineKeyboardButton.WithCallbackData("Непроверенные", "/homeWorkStatus_unchecked"),
          },
          new []
          {
            InlineKeyboardButton.WithCallbackData("Проверенные", "/homeWorkStatus_checked"),
          },
          new []
          {
            InlineKeyboardButton.WithCallbackData("В доработке", "/homeWorkStatus_needsRevision")
          }
        });

          await TelegramBotHandler.SendMessageAsync(botClient, chatId, "Выберите статус домашнего задания:", keyboard, messageId);
        }
        else if (callbackData.Contains("/homeWorkStatus"))
        {
          await CheckStatusHomeWork(botClient, chatId, callbackData, messageId);
        }
        else if (callbackData.Contains("/start"))
        {
          await botClient.DeleteMessageAsync(chatId, messageId);
          await Task.Delay(10);
          await ProcessMessageAsync(botClient, chatId, callbackData);
        }
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
        "/homeWorkStatus_unchecked" => StatusWork.Unchecked,
        "/homeWorkStatus_checked" => StatusWork.Checked,
        "/homeWorkStatus_needsRevision" => StatusWork.NeedsRevision,
        _ => throw new NotImplementedException(),
      };

      var message = await DisplayHomeWorkStatuses(statusWork, chatId);
      var keyboard = new InlineKeyboardMarkup(new[]
       {
          new []
          {
            InlineKeyboardButton.WithCallbackData("Назад", "/start"),
          },
        });
      await TelegramBot.TelegramBotHandler.SendMessageAsync(botClient, chatId, message, keyboard, messageId);
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
    private async Task<string> DisplayHomeWorkStatuses(StatusWork status, long userId)
    {
      var filteredHomeWorks = homeWorkModels
          .Where(hw => hw.IdStudent == userId && hw.Status == status)
          .ToList();

      if (!filteredHomeWorks.Any())
      {
        return $"Нет домашних заданий со статусом '{status}' для пользователя с ID {userId}.";
      }

      var sb = new StringBuilder();
      sb.AppendLine($"Домашние задания со статусом '{status}' для пользователя с ID {userId}:");

      foreach (var hw in filteredHomeWorks)
      {
        sb.AppendLine($"ID задания: {hw.IdHomeWork}, GitHub: {hw.GithubLink}");
      }

      return sb.ToString();
    }
  }
}
