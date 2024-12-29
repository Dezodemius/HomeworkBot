using HomeWorkTelegramBot.Config;
using HomeWorkTelegramBot.Models;
using System.Linq;

namespace HomeWorkTelegramBot.DataBase
{
  internal class UserRegistrationRepository
  {
    /// <summary>
    /// Добавляет новую запись о регистрации пользователя в базу данных.
    /// </summary>
    /// <param name="registration">Объект регистрации пользователя для добавления.</param>
    public void AddUserRegistration(UserRegistration registration)
    {
      ApplicationData.DbContext.UserRegistrations.Add(registration);
      ApplicationData.DbContext.SaveChanges();
    }

    /// <summary>
    /// Удаляет запись о регистрации пользователя из базы данных.
    /// </summary>
    /// <param name="chatId">Идентификатор чата пользователя.</param>
    public void DeleteUserRegistration(long chatId)
    {
      var registration = ApplicationData.DbContext.UserRegistrations
                          .FirstOrDefault(r => r.ChatId == chatId);
      if (registration != null)
      {
        ApplicationData.DbContext.UserRegistrations.Remove(registration);
        ApplicationData.DbContext.SaveChanges();
      }
    }

    /// <summary>
    /// Проверяет, существует ли запись о регистрации пользователя с данным ChatId.
    /// </summary>
    /// <param name="chatId">Идентификатор чата пользователя.</param>
    /// <returns>True, если запись существует, иначе false.</returns>
    public bool UserRegistrationExists(long chatId)
    {
      if (ApplicationDbContext.HasData<UserRegistration>())
      {
        return ApplicationData.DbContext.UserRegistrations.Any(r => r.ChatId == chatId);
      }
      else
      {
        return false;
      }
    }

    /// <summary>
    /// Возвращает запись о регистрации пользователя по идентификатору чата.
    /// </summary>
    /// <param name="chatId">Идентификатор чата пользователя.</param>
    /// <returns>Объект UserRegistration или null, если запись не найдена.</returns>
    public UserRegistration GetUserRegistrationByChatId(long chatId)
    {
      return ApplicationData.DbContext.UserRegistrations.FirstOrDefault(r => r.ChatId == chatId);
    }
  }
}