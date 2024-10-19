using ModelInterfaceHub.Interfaces;
using ModelInterfaceHub.Models;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBot.Model;

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
              InlineKeyboardButton.WithCallbackData("Просмотр дз", "/helpHomework")
            },
            new[]
            {
              InlineKeyboardButton.WithCallbackData("Создать дз", "/addHomeWork")
            },

          });
        await TelegramBotHandler.SendMessageAsync(botClient, chatId, "Выберите действие: ", keyboard);
      }
      else if (!string.IsNullOrEmpty(UserStateTracker.GetUserState(chatId)))
      {
        await ProcessCreatingHomeWorkAsync(botClient, chatId, message);
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
        if (callbackData.ToLower(CultureInfo.CurrentCulture).Contains("/helphomework"))
        {
          var keyboard = new InlineKeyboardMarkup(new[]
          {
            new[]
            {
              InlineKeyboardButton.WithCallbackData("задания на проверку", "/get_jobhomework")
            },
            new[]
            {
              InlineKeyboardButton.WithCallbackData("Не проверенные домашние задание", "/get_uncheckedHomework")
            },
            new[]
            {
              InlineKeyboardButton.WithCallbackData("Сданные домашние задание", "/get_checkedHomework")
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
          await TelegramBotHandler.SendMessageAsync(botClient, chatId, "Выберите действие: ", keyboard, messageId);
        }
        else if (callbackData.ToLower(CultureInfo.CurrentCulture).Contains("/addhomework"))
        {
          await CreateHomeWorkAsync(botClient, chatId);
        }
        else if (callbackData.Contains("/get_jobhomework"))
        {
          await ShowHomeWork(botClient, chatId, messageId, StatusWork.Unchecked);
        }
        else if (callbackData.Contains("/get_uncheckedHomework"))
        {
          if (callbackData.Contains("homework_"))
          {
            await ShowHomeWorkForStatus(botClient, chatId, callbackData, messageId, StatusWork.Unchecked);
          }
          else
          {
            await DisplayHomeworkButtons(botClient, chatId, callbackData, messageId);
          }
        }
        else if (callbackData.Contains("/get_checkedHomework"))
        {
          if (callbackData.Contains("homework_"))
          {
            await ShowHomeWorkForStatus(botClient, chatId, callbackData, messageId, StatusWork.Checked);
          }
          else
          {
            await DisplayHomeworkButtons(botClient, chatId, callbackData, messageId);
          }
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
        var homeData = Core.CommonDataModel.GetHomeWorkById(task.Item1);
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
      var homeWork = Core.CommonHomeWorkModel.GetHomeWork();
      var buttons = new List<InlineKeyboardButton[]>();

      foreach (var item in homeWork)
      {
        var homeData = Core.CommonDataModel.GetHomeWorkById(item.Id);

        var button = InlineKeyboardButton.WithCallbackData(
            text: homeData.Title,
            callbackData: $"{callbackData}_homework_{homeData.Id}"
        );

        buttons.Add(new[] { button });
      }

      var inlineKeyboard = new InlineKeyboardMarkup(buttons);

      await TelegramBotHandler.SendMessageAsync(botClient, chatId, "Выберите домашнее задание", inlineKeyboard, messageId);
    }

    /// <summary>
    /// Показывает домашние задания по статусу.
    /// </summary>
    /// <param name="botClient">Клиент Telegram бота.</param>
    /// <param name="chatId">Идентификатор чата.</param>
    /// <param name="callbackData">Данные обратного вызова.</param>
    /// <param name="messageId">Идентификатор сообщения.</param>
    /// <param name="status">Статус домашнего задания.</param>
    /// <returns>Задача, представляющая асинхронную операцию.</returns>
    public async Task ShowHomeWorkForStatus(ITelegramBotClient botClient, long chatId, string callbackData, int messageId, StatusWork status)
    {
      var id = callbackData.Split('_');
      var homeWorkDescription = Core.CommonDataModel.GetHomeWorkById(Convert.ToInt32(id.Last()));
      var messageBuilder = new StringBuilder();

      messageBuilder.AppendLine("Заголовок: " + homeWorkDescription.Title);
      messageBuilder.AppendLine("Описание: " + homeWorkDescription.Description);

      var homeWork = Core.CommonHomeWorkModel.
      GetTasksForHomework(status).
      Where(x => x.IdHomeWork == Convert.ToInt32(id.Last())).ToList();

      foreach (var task in homeWork)
      {
        var userData = Core.CommonDataModel.GetUserById(task.IdStudent);
        messageBuilder.AppendLine($"\t\tСтудент: {userData.LastName} {userData.FirstName}");
      }

      var keyboard = new InlineKeyboardMarkup(new[]
      {
          new[]
          {
            InlineKeyboardButton.WithCallbackData("На главную", "/start")
          },
        });
      await TelegramBot.TelegramBotHandler.SendMessageAsync(botClient, chatId, messageBuilder.ToString(), keyboard, messageId);
    }

    /// <summary>
    /// Создает новое домашнее задание.
    /// </summary>
    /// <param name="botClient">Клиент Telegram бота.</param>
    /// <param name="chatId">Идентификатор чата.</param>
    /// <returns>Задача, представляющая асинхронную операцию.</returns>
    public async Task CreateHomeWorkAsync(ITelegramBotClient botClient, long chatId)
    {
      await TelegramBotHandler.SendMessageAsync(botClient, chatId, "Введите название домашней работы:");
      UserStateTracker.SetUserState(chatId, "awaiting_homework_title");
    }

    /// <summary>
    /// Обрабатывает создание домашнего задания.
    /// </summary>
    /// <param name="botClient">Клиент Telegram бота.</param>
    /// <param name="chatId">Идентификатор чата.</param>
    /// <param name="message">Текст сообщения от учителя.</param>
    /// <returns>Задача, представляющая асинхронную операцию.</returns>
    public async Task ProcessCreatingHomeWorkAsync(ITelegramBotClient botClient, long chatId, string message)
    {
      var state = UserStateTracker.GetUserState(chatId);

      if (state == "awaiting_homework_title")
      {
        UserStateTracker.SetTemporaryData(chatId, "homework_title", message);
        await TelegramBotHandler.SendMessageAsync(botClient, chatId, "Введите описание домашней работы:");
        UserStateTracker.SetUserState(chatId, "awaiting_homework_description");
      }
      else if (state == "awaiting_homework_description")
      {
        var title = UserStateTracker.GetTemporaryData(chatId, "homework_title");
        var description = message;

        Core.CommonHomeWorkModel.AddHomeWork(title, description);
        await TelegramBotHandler.SendMessageAsync(botClient, chatId, "Домашняя работа успешно создана!");

        UserStateTracker.ClearTemporaryData(chatId);
        UserStateTracker.SetUserState(chatId, null);
      }
    }
  }
}