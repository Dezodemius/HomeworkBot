using HomeWorkTelegramBot.DataBase;
using HomeWorkTelegramBot.Models;
using static HomeWorkTelegramBot.Config.Logger;

namespace HomeWorkTelegramBot.Core
{
  internal static class UserService
  {
    private static readonly UserRepository userRepository = new UserRepository();

    public static void AddUser(User user)
    {
      userRepository.AddUser(user);
      LogInformation($"Добавлен новый пользователь: ChatId - {user.ChatId}");
    }
  }
}
