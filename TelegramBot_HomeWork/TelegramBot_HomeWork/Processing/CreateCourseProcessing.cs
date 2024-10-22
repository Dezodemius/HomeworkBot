using Core;
using DataContracts.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Requests.Abstractions;
using TelegramBot.Model;
using TelegramBot.Roles.Administrator;

namespace TelegramBot.Processing
{
  internal class CreateCourseProcessing
  {

    private Course course;

    /// <summary>
    /// Инициализирует новый экземпляр класса RegistrationRequest.
    /// </summary>
    /// <param name="telegramChatId">Идентификатор чата Telegram.</param>
    public CreateCourseProcessing(Course course)
    {
      this.course = course;
    }

    /// <summary>
    /// Обрабатывает текущий шаг регистрации.
    /// </summary>
    /// <param name="message">Сообщение от пользователя.</param>
    /// <param name="dbManager">Менеджер базы данных.</param>
    /// <returns>Ответ на текущий шаг регистрации.</returns>
    public async Task ProcessCreateCourseStepAsync(ITelegramBotClient botClient, long chatId, string message, int messageId)
    {
      switch (course.GetStep())
      {

        case CreateStep.Start:
          {
            string messageData = "Отлично. Для начала, выберите преподавателя, который будет вести курсы:";
            List<CallbackModel> callbackModels = new List<CallbackModel>();
            var teachers = CommonUserModel.GetAllteachers();
            if (teachers.Count == 0)
            {
              await TelegramBotHandler.SendMessageAsync(botClient, chatId, $"К сожалению, вы не можете создать курс без преподавателя. Добавьте преподавателя и попробуйте попытку снова", null, messageId);
              Administrator.course.Remove(chatId);
              return;
            }
            foreach (var teacher in teachers)
            {
              callbackModels.Add(new CallbackModel($"{teacher.LastName} {teacher.FirstName}", $"/createCourse_{teacher.TelegramChatId}"));
            }
            course.SetStep(CreateStep.Teacher);

            await TelegramBotHandler.SendMessageAsync(botClient, chatId, messageData, TelegramBotHandler.GetInlineKeyboardMarkupAsync(callbackModels), messageId);
            return;
          }
        case CreateStep.Teacher:
          {
            var teacherData = message.Split('_');
            if (long.TryParse(teacherData.Last(), out long teacherDataId))
            {
              course.TeacherId = teacherDataId;
              course.SetStep(CreateStep.Name);
              var teacher = CommonUserModel.GetAllteachers().ToList().First();
              await TelegramBotHandler.SendMessageAsync(botClient, chatId, $"Вы выбрали преподавателя {teacher.LastName} {teacher.FirstName}. Теперь введите название курса:", null, messageId);
              return;
            }
            else
            {
              await TelegramBotHandler.SendMessageAsync(botClient, chatId, $"Системная ошибка! Попробуйте ещё раз!");
              return;
            }
          }
        case CreateStep.Name:
          {
            course.CourseName = message;
            course.SetStep(CreateStep.Completed);
            await NewCourseAsync(course, botClient, chatId);
            return;
          }


        default:
          await TelegramBotHandler.SendMessageAsync(botClient, chatId, "Неизвестный шаг регистрации.");
          return;
      }
    }

    private async Task NewCourseAsync(Course course, ITelegramBotClient botClient, long chatId)
    {
      CommonCourseModel.AddCourse(course);
      Administrator.course.Remove(chatId);
      await TelegramBotHandler.SendMessageAsync(botClient, chatId, $"Создан новый курс {course.CourseName}");
    }

  }
}
