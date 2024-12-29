using HomeWorkTelegramBot.DataBase;
using HomeWorkTelegramBot.Models;
using static HomeWorkTelegramBot.Config.Logger;

namespace HomeWorkTelegramBot.Core
{
  internal class UserRegistrationService
  {
    private static readonly UserRegistrationRepository registrationRepository = new UserRegistrationRepository();

    /// <summary>
    /// Добавляет новую запись о регистрации пользователя в базу данных и логирует это действие.
    /// </summary>
    /// <param name="registration">Объект регистрации пользователя для добавления.</param>
    public static void AddUserRegistration(UserRegistration registration)
    {
      registrationRepository.AddUserRegistration(registration);
      LogInformation($"Добавлена новая регистрация пользователя: ChatId - {registration.ChatId}");
    }

    /// <summary>
    /// Удаляет запись о регистрации пользователя из базы данных и логирует это действие.
    /// </summary>
    /// <param name="chatId">Идентификатор чата пользователя.</param>
    public static void DeleteUserRegistration(long chatId)
    {
      registrationRepository.DeleteUserRegistration(chatId);
      LogInformation($"Удалена регистрация пользователя с ChatId {chatId}");
    }

    /// <summary>
    /// Проверяет, существует ли запись о регистрации пользователя с данным ChatId.
    /// </summary>
    /// <param name="chatId">Идентификатор чата пользователя.</param>
    /// <returns>True, если запись существует, иначе false.</returns>
    public static bool UserRegistrationExists(long chatId)
    {
      try
      {
        var exists = registrationRepository.UserRegistrationExists(chatId);
        LogInformation($"Проверка существования регистрации пользователя с ChatId {chatId}: {exists}");
        return exists;
      }
      catch (Exception ex)
      {
        LogError($"Ошибка при проверке записи о регистрации пользователя: {ex}");
        throw;
      }
    }
  }
}
