using HomeWorkTelegramBot.Config;
using HomeWorkTelegramBot.Models;

namespace HomeWorkTelegramBot.DataBase
{
  internal class UserRepository
  {
    /// <summary>
    /// Добавляет нового пользователя в базу данных.
    /// </summary>
    /// <param name="user">Объект пользователя для добавления.</param>
    public void AddUser(User user)
    {
      ApplicationData.DbContext.Users.Add(user);
      ApplicationData.DbContext.SaveChanges();
    }

    /// <summary>
    /// Возвращает роль пользователя по идентификатору чата.
    /// </summary>
    /// <param name="chatId">Идентификатор чата пользователя.</param>
    /// <returns>Роль пользователя или null, если пользователь не найден.</returns>
    public User.Role? GetUserRoleByChatId(long chatId)
    {
      var user = ApplicationData.DbContext.Users
                  .FirstOrDefault(u => u.ChatId == chatId);
      return user?.UserRole;
    }

    /// <summary>
    /// Проверяет, существует ли пользователь с данным ChatId.
    /// </summary>
    /// <param name="chatId">Идентификатор чата пользователя.</param>
    /// <returns>True, если пользователь существует, иначе false.</returns>
    public bool UserExists(long chatId)
    {
      return ApplicationData.DbContext.Users.Any(u => u.ChatId == chatId);
    }

    /// <summary>
    /// Возвращает пользователя по идентификатору чата.
    /// </summary>
    /// <param name="chatId">Идентификатор чата пользователя.</param>
    /// <returns>Объект User или null, если пользователь не найден.</returns>
    public User GetUserByChatId(long chatId)
    {
      return ApplicationData.DbContext.Users.FirstOrDefault(u => u.ChatId == chatId);
    }

    /// <summary>
    /// Возвращает пользователя по уникальному идентификатору.
    /// </summary>
    /// <param name="userId">Идентификатор пользователя.</param>
    /// <returns>Объект User или null, если пользователь не найден.</returns>
    public User GetUserById(int userId)
    {
      return ApplicationData.DbContext.Users.FirstOrDefault(u => u.Id == userId);
    }

    /// <summary>
    /// Возвращает список всех пользователей.
    /// </summary>
    /// <returns>Список объектов User.</returns>
    public List<User> GetAllUsers()
    {
      return ApplicationData.DbContext.Users.ToList();
    }

    /// <summary>
    /// Обновляет информацию о пользователе в базе данных.
    /// </summary>
    /// <param name="user">Объект пользователя для обновления.</param>
    public void UpdateUser(User user)
    {
      ApplicationData.DbContext.Users.Update(user);
      ApplicationData.DbContext.SaveChanges();
    }

    /// <summary>
    /// Удаляет пользователя из базы данных по идентификатору чата.
    /// </summary>
    /// <param name="chatId">Идентификатор чата пользователя.</param>
    public void DeleteUser(long chatId)
    {
      var user = ApplicationData.DbContext.Users.FirstOrDefault(u => u.ChatId == chatId);
      if (user != null)
      {
        ApplicationData.DbContext.Users.Remove(user);
        ApplicationData.DbContext.SaveChanges();
      }
    }

  }
}
