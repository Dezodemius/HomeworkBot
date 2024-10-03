using DataContracts;
using SqlHelper;
using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using static DataContracts.IParticipant;

namespace RoleModels
{
  public class Student : IParticipant
  {
    public string Id { get; set; }
    public string FullName { get; set; }
    public UserRole Role { get; set; }
    public string ActiveHomeWorkNumber { get; set; }
    static SqliteDatabase sqliteDatabase;

    /// <summary>
    /// Основной метод, обрабатывающий входящие сообщения.
    /// </summary>
    /// <param name="client">Клиент Telegram Bot API.</param>
    /// <param name="update">Обновление, содержащее сообщение.</param>
    /// <param name="token">Токен для отмены асинхронных операций.</param>
    public async Task StartAsync(ITelegramBotClient client, Update update, CancellationToken token)
    {
      var message = update.Message;

      if (message != null && message.Text != null)
      {
        if (message.Text.ToLower().Contains("/start"))
        {
          await SendWelcomeMessage(client, message.Chat.Id, message.Chat.FirstName, token);
        }
        else if (message.Text.ToLower().Contains("просмотреть статусы дз"))
        {
          await ShowHomeworkStatuses(client, message.Chat.Id, token);
        }
        else if (message.Text.ToLower().Contains("залить дз"))
        {
          await ShowHomeworkOptions(client, message.Chat.Id, "Выберите домашнее задание:", token);
        }
        else if (message.Text.ToLower().StartsWith("домашнее задание"))
        {
          await HandleHomeworkSelection(client, message, token);
        }
        else if (message.Text.ToLower().Contains("назад"))
        {
          await ShowHomeworkOptions(client, message.Chat.Id, "Пожалуйста, выберите домашнее задание с помощью кнопок ниже:", token);
        }
        else if (IsGitHubLink(message.Text))
        {
          await HandleGitHubLink(client, message, token);
        }
        else
        {
          await RequestValidGitHubLink(client, message.Chat.Id, token);
        }
      }
    }

    /// <summary>
    /// Отправляет приветственное сообщение с опциями меню.
    /// </summary>
    /// <param name="client">Клиент Telegram Bot API.</param>
    /// <param name="chatId">Идентификатор чата.</param>
    /// <param name="firstName">Имя пользователя.</param>
    /// <param name="token">Токен для отмены асинхронных операций.</param>
    private async Task SendWelcomeMessage(ITelegramBotClient client, long chatId, string firstName, CancellationToken token)
    {
      string welcomeMessage = $"{firstName}, привет! Выберите пункт из меню:";
      await client.SendTextMessageAsync(chatId, welcomeMessage, replyMarkup: new ReplyKeyboardMarkup(new[]
      {
        new KeyboardButton("Просмотреть статусы ДЗ"),
        new KeyboardButton("Залить ДЗ")
      }) 
      { ResizeKeyboard = true }, cancellationToken: token);
    }

    /// <summary>
    /// Отображает статусы домашних заданий.
    /// </summary>
    /// <param name="client">Клиент Telegram Bot.</param>
    /// <param name="chatId">Идентификатор чата.</param>
    /// <param name="token">Токен отмены.</param>
    private async Task ShowHomeworkStatuses(ITelegramBotClient client, long chatId, CancellationToken token)
    {
      List<Tuple<string, string>> result = await sqliteDatabase.GetHomeworkStatusesAsync(chatId.ToString());

      if (result.Count == 0)
      {
        await client.SendTextMessageAsync(chatId, "У вас нет статусов домашних заданий.", cancellationToken: token);
        return;
      }

      string message = "Статусы ваших домашних заданий:\n";
      foreach (var item in result)
      {
        message += $"{item.Item1}, Статус: {item.Item2}\n";
      }

      await client.SendTextMessageAsync(chatId, message, cancellationToken: token);
    }


    /// <summary>
    /// Обрабатывает выбор домашнего задания.
    /// </summary>
    /// <param name="client">Клиент Telegram Bot.</param>
    /// <param name="message">Сообщение Telegram.</param>
    /// <param name="token">Токен отмены.</param>
    private async Task HandleHomeworkSelection(ITelegramBotClient client, Message message, CancellationToken token)
    {
      DataContracts.IParticipant.HomeworkStatus? status = await sqliteDatabase.GetHomeworkStatusAsync(Id, message.Text);
      if (status != null)
      {
        if (status == HomeworkStatus.NotReviewed)
        {
          await ShowHomeworkOptions(client, message.Chat.Id, "Вы не можете изменить данное домашнее задание, пока преподаватель его не проверил. Выберите другое задание.", token);
          return;
        }
        else if (status == HomeworkStatus.Accepted)
        {
          await ShowHomeworkOptions(client, message.Chat.Id, "Ваше домашнее задание уже зачтено. Пожалуйста, не нагружайте преподавателя лишней работой. Выберите другое задание.", token);
          return;
        }
      }

      await client.SendTextMessageAsync(message.Chat.Id, $"Выбрано: \"{message.Text}\". Добавьте ссылку на ваш репозиторий GitHub для отправки на проверку.", replyMarkup: new ReplyKeyboardMarkup(new[]
      {
        new KeyboardButton("Назад")
    })
      {
        ResizeKeyboard = true
      }, cancellationToken: token);
      ActiveHomeWorkNumber = message.Text;
    }

    /// <summary>
    /// Отображает опции домашних заданий.
    /// </summary>
    /// <param name="client">Клиент Telegram Bot.</param>
    /// <param name="chatId">Идентификатор чата.</param>
    /// <param name="messageText">Текст сообщения.</param>
    /// <param name="token">Токен отмены.</param>
    async Task ShowHomeworkOptions(ITelegramBotClient client, long chatId, string messageText, CancellationToken token)
    {
      var rkm = new ReplyKeyboardMarkup(new[]
      {
                new KeyboardButton[]
                {
                    "Домашнее задание №1",
                    "Домашнее задание №2",
                },
                new KeyboardButton[]
                {
                    "Домашнее задание №3",
                    "Домашнее задание №4",
                },
                new KeyboardButton[]
                {
                    "Домашнее задание №5",
                    "Домашнее задание №6",
                },
                new KeyboardButton[]
                {
                    "Домашнее задание №7",
                    "Домашнее задание №8",
                },
            })
      {
        ResizeKeyboard = true,
        OneTimeKeyboard = true
      };

      await client.SendTextMessageAsync(chatId, messageText, replyMarkup: rkm, cancellationToken: token);
    }

    /// <summary>
    /// Обрабатывает ссылку на GitHub.
    /// </summary>
    /// <param name="client">Клиент Telegram Bot.</param>
    /// <param name="message">Сообщение Telegram.</param>
    /// <param name="token">Токен отмены.</param>
    private async Task HandleGitHubLink(ITelegramBotClient client, Message message, CancellationToken token)
    {
      if (await sqliteDatabase.AddHomeworkRecordAsync(this, ActiveHomeWorkNumber, message.Text))
      {
        List<string> teachers = await sqliteDatabase.GetAllTeacherIdsAsync();
        foreach (string teacher in teachers)
        {
          ChatId id = new ChatId(teacher);
          await client.SendTextMessageAsync(id, $"Пользователь: {FullName} добавил ответ на \"{ActiveHomeWorkNumber}\" : {message.Text}", cancellationToken: token);
        }
      }

      await client.SendTextMessageAsync(message.Chat.Id, $"Ссылка получена: {message.Text}. Ваше домашнее задание будет проверено.", cancellationToken: token);
      await ShowHomeworkOptions(client, message.Chat.Id, "Выберите следующее домашнее задание:", token);
    }

    /// <summary>
    /// Запрашивает корректную ссылку на GitHub.
    /// </summary>
    /// <param name="client">Клиент Telegram Bot.</param>
    /// <param name="chatId">Идентификатор чата.</param>
    /// <param name="token">Токен отмены.</param>
    private async Task RequestValidGitHubLink(ITelegramBotClient client, long chatId, CancellationToken token)
    {
      await client.SendTextMessageAsync(chatId, "Пожалуйста, добавьте корректную ссылку на GitHub или нажмите 'Назад', чтобы выбрать другое задание.", replyMarkup: new ReplyKeyboardMarkup(new[]
      {
        new KeyboardButton("Назад")
      })
      {
        ResizeKeyboard = true
      }, cancellationToken: token);
    }

    /// <summary>
    /// Проверяет, является ли текст ссылкой на GitHub.
    /// </summary>
    /// <param name="text">Текст для проверки.</param>
    /// <returns>Возвращает true, если текст является корректной ссылкой на GitHub.</returns>
    bool IsGitHubLink(string text)
    {
      var githubLinkPattern = @"^(https?:\/\/)?(www\.)?github\.com\/[\w\-]+\/[\w\-]+(\/)?$";
      return Regex.IsMatch(text, githubLinkPattern, RegexOptions.IgnoreCase);
    }

    /// <summary>
    /// Конструктор класса Student.
    /// </summary>
    /// <param name="id">Идентификатор студента.</param>
    /// <param name="fullName">Полное имя студента.</param>
    public Student(string id, string fulName)
    {
      Id = id;
      FullName = fulName;
      Role = UserRole.Student;
      sqliteDatabase = new SqliteDatabase(DefaultData.botConfig.DbConnectionString, null, null);
    }

    /// <summary>
    /// Конструктор класса Student.
    /// </summary>
    /// <param name="id">Идентификатор студента.</param>
    /// <param name="fullName">Полное имя студента.</param>
    public Student(string id, string fulName, string numberHomeWork)
    {
      Id = id;
      FullName = fulName;
      Role = UserRole.Student;
      sqliteDatabase = new SqliteDatabase(DefaultData.botConfig.DbConnectionString, null, null);
      ActiveHomeWorkNumber = numberHomeWork;
    }
  }
}
