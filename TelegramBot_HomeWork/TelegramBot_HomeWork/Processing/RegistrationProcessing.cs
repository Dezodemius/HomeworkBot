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
using TelegramBot.Roles.Student;

namespace TelegramBot.Processing
{
  /// <summary>
  /// Класс для обработки регистрационного процесса.
  /// </summary>
  public class RegistrationProcessing
  {
    private readonly RegistrationRequest _request;

    /// <summary>
    /// Инициализирует новый экземпляр класса RegistrationRequest.
    /// </summary>
    /// <param name="registrationRequest">Объект запроса на регистрацию.</param>
    public RegistrationProcessing(RegistrationRequest registrationRequest)
    {
      _request = registrationRequest;
      _request.Status = "Pending";
    }

    /// <summary>
    /// Обрабатывает текущий шаг регистрации.
    /// </summary>
    /// <param name="botClient">Клиент Telegram-бота.</param>
    /// <param name="chatId">Идентификатор чата Telegram.</param>
    /// <param name="message">Сообщение от пользователя.</param>
    public async Task ProcessRegistrationStepAsync(ITelegramBotClient botClient, long chatId, string message)
    {
      switch (_request.GetStep())
      {
        case RegistrationStep.Start:
          await ProcessCourseSelectionStepAsync(botClient, chatId);
          break;

        case RegistrationStep.Course:
          await ProcessCourseConfirmationStepAsync(botClient, chatId, message);
          break;

        case RegistrationStep.FirstName:
          await ProcessFirstNameStepAsync(botClient, chatId, message);
          break;

        case RegistrationStep.LastName:
          await ProcessLastNameStepAsync(botClient, chatId, message);
          break;

        case RegistrationStep.Email:
          await ProcessEmailStepAsync(botClient, chatId, message);
          break;

        case RegistrationStep.Completed:
          await SendCompletionMessageAsync(botClient, chatId);
          break;

        default:
          await TelegramBotHandler.SendMessageAsync(botClient, chatId, "Неизвестный шаг регистрации.");
          break;
      }
    }

    /// <summary>
    /// Отправляет пользователю список доступных курсов для выбора.
    /// </summary>
    private async Task ProcessCourseSelectionStepAsync(ITelegramBotClient botClient, long chatId)
    {
      var courses = Core.CommonCourseModel.GetAllCourses();
      var callbacks = courses.Select(course => new CallbackModel(course.CourseName, $"/courseId_{course.CourseId}")).ToList();

      var inlineMarkup = TelegramBotHandler.GetInlineKeyboardMarkupAsync(callbacks);
      await TelegramBotHandler.SendMessageAsync(botClient, chatId, "Выберите курс:", inlineMarkup);
      _request.SetStep(RegistrationStep.Course);
    }

    /// <summary>
    /// Обрабатывает выбор курса и запрашивает имя пользователя.
    /// </summary>
    private async Task ProcessCourseConfirmationStepAsync(ITelegramBotClient botClient, long chatId, string message)
    {
      var courseData = message.Split('_');
      if (int.TryParse(courseData.Last(), out int courseId))
      {
        var courseName = CommonCourseModel.GetNameCourse(courseId);
        _request.CourseId = courseId;
        _request.SetStep(RegistrationStep.FirstName);
        await TelegramBotHandler.SendMessageAsync(botClient, chatId, $"Вы выбрали курс \"{courseName}\". Теперь введите ваше имя:");
      }
      else
      {
        await TelegramBotHandler.SendMessageAsync(botClient, chatId, "Системная ошибка! Попробуйте ещё раз!");
      }
    }

    /// <summary>
    /// Сохраняет имя пользователя и запрашивает фамилию.
    /// </summary>
    private async Task ProcessFirstNameStepAsync(ITelegramBotClient botClient, long chatId, string message)
    {
      _request.FirstName = message;
      _request.SetStep(RegistrationStep.LastName);
      await TelegramBotHandler.SendMessageAsync(botClient, chatId, "Отлично! Теперь введите вашу фамилию:");
    }

    /// <summary>
    /// Сохраняет фамилию пользователя и запрашивает email.
    /// </summary>
    private async Task ProcessLastNameStepAsync(ITelegramBotClient botClient, long chatId, string message)
    {
      _request.LastName = message;
      _request.SetStep(RegistrationStep.Email);
      await TelegramBotHandler.SendMessageAsync(botClient, chatId, "Хорошо. Теперь введите ваш email:");
    }

    /// <summary>
    /// Завершает регистрацию, сохраняя email и отправляя уведомления.
    /// </summary>
    private async Task ProcessEmailStepAsync(ITelegramBotClient botClient, long chatId, string message)
    {
      _request.Email = message;
      _request.SetStep(RegistrationStep.Completed);

      await NotifyAdministratorsAsync(botClient);
      await TelegramBotHandler.SendMessageAsync(botClient, chatId, "Ваша заявка на регистрацию принята. Пожалуйста, ожидайте подтверждения от администратора.");
    }

    /// <summary>
    /// Отправляет сообщение пользователю о завершенной регистрации.
    /// </summary>
    private async Task SendCompletionMessageAsync(ITelegramBotClient botClient, long chatId)
    {
      await TelegramBotHandler.SendMessageAsync(botClient, chatId, "Ваша регистрация уже завершена. Ожидайте подтверждения от администратора.");
    }

    /// <summary>
    /// Уведомляет администраторов о новой заявке на регистрацию.
    /// </summary>
    private async Task NotifyAdministratorsAsync(ITelegramBotClient botClient)
    {
      CommonRegistrationRequest.AddRegistrationRequests(_request);
      var admins = CommonUserModel.GetAllAdministrators();
      var courseName = CommonCourseModel.GetNameCourse(_request.CourseId);

      var message = new StringBuilder()
        .AppendLine("Новая заявка от участника:")
        .AppendLine($"Фамилия: {_request.LastName}")
        .AppendLine($"Имя: {_request.FirstName}")
        .AppendLine($"Почта: {_request.Email}")
        .AppendLine($"Выбранный курс: {courseName}")
        .ToString();

      var callbacks = new List<CallbackModel>
      {
        new CallbackModel("Принять", $"/registration_accept_{_request.TelegramChatId}"),
        new CallbackModel("Отклонить", $"/registration_reject_{_request.TelegramChatId}")
      };

      foreach (var admin in admins)
      {
        await TelegramBotHandler.SendMessageAsync(botClient, admin.TelegramChatId, message, TelegramBotHandler.GetInlineKeyboardMarkupAsync(callbacks));
      }
    }

    /// <summary>
    /// Обрабатывает ответ администратора на заявку пользователя на регистрацию.
    /// </summary>
    public static async Task<bool> AnswerRegistrationUser(ITelegramBotClient botClient, long chatId, int messageId, string callbackData)
    {
      var data = callbackData.Split('_');
      var user = CommonRegistrationRequest.GetRegistrationRequests(Convert.ToInt64(data.Last()));
      var courseName = CommonCourseModel.GetNameCourse(user.CourseId);

      if (data[1].ToLower().Contains("accept"))
      {
        await ApproveRegistrationAsync(botClient, chatId, messageId, user, courseName);
        return true;
      }
      else
      {
        await RejectRegistrationAsync(botClient, chatId, messageId, user, courseName);
        return false;
      }
    }

    /// <summary>
    /// Одобряет заявку пользователя и создает записи заданий для студента.
    /// </summary>
    private static async Task ApproveRegistrationAsync(ITelegramBotClient botClient, long chatId, int messageId, RegistrationRequest user, string courseName)
    {
      CommonUserModel.AddStudent(user);
      CommonRegistrationRequest.DeleteRegistrationRequests(user);

      var homeworks = CommonHomeWork.GetHomeWork(user.CourseId);
      var student = CommonUserModel.GetUserById(user.TelegramChatId);

      foreach (var homework in homeworks)
      {
        var submission = new Submission
        {
          AssignmentId = homework.AssignmentId,
          StudentId = student.TelegramChatId,
          Status = Submission.StatusWork.Unfulfilled,
          CourseId = homework.CourseId,
        };
        CommonSubmission.AddSubmission(submission);
      }

      await TelegramBotHandler.SendMessageAsync(botClient, chatId, $"Студент {user.LastName} {user.FirstName} принят в курс \"{courseName}\"", null, messageId);
      await TelegramBotHandler.SendMessageAsync(botClient, user.TelegramChatId, $"Ваша заявка на вступление в курс \"{courseName}\" успешно принята!");
    }

    /// <summary>
    /// Отклоняет заявку пользователя и отправляет уведомление.
    /// </summary>
    private static async Task RejectRegistrationAsync(ITelegramBotClient botClient, long chatId, int messageId, RegistrationRequest user, string courseName)
    {
      CommonRegistrationRequest.UpdateStatusRegistrationRequests(user, "Rejected");
      await TelegramBotHandler.SendMessageAsync(botClient, chatId, $"Заявка студента {user.LastName} {user.FirstName} на вступление в курс \"{courseName}\" отклонена!", null, messageId);
      await TelegramBotHandler.SendMessageAsync(botClient, user.TelegramChatId, $"Ваша заявка на вступление в курс \"{courseName}\" отклонена.");
    }
  }
}
