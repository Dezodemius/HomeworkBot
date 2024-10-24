using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core;
using Database;
using DataContracts.Models;
using Telegram.Bot;
using Telegram.Bot.Requests.Abstractions;
using TelegramBot.Model;
using TelegramBot.Roles.Student;

namespace TelegramBot.Processing
{
  public class RegistrationProcessing
  {

    RegistrationRequest _request;

    /// <summary>
    /// Инициализирует новый экземпляр класса RegistrationRequest.
    /// </summary>
    /// <param name="telegramChatId">Идентификатор чата Telegram.</param>
    public RegistrationProcessing(RegistrationRequest registrationRequest)
    {
      _request = registrationRequest;
      _request.Status = "Pending";
    }

    /// <summary>
    /// Обрабатывает текущий шаг регистрации.
    /// </summary>
    /// <param name="message">Сообщение от пользователя.</param>
    /// <param name="dbManager">Менеджер базы данных.</param>
    /// <returns>Ответ на текущий шаг регистрации.</returns>
    public async Task ProcessRegistrationStepAsync(ITelegramBotClient botClient, long chatId, string message)
    {
      switch (_request.GetStep())
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

            _request.SetStep(RegistrationStep.Course);
            return;
          }
        case RegistrationStep.Course:
          {
            var courseData = message.Split('_');
            if (int.TryParse(courseData.Last(), out int courseId))
            {
              var nameCourse = CommonCourseModel.GetNameCourse(courseId);
              _request.CourseId = courseId;
              _request.SetStep(RegistrationStep.FirstName);
              await TelegramBotHandler.SendMessageAsync(botClient, chatId, $"Вы выбрали курс \"{nameCourse}\". Теперь введите ваше имя:");
              return;
            }
            else
            {
              await TelegramBotHandler.SendMessageAsync(botClient, chatId, $"Системная ошибка! Попробуйте ещё раз!");
              return;
            }
          }

        case RegistrationStep.FirstName:
          _request.FirstName = message;
          _request.SetStep(RegistrationStep.LastName);
          await TelegramBotHandler.SendMessageAsync(botClient, chatId, "Отлично! Теперь введите вашу фамилию:");
          return;

        case RegistrationStep.LastName:
          _request.LastName = message;
          _request.SetStep(RegistrationStep.Email);
          await TelegramBotHandler.SendMessageAsync(botClient, chatId, "Хорошо. Теперь введите ваш email:");
          return;

        case RegistrationStep.Email:
          _request.Email = message;
          _request.SetStep(RegistrationStep.Completed);

          await NewUserAsync(_request, botClient);
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

    private async Task NewUserAsync(RegistrationRequest request, ITelegramBotClient botClient)
    {
      CommonRegistrationRequest.AddRegistrationRequests(request);
      var data = CommonUserModel.GetAllAdministrators();
      List<CallbackModel> callbackModels = new List<CallbackModel>()
      {
        new CallbackModel("Принять", $"/registration_accept_{request.TelegramChatId}"),
        new CallbackModel("Отклонить", $"/registration_reject_{request.TelegramChatId}"),
      };

      StringBuilder stringBuilder = new StringBuilder();
      stringBuilder.AppendLine("Новая заявка от участники:");
      stringBuilder.AppendLine($"Фамилия: {request.LastName};");
      stringBuilder.AppendLine($"Имя: {request.FirstName};");
      stringBuilder.AppendLine($"Почта: {request.Email};");
      var nameCourse = CommonCourseModel.GetNameCourse(request.CourseId);
      stringBuilder.AppendLine($"Выбранный курс: {nameCourse}.");
      foreach (var item in data)
      {
        await TelegramBotHandler.SendMessageAsync(botClient, item.TelegramChatId, stringBuilder.ToString(), TelegramBotHandler.GetInlineKeyboardMarkupAsync(callbackModels));
      }
    }

    static internal async Task<bool> AnswerRegistrationUser(ITelegramBotClient botClient, long chatId, int messageId, string callbackData)
    {
      var data = callbackData.Split('_');
      var user = CommonRegistrationRequest.GetRegistrationRequests(Convert.ToInt64(data.Last()));
      var nameCourse = CommonCourseModel.GetNameCourse(user.CourseId);

      if (data[1].ToLower().Contains("accept"))
      {
        CommonUserModel.AddStudent(user);
        CommonRegistrationRequest.DeleteRegistrationRequests(user);
        var homeWorks = CommonHomeWork.GetHomeWork(user.CourseId);
        var student = CommonUserModel.GetUserById(user.TelegramChatId);

        foreach (var item in homeWorks)
        {
          Submission submission = new Submission
          {
            AssignmentId = item.AssignmentId,
            StudentId = student.UserId,
            Status = Submission.StatusWork.Unfulfilled,
            CourseId = item.CourseId,
          };

          CommonSubmission.AddSubmission(submission);
        }

        await TelegramBotHandler.SendMessageAsync(botClient, chatId, $"Студент {user.LastName} {user.FirstName} принят в курс \"{nameCourse}\"", null, messageId);
        await TelegramBotHandler.SendMessageAsync(botClient, user.TelegramChatId, $"Ваша заявка на вступление в курс \"{nameCourse}\" успешно принята!");
        return true;
      }
      else
      {
        CommonRegistrationRequest.UpdateStatusRegistrationRequests(user, "Rejected");
        await TelegramBotHandler.SendMessageAsync(botClient, chatId, $"Заявка студента {user.LastName} {user.FirstName} на вступление в курс \"{nameCourse}\" отклонена!", null, messageId);
        await TelegramBotHandler.SendMessageAsync(botClient, user.TelegramChatId, $"Ваша заявка на вступление в курс \"{nameCourse}\" отклонена!");
        return false;
      }

    }
  }
}

