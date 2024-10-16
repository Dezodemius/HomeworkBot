using ModelInterfaceHub.Interfaces;
using ModelInterfaceHub.Models;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramBot.Roles
{
  /// <summary>
  /// Представляет пользователя с ролью учителя в системе.
  /// </summary>
  public class Teacher : UserModel, IMessageHandler
  {

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
    /// <param name="message">Текст сообщения от пропеодавателя.</param>
    /// <returns>Ответ на сообщение учителя.</returns>
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
          new[]
          {
            InlineKeyboardButton.WithCallbackData("задания на проверку", "/get_jobhomework")
          },
          new[]
          {
            InlineKeyboardButton.WithCallbackData("домашние задание", "/get_homework")
          },
          new[]
          {
            InlineKeyboardButton.WithCallbackData("выполненые ДЗ", "/get_completedhomework")
          },
          new[]
          {
            InlineKeyboardButton.WithCallbackData("невыполненые ДЗ", "/get_uncompletedhomework")
          },
        });
        await TelegramBotHandler.SendMessageAsync(botClient, chatId, "выберите действие: ", keyboard);
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
      else
      {
        if (callbackData.Contains("/get_jobhomework"))
        {
          await ShowHomeWork(botClient, chatId, messageId, StatusWork.Unchecked);
        }
        else if (callbackData.Contains("/get_homework"))
        {
          await DisplayHomeworkButtons(botClient, chatId, callbackData, messageId);
        }
        else if (callbackData.Contains("/get_completedhomework"))
        {
          await ShowHomeWork(botClient, chatId, messageId, StatusWork.Checked);
        }
        else if (callbackData.Contains("/get_uncompletedhomework"))
        {
          await ShowHomeWork(botClient, chatId, messageId, StatusWork.Unfulfilled);
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
    /// Просмотр домашних заданий по статусу.
    /// </summary>
    /// <returns>Задача, представляющая асинхронную операцию.</returns>
    public async Task ShowHomeWork(ITelegramBotClient botClient, long chatId, int messageId, StatusWork statusWork)
    {
      var homeWork = Core.CommonHomeWorkModel.
        GetTasksForHomework(statusWork).
        GroupBy(x => x.IdHomeWork).
        Select(group =>
        {
          var assigmentId = group.Key;
          var students = group.ToList();

          return Tuple.Create(assigmentId, students);
        });

      var messageBuilder = new StringBuilder();
      messageBuilder.AppendLine($"Домашние работы со статусом: {statusWork.ToString()}");
      foreach (var task in homeWork)
      {
        var homeData = Core.CommonDataModel.GetHomrWorkById(task.Item1);
        messageBuilder.AppendLine($"\tЗадание: {homeData.Title} ");
        foreach (var student in task.Item2)
        {
          var userData = Core.CommonDataModel.GetUserById(student.IdStudent);
          messageBuilder.AppendLine($"\t\tСтудент: {userData.LastName} {userData.FirstName}, Ссылка на GitHub: {student.GithubLink}");
        }
      }
      var keyboard = new InlineKeyboardMarkup(new[]
       {
          new[]
          {
            InlineKeyboardButton.WithCallbackData("На главную", "/start")
          }
        });
      await TelegramBot.TelegramBotHandler.SendMessageAsync(botClient, chatId, messageBuilder.ToString(), keyboard, messageId);

      return;
    }

    /// <summary>
    /// Отображение кнопок для работы с домашними заданиями.
    /// </summary>
    /// <returns>Задача, представляющая асинхронную операцию.</returns>
    public async Task DisplayHomeworkButtons(ITelegramBotClient botClient, long chatId, string callbackData, int messageId)
    {
      return;
    }
  }
}