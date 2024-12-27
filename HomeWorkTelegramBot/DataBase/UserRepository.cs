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
  }
}
