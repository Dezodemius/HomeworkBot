using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core;
using Database;
using DataContracts.Models;
using Telegram.Bot;
using TelegramBot.Model;

namespace TelegramBot.Processing
{
  public class RegistrationProcessing
  {

    RegistrationRequest request = new RegistrationRequest();

    /// <summary>
    /// Инициализирует новый экземпляр класса RegistrationRequest.
    /// </summary>
    /// <param name="telegramChatId">Идентификатор чата Telegram.</param>
    public RegistrationProcessing(RegistrationRequest registrationRequest)
    {
      request = registrationRequest;
      request.Status = "Pending";
    }

    /// <summary>
    /// Обрабатывает текущий шаг регистрации.
    /// </summary>
    /// <param name="message">Сообщение от пользователя.</param>
    /// <param name="dbManager">Менеджер базы данных.</param>
    /// <returns>Ответ на текущий шаг регистрации.</returns>
    public async Task ProcessRegistrationStepAsync(ITelegramBotClient botClient, long chatId, string message)
    {
      switch (request.GetStep())
      {
        case RegistrationStep.Start:
          {

            var course = Core.CommonCourseModel.GetAllCourses();
            List<CallbackModel> callbacks = new List<CallbackModel>();
            foreach (var callback in course)
            {
              callbacks.Add(new CallbackModel(callback.CourseName, $"/courseId_{callback.CourseId.ToString()}"));
            }
            var inlineMarkup = TelegramBotHandler.GetInlineKeyboardMarkupAsync(callbacks);
            await TelegramBotHandler.SendMessageAsync(botClient, chatId, "Выберите курс:", inlineMarkup);
            
            request.SetStep(RegistrationStep.Course);
            return;
          }
        case RegistrationStep.Course:

          var courseData = message.Split('_');
          if (int.TryParse(courseData.Last(), out int courseId))
          {
            var nameCourse = CommonCourseModel.GetNameCourse(courseId);
            request.CourseId = courseId;
            request.SetStep(RegistrationStep.FirstName);
            await TelegramBotHandler.SendMessageAsync(botClient, chatId, $"Вы выбрали курс \"{nameCourse}\". Теперь введите ваше имя:");
            return;
          }
          else
          {
            var courses = Core.CommonCourseModel.GetAllCourses();
            await TelegramBotHandler.SendMessageAsync(botClient, chatId, $"Выберите курс:\n{string.Join("\n", courses.Select(c => $"{c.CourseId}. {c.CourseName}"))}");
            return;
          }

        case RegistrationStep.FirstName:
          request.FirstName = message;
          request.SetStep(RegistrationStep.LastName);
          await TelegramBotHandler.SendMessageAsync(botClient, chatId, "Отлично! Теперь введите вашу фамилию:");
          return;

        case RegistrationStep.LastName:
          request.LastName = message;
          request.SetStep(RegistrationStep.Email);
          await TelegramBotHandler.SendMessageAsync(botClient, chatId, "Хорошо. Теперь введите ваш email:");
          return;

        case RegistrationStep.Email:
          request.Email = message;
          request.SetStep(RegistrationStep.Completed);

          //await dbManager.AddRegistrationRequestAsync(this);
          await TelegramBotHandler.SendMessageAsync(botClient, chatId, "Ваша заявка на регистрацию принята. Пожалуйста, ожидайте подтверждения от администратора.");
          return;

        case RegistrationStep.Completed:
          await TelegramBotHandler.SendMessageAsync(botClient, chatId, "Ваша регистрация уже завершена. Ожидайте подтверждения от администратора.");
          return;

        default:
          await TelegramBotHandler.SendMessageAsync(botClient, chatId, "Неизвестный шаг регистрации.");
          return;
      }
    }
  }
}

