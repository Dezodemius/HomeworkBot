using DataContracts;
using SqlHelper;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Runtime.Intrinsics.Arm;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using static DataContracts.IParticipant;

namespace RoleModels
{
  public class Teacher : IParticipant
  {
    public string Id { get; set; }
    public string FullName { get; set; }
    public UserRole Role { get; set; }
    private static SqliteDatabase sqliteDatabase;
    private static CurrentTeachingAssignment currentTeachingAssignments;
    private static Dictionary<long, UserState> userStates = new Dictionary<long, UserState>();
    private static string idStudent;
    private static string currentHomework;
    private static Dictionary<long, string> pendingReviewComments = new Dictionary<long, string>();


    public async Task StartAsync(ITelegramBotClient client, Update update, CancellationToken token)
    {
      var message = update.Message;

      try
      {
        if (message != null && message.Text != null)
        {

          if (message.Text.ToLower().Contains("/start"))
          {
            await MessageStudents(client, message.Chat.Id, token);
          }
          else if (message.Text.ToLower().Contains("домашнее задание"))
          {
            await MessageHomework(client, message.Chat.Id, idStudent, message.Text, token);
          }
          else if (message.Text.ToLower().Contains("отзыв"))
          {
            await EvaluateReviewStatus(client, message.Chat.Id, token);
          }
          else if (message.Text.ToLower().Contains("зачтено"))
          {
            await CompleteTask(client, message.Chat.Id, token);
          }
          else if (message.Text.ToLower().Contains("доработать"))
          {
            await RequestRevisionComment(client, message.Chat.Id, token);
          }
          else if (message.Text == "Отмена")
          {
            await MessageHomeWorks(client, message.Chat.Id, idStudent, token);
          }
          else if (pendingReviewComments.ContainsKey(message.Chat.Id))
          {
            await SubmitRevisionComment(client, message.Chat.Id, message.Text, token);
          }
          else
          {
            string id = await sqliteDatabase.SearchStudentIdByFullNameExactAsync(message.Text);
            if (id != null && id != string.Empty)
            {
              idStudent = id;
              await MessageHomeWorks(client, message.Chat.Id, id, token);
            }
            else
            {
              await client.SendTextMessageAsync(message.Chat.Id, $"Команда не распознана!", cancellationToken: token);
            }
          }
        }
      }
      catch (Exception ex)
      {
        Console.WriteLine($"Exception in StartAsync: {ex.Message}");
        await client.SendTextMessageAsync(message.Chat.Id, $"На стороне приложения произошла ошибка!({ex})", cancellationToken: token);
      }
    }

    /// <summary>
    /// Предоставляет выбор студента по кнопкам.
    /// </summary>
    /// <returns></returns>
    private async Task MessageStudents(ITelegramBotClient client, long chatId, CancellationToken token)
    {
      List<Tuple<string, string>> students = await sqliteDatabase.GetAllStudentsAsync();
      List<string> result = new List<string>();
      foreach (var item in students)
      {
        result.Add(item.Item2);
      }

      ReplyKeyboardMarkup replyKeyboardMarkup = GetReplyKeyboardMarkup(result);

      await client.SendTextMessageAsync(chatId, "Выберите студента из списка", replyMarkup: replyKeyboardMarkup, cancellationToken: token);
    }

    /// <summary>
    /// Предоставляет выбор домашнего задания опредлённого студента по кнопкам.
    /// </summary>
    /// <returns></returns>
    private async Task MessageHomeWorks(ITelegramBotClient client, long chatId, string idStudent, CancellationToken token)
    {
      var statusTranslations = new Dictionary<string, HomeworkStatus>
      {
          { "Не сдано", HomeworkStatus.NotSubmitted},
          { "Не проверено" ,HomeworkStatus.NotReviewed},
          { "Требует доработки", HomeworkStatus.RequiresRevision},
          { "Принято", HomeworkStatus.Accepted}
      };


      List<Tuple<string, string>> homeWorks = await sqliteDatabase.GetHomeworkStatusesAsync(idStudent);
      List<string> result = new List<string>();


      foreach (var item in homeWorks)
      {
        HomeworkStatus status = statusTranslations[item.Item2];
        if (status == HomeworkStatus.NotReviewed)
        {
          result.Add(item.Item1);
        }
      }

      ReplyKeyboardMarkup replyKeyboardMarkup = GetReplyKeyboardMarkup(result);

      await client.SendTextMessageAsync(chatId, "Выберите домашнее задание из списка", replyMarkup: replyKeyboardMarkup, cancellationToken: token);

    }

    /// <summary>
    /// Выводит информацию об опреденном домашнем задании, определённого студента.
    /// </summary>
    /// <returns></returns>
    private async Task MessageHomework(ITelegramBotClient client, long chatId, string idStudent, string numberHomework, CancellationToken token)
    {
      string fullname = await sqliteDatabase.SearchStudentFullNameByIdExactAsync(idStudent);
      string link = await sqliteDatabase.GetHomeworkLinkAsync(idStudent, numberHomework);
      string message = $"{fullname} - {numberHomework}: {link}";

      List<string> buttonsName = new List<string>()
      {
        "Отзыв",
        "Отмена",
      };
      currentHomework = numberHomework;

      ReplyKeyboardMarkup replyKeyboardMarkup = GetReplyKeyboardMarkup(buttonsName);
      await client.SendTextMessageAsync(chatId, message, replyMarkup: replyKeyboardMarkup, cancellationToken: token);
    }

    /// <summary>
    /// Кнопки при нажатии на отзыв.
    /// </summary>
    /// <param name="client"></param>
    /// <param name="chatId"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    private async Task EvaluateReviewStatus(ITelegramBotClient client, long chatId, CancellationToken token)
    {
      string message = $"Выберите действие";

      List<string> buttonsName = new List<string>()
      {
        "Зачтено",
        "Доработать",
      };

      ReplyKeyboardMarkup replyKeyboardMarkup = GetReplyKeyboardMarkup(buttonsName);
      await client.SendTextMessageAsync(chatId, message, replyMarkup: replyKeyboardMarkup, cancellationToken: token);
    }

    /// <summary>
    /// Завершает задачу студента.
    /// </summary>
    /// <param name="client"></param>
    /// <param name="chatId"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    private async Task CompleteTask(ITelegramBotClient client, long chatId, CancellationToken token)
    {
      bool result = await sqliteDatabase.UpdateHomeworkStatusAsync(idStudent, currentHomework, HomeworkStatus.Accepted);
      if (result)
      {
        string fullname = await sqliteDatabase.SearchStudentFullNameByIdExactAsync(idStudent);
        await client.SendTextMessageAsync(chatId, $"{currentHomework}  студента \"{fullname}\" зачтено.", cancellationToken: token);
        await client.SendTextMessageAsync(idStudent, $"{currentHomework} зачтено.", cancellationToken: token);
      }
      else
      { 
        await client.SendTextMessageAsync(chatId, $"Ошибка приложения", cancellationToken: token);
      }
    }

    private async Task RequestRevisionComment(ITelegramBotClient client, long chatId, CancellationToken token)
    {
      string message = "Пожалуйста, введите комментарий или причину доработки.";
      // Save the current state to track pending review comments
      pendingReviewComments[chatId] = currentHomework;

      await client.SendTextMessageAsync(chatId, message, cancellationToken: token);
    }

    private async Task SubmitRevisionComment(ITelegramBotClient client, long chatId, string comment, CancellationToken token)
    {
      if (pendingReviewComments.TryGetValue(chatId, out string homeworkNumber))
      {
        pendingReviewComments.Remove(chatId);

        string message = $"Ваша работа \"{homeworkNumber}\" требует доработки. Комментарий: {comment}";
        await client.SendTextMessageAsync(idStudent, message, cancellationToken: token);
        await client.SendTextMessageAsync(chatId, $"Комментарий отправлен студенту.", cancellationToken: token);
        await sqliteDatabase.UpdateHomeworkStatusAsync(idStudent, homeworkNumber, HomeworkStatus.RequiresRevision);
      }
      else
      {
        await client.SendTextMessageAsync(chatId, "Не удалось найти задачу для доработки.", cancellationToken: token);
      }
    }

    private ReplyKeyboardMarkup GetReplyKeyboardMarkup(List<string> names)
    {
      int buttonsPerRow = 2;
      int numberOfRows = (int)Math.Ceiling((double)names.Count / buttonsPerRow);
      var keyboardButtons = new KeyboardButton[numberOfRows][];

      for (int i = 0; i < numberOfRows; i++)
      {
        keyboardButtons[i] = new KeyboardButton[buttonsPerRow];

        for (int j = 0; j < buttonsPerRow; j++)
        {
          int index = i * buttonsPerRow + j;
          if (index < names.Count)
          {
            keyboardButtons[i][j] = new KeyboardButton(names[index]);
          }
          else
          {
            keyboardButtons[i][j] = new KeyboardButton(""); // Пустая кнопка, если нет данных
          }
        }
      }

      return new ReplyKeyboardMarkup(keyboardButtons) { ResizeKeyboard = true };
    }

    /// <summary>
    /// Конструктор класса Учитель.
    /// </summary>
    public Teacher(string id, string fullName)
    {
      Id = id;
      FullName = fullName;
      Role = UserRole.Teacher;
      sqliteDatabase = new SqliteDatabase(DefaultData.botConfig.DbConnectionString, null, null);
    }
  }
}
