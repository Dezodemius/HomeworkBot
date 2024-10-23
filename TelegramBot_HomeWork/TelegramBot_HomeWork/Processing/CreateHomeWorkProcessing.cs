using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core;
using DataContracts.Models;
using Telegram.Bot;
using Telegram.Bot.Requests.Abstractions;
using Telegram.Bot.Types;
using TelegramBot.Model;
using TelegramBot.Roles.Administrator;
using TelegramBot.Roles.Teacher;

namespace TelegramBot.Processing
{
  internal class CreateHomeWorkProcessing
  {

    Assignment _assignment;

    internal CreateHomeWorkProcessing(Assignment assignment)
    {
      _assignment = assignment;
    }

    /// <summary>
    /// Обрабатывает текущий шаг регистрации.
    /// </summary>
    /// <param name="message">Сообщение от пользователя.</param>
    /// <param name="dbManager">Менеджер базы данных.</param>
    /// <returns>Ответ на текущий шаг регистрации.</returns>
    public async Task ProcessCreateStepAsync(ITelegramBotClient botClient, long chatId, string message)
    {
      switch (_assignment.GetStep())
      {
        case CreateHomeworkStep.Start:
          {
            var course = Core.CommonCourseModel.GetAllCourses();
            List<CallbackModel> callbacks = new List<CallbackModel>();
            foreach (var callback in course)
            {
              callbacks.Add(new CallbackModel(callback.CourseName, $"/createHomework_courseId_{callback.CourseId.ToString()}"));
            }
            var inlineMarkup = TelegramBotHandler.GetInlineKeyboardMarkupAsync(callbacks);
            await TelegramBotHandler.SendMessageAsync(botClient, chatId, "Выберите курс:", inlineMarkup);

            _assignment.SetStep(CreateHomeworkStep.Course);
            return;
          }

        case CreateHomeworkStep.Course:
          {
            var courseData = message.Split('_');
            if (int.TryParse(courseData.Last(), out int courseId))
            {
              var nameCourse = CommonCourseModel.GetNameCourse(courseId);
              _assignment.CourseId = courseId;
              _assignment.SetStep(CreateHomeworkStep.Name);
              await TelegramBotHandler.SendMessageAsync(botClient, chatId, $"Вы выбрали курс \"{nameCourse}\". Теперь введите название домашней работы:");
              return;
            }
            else
            {
              await TelegramBotHandler.SendMessageAsync(botClient, chatId, $"Системная ошибка! Попробуйте ещё раз!");
              return;
            }
          }

        case CreateHomeworkStep.Name:
          {
            _assignment.Title = message;
            _assignment.SetStep(CreateHomeworkStep.Description);
            await TelegramBotHandler.SendMessageAsync(botClient, chatId, "Отлично! Теперь введите описание:");
            return;
          }

        case CreateHomeworkStep.Description:
          {
            _assignment.Description = message;
            _assignment.SetStep(CreateHomeworkStep.DueDate);
            await TelegramBotHandler.SendMessageAsync(botClient, chatId, "Отлично! Теперь введите дедлайн в формате (dd.mm.yyyy):");
            return;
          }

        case CreateHomeworkStep.DueDate:
          {
            if (DateTime.TryParseExact(message, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dueDate))
            {
              dueDate = dueDate.Add(new TimeSpan(23, 59, 59));

              if (dueDate > DateTime.Now)
              {
                _assignment.DueDate = dueDate;
                _assignment.SetStep(CreateHomeworkStep.Completed);
                await NewHomeworkAsync(_assignment, botClient, chatId);
              }
              else
              {
                await TelegramBotHandler.SendMessageAsync(botClient, chatId, "Дата дедлайна должна быть позже сегодняшнего дня. Попробуйте ещё раз.");
              }
            }
            else
            {
              await TelegramBotHandler.SendMessageAsync(botClient, chatId, "Некорректный формат даты. Введите дату в формате dd.mm.yyyy.");
            }
            return;
          }

        case CreateHomeworkStep.Completed:
          {
            return;
          }

        default:
          {
            await TelegramBotHandler.SendMessageAsync(botClient, chatId, "Неизвестный шаг создания домашней работы.");
            return;
          }
      }

    }
    private async Task NewHomeworkAsync(Assignment request, ITelegramBotClient botClient, long chatId)
    {
      CommonHomeWork.AddHomeWork(request);
      Teacher.assigments.Remove(chatId);
      await TelegramBotHandler.SendMessageAsync(botClient, chatId, $"Создана новая домашняя работа  {request.Title}");
    }
  }
}
