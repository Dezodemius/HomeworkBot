using HomeWorkTelegramBot.Models;
using Telegram.Bot.Types;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace HomeWorkTelegramBot.Bot
{
  internal class GetInlineKeyboard
  {
    /// <summary>
    /// Создает клавиатуру с курсами.
    /// </summary>
    /// <param name="courses">Список курсов.</param>
    /// <returns>Клавиатуру с данными о курсах.</returns>
    public static InlineKeyboardMarkup GetCoursesKeyboard(List<Courses> courses, string commandText, int page = 0)
    {
      return GetPagination(courses, commandText, page);
    }

    /// <summary>
    /// Создает клавиатуру со студентами.
    /// </summary>
    /// <param name="users">Список студентов курса.</param>
    /// <returns>Клавиатуру с данными о студентах.</returns>
    public static InlineKeyboardMarkup GetStudentsKeyboard(List<Models.User> users, int page = 0)
    {
      return GetPagination(users, "selectuser", page);
    }

    /// <summary>
    /// Создает клавиатуру со студентами.
    /// </summary>
    /// <param name="tasks">Список студентов курса.</param>
    /// <returns>Клавиатуру с данными о студентах.</returns>
    public static InlineKeyboardMarkup GetTaskKeyboard(List<TaskWork> tasks, int page = 0)
    {
      return GetPagination(tasks, "selecttask", page);
    }

    /// <summary>
    /// Создает клавиатуру с пагинацией для различных типов объектов.
    /// </summary>
    /// <typeparam name="T">Тип объекта (User, TaskWork или Courses).</typeparam>
    /// <param name="items">Список объектов.</param>
    /// <param name="commandText">Префикс команды.</param>
    /// <param name="page">Текущая страница.</param>
    /// <param name="itemsPerPage">Количество элементов на странице.</param>
    /// <returns>Клавиатура с кнопками и навигацией.</returns>
    private static InlineKeyboardMarkup GetPagination<T>(List<T> items, string commandText, int page = 1, int itemsPerPage = 2)
    {
      List<CallbackModel> callbackModels = new List<CallbackModel>();

      var totalPages = (int)Math.Ceiling(items.Count / (double)itemsPerPage);
      page = Math.Max(1, Math.Min(page, totalPages));
      var pageItems = items
        .Skip((page - 1) * itemsPerPage)
        .Take(itemsPerPage);

      GetDataButtons(commandText, callbackModels, pageItems);
      GetNavigationButtons(page, callbackModels, totalPages);

      return TelegramBotHandler.GetInlineKeyboardMarkupAsync(callbackModels);
    }

    /// <summary>
    /// Создает кнокпи с необходимыми даннными в зависимости от типа объекта.
    /// </summary>
    /// <typeparam name="T">Тип объекта (User, TaskWork или Courses).</typeparam>
    /// <param name="commandText">Префикс команды.</param>
    /// <param name="callbackModels">Список объектов класса CallbackModel.</param>
    /// <param name="pageItems">Список элементов на странице.</param>
    private static void GetDataButtons<T>(string commandText, List<CallbackModel> callbackModels, IEnumerable<T> pageItems)
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

          default:
            continue;
        }

        callbackModels.Add(new CallbackModel(buttonText, command));
      }
    }

    /// <summary>
    /// Формирует и добавляет кнопки навигации.
    /// </summary>
    /// <param name="page">Номер страницы.</param>
    /// <param name="callbackModels">Список объектов класса CallbackModel.</param>
    /// <param name="totalPages">Общее число страниц.</param>
    private static void GetNavigationButtons(int page, List<CallbackModel> callbackModels, int totalPages)
    {
      if (page > 1)
      {
        callbackModels.Add(new CallbackModel("◀️", $"page_{page - 1}"));
      }

      callbackModels.Add(new CallbackModel($"{page}/{totalPages}", "current_page"));

      if (page < totalPages)
      {
        callbackModels.Add(new CallbackModel("▶️", $"page_{page + 1}"));
      }
    }
  }
}
