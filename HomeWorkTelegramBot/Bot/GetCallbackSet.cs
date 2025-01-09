using HomeWorkTelegramBot.Models;
using Telegram.Bot.Types.ReplyMarkups;
using System.Text;
using HomeWorkTelegramBot.Core;

namespace HomeWorkTelegramBot.Bot
{
  internal class GetCallbackSet
  {
    /// <summary>
    /// Создает клавиатуру с курсами.
    /// </summary>
    /// <param name="courses">Список курсов.</param>
    /// <returns>Клавиатуру с данными о курсах.</returns>
    public static List<CallbackModel> GetCoursesCallbacks(List<Courses> courses, string commandText)
    {
      return GetCallbacks(courses, commandText);
    }

    /// <summary>
    /// Создает клавиатуру со студентами.
    /// </summary>
    /// <param name="users">Список студентов курса.</param>
    /// <returns>Клавиатуру с данными о студентах.</returns>
    public static List<CallbackModel> GetStudentsCallbacks(List<Models.User> users, string commandText)
    {
      return GetCallbacks(users, commandText);
    }

    /// <summary>
    /// Создает клавиатуру с заданиями.
    /// </summary>
    /// <param name="tasks">Список заданий курса.</param>
    /// <returns>Клавиатуру с данными о заданиях.</returns>
    public static List<CallbackModel> GetTaskCallbacks(List<TaskWork> tasks)
    {
      return GetCallbacks(tasks, "selecttask");
    }

    /// <summary>
    /// Создает клавиатуру с заданиями.
    /// </summary>
    /// <param name="tasks">Список заданий курса.</param>
    /// <returns>Клавиатуру с данными о заданиях.</returns>
    public static List<CallbackModel> GetAnswersCallbacks(List<Answer> answers)
    {
      return GetCallbacks(answers, "selectanswer");
    }

    /// <summary>
    /// Создает клавиатуру с пагинацией для различных типов объектов.
    /// </summary>
    /// <typeparam name="T">Тип объекта (User, TaskWork или Courses).</typeparam>
    /// <param name="items">Список объектов.</param>
    /// <param name="commandText">Префикс команды.</param>
    /// <returns>Клавиатура с кнопками и навигацией.</returns>
    private static List<CallbackModel> GetCallbacks<T>(List<T> items, string commandText)
    {
      List<CallbackModel> callbackModels = new List<CallbackModel>();
      GetDataButtons(commandText, callbackModels, items);
      return callbackModels;
      //return TelegramBotHandler.GetPaginatedInlineKeyboardMarkup(callbackModels);
    }

    /// <summary>
    /// Создает кнокпи с необходимыми даннными в зависимости от типа объекта.
    /// </summary>
    /// <typeparam name="T">Тип объекта (User, TaskWork или Courses).</typeparam>
    /// <param name="commandText">Префикс команды.</param>
    /// <param name="callbackModels">Список объектов класса CallbackModel.</param>
    /// <param name="pageItems">Список элементов на странице.</param>
    public static void GetDataButtons<T>(string commandText, List<CallbackModel> callbackModels, IEnumerable<T> pageItems)
    {
      foreach (var item in pageItems)
      {
        string buttonText;
        string command;

        switch (item)
        {
          case Models.User user:
            buttonText = $"{user.Surname} {user.Name}";
            command = $"/{commandText}_{user.ChatId}";
            break;

          case TaskWork task:
            buttonText = task.Name;
            command = $"/{commandText}_{task.Id}";
            break;

          case Courses course:
            buttonText = course.Name;
            command = $"/{commandText}_{course.Id}";
            break;

          case Answer answer:
            buttonText = GetButtonData(answer);
            command = $"/{commandText}_{answer.Id}";
            break;

          default:
            continue;
        }

        callbackModels.Add(new CallbackModel(buttonText, command));
      }
    }

    /// <summary>
    /// Получает данные для кнопки.
    /// </summary>
    /// <param name="answer">Объект класса Answer.</param>
    /// <returns>Строку с данными.</returns>
    private static string GetButtonData(Answer answer)
    {
      var sb = new StringBuilder();
      var task = TaskWorkService.GetTaskWorkById(answer.TaskId);
      if (task != null)
      {
        sb.AppendLine(task.Name);

        return sb.ToString();
      }

      return $"Для ответа с id {answer.Id} не найдено задание.";
    }
  }
}
