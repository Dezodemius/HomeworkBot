using HomeWorkTelegramBot.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace HomeWorkTelegramBot.Bot.Function.Administrator
{
  internal class DeleteCourse
  {
    /// <summary>
    /// Отображает список курсов для удаления.
    /// </summary>
    public async Task ShowCoursesForDeletionAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery)
    {
      var courses = CourseService.GetAllCourses();
      var buttons = courses.Select(course => new List<InlineKeyboardButton>
            {
                InlineKeyboardButton.WithCallbackData(course.Name, $"/deleteCourse:{course.Id}")
            }).ToList();

      var inlineKeyboard = new InlineKeyboardMarkup(buttons);
      await TelegramBotHandler.SendMessageAsync(botClient, callbackQuery.From.Id, "Выберите курс для удаления:", inlineKeyboard, callbackQuery.Message.Id);
    }

    /// <summary>
    /// Удаляет курс и все связанные с ним данные.
    /// </summary>
    public async Task DeleteCourseAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery)
    {
      var chatId = callbackQuery.Message.Chat.Id;
      var data = callbackQuery.Data;

      if (data.StartsWith("/deleteCourse:"))
      {
        var courseId = int.Parse(data.Split(':')[1]);

        var enrollments = CourseEnrollmentService.GetAllUsersCourseEnrollments(courseId);
        foreach (var enrollment in enrollments)
        {
          var userEnrollments = CourseEnrollmentService.GetAllUserCourseEnrollments(enrollment.UserId);
          if (userEnrollments.Count == 1) 
          {
            var user = UserService.GetUserByChatId(enrollment.UserId);
            UserService.DeleteUser(enrollment.UserId);
          }

          CourseEnrollmentService.DeleteCourseEnrollment(enrollment.Id);
        }

        var answers = AnswerService.GetAnswersByTaskId(courseId);
        foreach (var answer in answers)
        {
          AnswerService.DeleteAnswer(answer.Id);
        }

        var taskWorks = TaskWorkService.GetTaskWorksByCourseId(courseId);
        foreach (var taskWork in taskWorks)
        {
          TaskWorkService.DeleteTaskWork(taskWork.Id);
        }

        CourseService.RemoveCourse(courseId);

        await TelegramBotHandler.SendMessageAsync(botClient, chatId, "Курс и все связанные данные успешно удалены.", null, callbackQuery.Message.Id);
      }
    }
  }
}
