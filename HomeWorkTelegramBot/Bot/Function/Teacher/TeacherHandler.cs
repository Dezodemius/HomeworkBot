using HomeWorkTelegramBot.Core;
using HomeWorkTelegramBot.DataBase;
using HomeWorkTelegramBot.Models;
using Microsoft.EntityFrameworkCore;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using static HomeWorkTelegramBot.Config.Logger;

namespace HomeWorkTelegramBot.Bot.Function.Teacher
{
  internal class TeacherHandler : IRoleHandler
  {
    private static int _selectedCourseId = -1;
    public async Task HandleMessageAsync(ITelegramBotClient botClient, Message message)
    {
      if (CreateTaskWork._creationData.Count != 0)
      {
        await new NewTaskWork().HandleMessageAsync(botClient, message);
      }
      else
      {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("Добро пожаловать в панель преподавателя. Выберите действие:");
        var keyboard = new InlineKeyboardMarkup(new[]
        {
        new[]
            {
                InlineKeyboardButton.WithCallbackData("Создать новое домашнее задание", "/createhw"),
            },
        new[]
            {
                InlineKeyboardButton.WithCallbackData("Посмотреть статусы домашних заданий студента", "/studhwstat"),
            },
        new[]
            {
                InlineKeyboardButton.WithCallbackData("Посмотреть выполнение домашнего задания", "/hwstatistics"),
            },
        });
        await TelegramBotHandler.SendMessageAsync(botClient, message.Chat.Id, sb.ToString(), keyboard);
      }
    }

    public async Task HandleCallback(ITelegramBotClient botClient, CallbackQuery callbackQuery)
    {
      if (callbackQuery.Data.StartsWith("page_"))
      {
        int page = int.Parse(callbackQuery.Data.Split('_')[1]);
        await HandlePagination(botClient, callbackQuery, page);
        return;
      }

      if (callbackQuery.Data.StartsWith("/selectcourse_"))
      {
        string[] parts = callbackQuery.Data.Split('_');
        if (parts.Length > 2 && int.TryParse(parts[2], out int courseId))
        {
          _selectedCourseId = courseId;
        }
      }

      var commandHandlers = new Dictionary<string, Func<Task>>
      {
        { "/createhw", async () => await new NewTaskWork().HandleCallback(botClient, callbackQuery) },
        { "/selectcourse_nt", async () => await new NewTaskWork().HandleCallback(botClient, callbackQuery) },

        { "/studhwstat", async () => await new GetStudentStatistics().HandleCallbackQueryAsync(botClient, callbackQuery) },
        { "/selectcourse_sd", async () => await new GetStudentStatistics().HandleCallbackQueryAsync(botClient, callbackQuery) },
        { "/selectuser_", async () => await new GetStudentStatistics().HandleCallbackQueryAsync(botClient, callbackQuery) },

        { "/hwstatistics", async () => await new GetTaskWorkStatistics().HandleCallbackQueryAsync(botClient, callbackQuery) },
        { "/selectcourse_tw", async () => await new GetTaskWorkStatistics().HandleCallbackQueryAsync(botClient, callbackQuery) },
        { "/selecttask_", async () => await new GetTaskWorkStatistics().HandleCallbackQueryAsync(botClient, callbackQuery) },
      };

      foreach (var command in commandHandlers.Keys)
      {
        if (callbackQuery.Data.StartsWith(command))
        {
          LogInformation($"Выполняется обработчик для команды: {command}");
          await commandHandlers[command]();
          return;
        }
      }
    }

    public async Task HandlePagination(ITelegramBotClient botClient, CallbackQuery callbackQuery, int page)
    {
      var messageText = callbackQuery.Message.Text;
      InlineKeyboardMarkup newKeyboard = null;

      using (var context = new ApplicationDbContext())
      {
        if (messageText.Contains("выберите курс"))
        {
          var courses = await context.Courses
            .Where(c => c.TeacherId == callbackQuery.From.Id)
            .ToListAsync();
          string commandText = messageText.Contains("статистики") ? "selectcourse_tw" : "selectcourse_sd";
          newKeyboard = GetInlineKeyboard.GetCoursesKeyboard(courses, commandText, page);
        }
        else if (messageText.Contains("студент"))
        {
          var studentsId = CourseEnrollmentService.GetAllUsersCourseEnrollments(_selectedCourseId);
          var students = new List<Models.User>();
          foreach (var student in studentsId)
          {
            var foundStudent = UserService.GetUserById(student.Id);
            if (foundStudent != null)
            {
              students.Add(foundStudent);
            }
          }

          newKeyboard = GetInlineKeyboard.GetStudentsKeyboard(students, page);
        }
        else if (messageText.Contains("задание"))
        {
          var tasks = await context.TaskWorks
            .Where(tw => tw.CourseId == _selectedCourseId)
            .ToListAsync();
          newKeyboard = GetInlineKeyboard.GetTaskKeyboard(tasks, page);
        }
      }

      if (newKeyboard != null)
      {
        await TelegramBotHandler.SendMessageAsync(botClient, callbackQuery.Message.Chat.Id, callbackQuery.Message.Text, newKeyboard, callbackQuery.Message.Id);
      }
    }

    public async Task HandleStartButton(ITelegramBotClient botClient, long chatId)
    {
      throw new NotImplementedException();
    }
  }
}
