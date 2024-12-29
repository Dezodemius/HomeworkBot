using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot;
using static HomeWorkTelegramBot.Config.Logger;
using HomeWorkTelegramBot.Core;
using HomeWorkTelegramBot.Models;

namespace HomeWorkTelegramBot.Bot.Function.Administrator
{
  /// <summary>
  /// Работа с заявкой на регистрацию пользователя.
  /// </summary>
  internal class NewUser
  {
    /// <summary>
    /// Обрабатывает нажатия на кнопки "Принять" и "Отказать".
    /// </summary>
    public async Task HandleCallbackQueryAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery)
    {
      long chatId = callbackQuery.Message.Chat.Id;
      string data = callbackQuery.Data;

      if (data.StartsWith("/approve_"))
      {
        long userId = long.Parse(data.Replace("/approve_", ""));
        await ApproveUser(botClient, callbackQuery, chatId, userId);
      }
      else if (data.StartsWith("/reject_"))
      {
        long userId = long.Parse(data.Replace("/reject_", ""));
        await RejectUser(botClient, callbackQuery, chatId, userId);
      }
    }

    /// <summary>
    /// Подтверждает регистрацию пользователя.
    /// </summary>
    private async Task ApproveUser(ITelegramBotClient botClient, CallbackQuery callbackQuery, long adminChatId, long userId)
    {
      var user = UserRegistrationService.GetUserRegistrationByChatId(userId);
      if (user == null)
      {
        LogWarning($"Регистрация пользователя с ChatId {userId} не найдена.");
        return;
      }

      var newUser = CreateUser(user);
      UserService.AddUser(newUser);

      EnrollUserInCourse(newUser, user.CourseId);

      AddDefaultAnswersForUser(user.CourseId, newUser.Id);

      LogInformation($"Пользователь с ChatId {userId} был принят.");
      await NotifyUserAndAdmin(botClient, callbackQuery, adminChatId, userId, "принят");
    }

    /// <summary>
    /// Создает нового пользователя на основе данных регистрации.
    /// </summary>
    private Models.User CreateUser(UserRegistration user)
    {
      return new Models.User
      {
        Name = user.Name,
        ChatId = user.ChatId,
        Lastname = user.Lastname,
        Surname = user.Surname,
        BirthDate = user.BirthDate,
        Email = user.Email,
        UserRole = Models.User.Role.Student,
      };
    }

    /// <summary>
    /// Зачисляет пользователя на курс.
    /// </summary>
    private void EnrollUserInCourse(Models.User user, int courseId)
    {
      var courseEnrollment = new CourseEnrollment
      {
        UserId = user.Id,
        CourseId = courseId,
      };
      CourseEnrollmentService.AddCourseEnrollment(courseEnrollment);
    }

    /// <summary>
    /// Добавляет ответы со статусом "без ответа" для всех заданий курса.
    /// </summary>
    private void AddDefaultAnswersForUser(int courseId, long userId)
    {
      var tasks = TaskWorkService.GetTaskWorksByCourseId(courseId);
      foreach (var task in tasks)
      {
        var answer = new Answer
        {
          AnswerText = string.Empty,
          CourseId = courseId,
          TaskId = task.Id,
          Date = DateTime.Now,
          Status = Answer.TaskStatus.NotAnswered,
        };
        AnswerService.AddAnswer(answer);
      }
    }

    /// <summary>
    /// Уведомляет пользователя и администратора о результате.
    /// </summary>
    private async Task NotifyUserAndAdmin(ITelegramBotClient botClient, CallbackQuery callbackQuery, long adminChatId, long userId, string result)
    {
      await TelegramBotHandler.SendMessageAsync(botClient, adminChatId, $"Пользователь с ChatId {userId} был {result}.", null, callbackQuery.Message.Id);
      await TelegramBotHandler.SendMessageAsync(botClient, userId, $"Ваша регистрация была {result} администратором.", null, callbackQuery.Message.Id);
    }

    /// <summary>
    /// Отклоняет регистрацию пользователя.
    /// </summary>
    private async Task RejectUser(ITelegramBotClient botClient, CallbackQuery callbackQuery, long adminChatId, long userId)
    {
      LogInformation($"Пользователь с ChatId {userId} был отклонен.");
      await NotifyUserAndAdmin(botClient, callbackQuery, adminChatId, userId, "отклонен");
    }

  }
}
