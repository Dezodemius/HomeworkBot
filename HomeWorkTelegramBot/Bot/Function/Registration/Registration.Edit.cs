using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot;

namespace HomeWorkTelegramBot.Bot.Function.Registration
{
  internal partial class Registration
  {
    /// <summary>
    /// Инициирует процесс редактирования имени пользователя.
    /// </summary>
    static private async Task EditName(ITelegramBotClient botClient, CallbackQuery callback, long chatId)
    {
      _userSteps[chatId] = RegistrationStep.Name;
      _hasChanges[chatId] = true;
      await TelegramBotHandler.SendMessageAsync(botClient, chatId, "Пожалуйста, введите ваше имя:", null, callback.Message.Id);
    }

    /// <summary>
    /// Инициирует процесс редактирования фамилии пользователя.
    /// </summary>
    static private async Task EditSurname(ITelegramBotClient botClient, CallbackQuery callback, long chatId)
    {
      _userSteps[chatId] = RegistrationStep.Surname;
      _hasChanges[chatId] = true;
      await TelegramBotHandler.SendMessageAsync(botClient, chatId, "Пожалуйста, введите вашу фамилию:", null, callback.Message.Id);
    }

    /// <summary>
    /// Инициирует процесс редактирования отчества пользователя.
    /// </summary>
    static private async Task EditLastname(ITelegramBotClient botClient, CallbackQuery callback, long chatId)
    {
      _userSteps[chatId] = RegistrationStep.Lastname;
      _hasChanges[chatId] = true;
      await TelegramBotHandler.SendMessageAsync(botClient, chatId, "Пожалуйста, введите ваше отчество:", null, callback.Message.Id);
    }

    /// <summary>
    /// Инициирует процесс редактирования электронной почты пользователя.
    /// </summary>
    static private async Task EditEmail(ITelegramBotClient botClient, CallbackQuery callback, long chatId)
    {
      _userSteps[chatId] = RegistrationStep.Email;
      _hasChanges[chatId] = true;
      await TelegramBotHandler.SendMessageAsync(botClient, chatId, "Пожалуйста, введите вашу электронную почту:", null, callback.Message.Id);
    }

    /// <summary>
    /// Инициирует процесс редактирования курса пользователя.
    /// </summary>
    static private async Task EditCourse(ITelegramBotClient botClient, long chatId)
    {
      _userSteps[chatId] = RegistrationStep.CourseSelection;
      _hasChanges[chatId] = true;
      await ProcessCourseSelection(botClient, chatId, _registrationData[chatId]);
    }
  }
}
