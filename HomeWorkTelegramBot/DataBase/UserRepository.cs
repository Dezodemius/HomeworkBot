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

  }
}
