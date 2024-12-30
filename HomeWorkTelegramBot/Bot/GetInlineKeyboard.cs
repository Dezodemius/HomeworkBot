using HomeWorkTelegramBot.Models;
using Telegram.Bot.Types.ReplyMarkups;

namespace HomeWorkTelegramBot.Bot
{
  internal class GetInlineKeyboard
  {
    /// <summary>
    /// Создает клавиатуру с курсами.
    /// </summary>
    /// <param name="courses">Список курсов.</param>
    /// <returns>Клавиатуру с данными о курсах.</returns>
    public static InlineKeyboardMarkup GetCoursesKeyboard(List<Courses> courses)
    {
      List<CallbackModel> callbackModels = new List<CallbackModel>();
      foreach (var course in courses)
      {
        string command = $"/selectcourse_{course.Id}";
        callbackModels.Add(new CallbackModel(course.Name, command));
      }

      return TelegramBotHandler.GetInlineKeyboardMarkupAsync(callbackModels);
    }

    /// <summary>
    /// Создает клавиатуру со студентами.
    /// </summary>
    /// <param name="users">Список студентов курса.</param>
    /// <returns>Клавиатуру с данными о студентах.</returns>
    public static InlineKeyboardMarkup GetStudentsKeyboard(List<User> users)
    {
      List<CallbackModel> callbackModels = new List<CallbackModel>();
      foreach (var user in users)
      {
        string command = $"/selectuser_{user.ChatId}";
        var studentName = $"{user.Surname} {user.Name}";
        callbackModels.Add(new CallbackModel(studentName, command));
      }

      return TelegramBotHandler.GetInlineKeyboardMarkupAsync(callbackModels);
    }

    /// <summary>
    /// Создает клавиатуру со студентами.
    /// </summary>
    /// <param name="tasks">Список студентов курса.</param>
    /// <returns>Клавиатуру с данными о студентах.</returns>
    public static InlineKeyboardMarkup GetTaskKeyboard(List<TaskWork> tasks)
    {
      List<CallbackModel> callbackModels = new List<CallbackModel>();
      foreach (var task in tasks)
      {
        string command = $"/selecttask_{task.Id}";
        callbackModels.Add(new CallbackModel(task.Name, command));
      }

      return TelegramBotHandler.GetInlineKeyboardMarkupAsync(callbackModels);
    }
  }
}
