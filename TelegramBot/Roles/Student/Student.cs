using ModelInterfaceHub.Interfaces;
using ModelInterfaceHub.Models;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramBot.Roles
{
  /// <summary>
  /// Представляет пользователя с ролью студента в системе.
  /// </summary>
  public class Student : UserModel, IMessageHandler
  {

    private List<HomeWorkModel> homeWorkModels;

    /// <summary>
    /// Инициализирует новый экземпляр класса Student.
    /// </summary>
    /// <param name="telegramChatId">Идентификатор чата Telegram.</param>
    /// <param name="firstName">Имя студента.</param>
    /// <param name="lastName">Фамилия студента.</param>
    /// <param name="email">Адрес электронной почты студента.</param>
    /// <param name="dbManager">Менеджер базы данных.</param>
    public Student(long telegramChatId, string firstName, string lastName, string email)
        : base(telegramChatId, firstName, lastName, email, UserRole.Student)
    {
      homeWorkModels = new List<HomeWorkModel>()
      {
        new HomeWorkModel(1, "Тест1", "Описание1", HomeWorkModel.StatusWork.Unread),
        new HomeWorkModel(2, "Тест2", "Описание2", HomeWorkModel.StatusWork.Unread),
        new HomeWorkModel(3, "Тест3", "Описание3", HomeWorkModel.StatusWork.NeedsRevision),
        new HomeWorkModel(4, "Тест4", "Описание4", HomeWorkModel.StatusWork.NeedsRevision),
        new HomeWorkModel(5, "Тест5", "Описание5", HomeWorkModel.StatusWork.Checked),
        new HomeWorkModel(6, "Тест6", "Описание6", HomeWorkModel.StatusWork.Checked),
        new HomeWorkModel(6, "Тест7", "Описание7", HomeWorkModel.StatusWork.Unchecked),
        new HomeWorkModel(6, "Тест8", "Описание8", HomeWorkModel.StatusWork.Unchecked),
      };
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
      HomeWorkModel.StatusWork statusWork = callbackData switch
      {
        "/homeWorkStatus_unchecked" => HomeWorkModel.StatusWork.Unchecked,
        "/homeWorkStatus_checked" => HomeWorkModel.StatusWork.Checked,
        "/homeWorkStatus_needsRevision" => HomeWorkModel.StatusWork.NeedsRevision,
        _ => throw new NotImplementedException(),
      };

      var message = await DisplayHomeWorkStatuses(statusWork);
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
    /// Отображает список домашних заданий с их статусами.
    /// </summary>
    /// <param name="status">Статус домашних заданий для отображения.</param>
    /// <returns>Строка с списком домашних заданий и их статусами.</returns>
    private async Task<string> DisplayHomeWorkStatuses(HomeWorkModel.StatusWork status)
    {
      var filteredHomeworks = homeWorkModels.Where(hw => hw.Status == status);
      var result = string.Join("\n", filteredHomeworks.Select(hw => $"Домашняя работа \"{hw.Title}\" - \"{hw.Status}\""));
      return result;
    }
  }
}