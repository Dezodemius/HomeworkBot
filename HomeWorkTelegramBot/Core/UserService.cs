﻿using HomeWorkTelegramBot.DataBase;
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

    /// <summary>
    /// Получает роль пользователя по идентификатору чата и логирует результат.
    /// </summary>
    /// <param name="chatId">Идентификатор чата пользователя.</param>
    /// <returns>Роль пользователя или null, если пользователь не найден.</returns>
    public static User.Role? GetUserRoleByChatId(long chatId)
    {
      var role = userRepository.GetUserRoleByChatId(chatId);
      if (role.HasValue)
      {
        LogInformation($"Роль пользователя с ChatId {chatId}: {role.Value}");
      }
      else
      {
        LogWarning($"Пользователь с ChatId {chatId} не найден.");
      }

      return role;
    }
  }
}
