using System;
using System.Collections.Generic;
using System.Data.SQLite;
using DataContracts.Models;
using DataContracts;

namespace Database
{
  public partial class DatabaseManager
  {
    /// <summary>
    /// Добавляет нового пользователя в систему.
    /// </summary>
    /// <param name="user">Модель пользователя</param>
    public void CreateUser(UserModel user)
    {
      using var connection = new SQLiteConnection(_connectionString);
      connection.Open();

      string query = "INSERT INTO Users (TelegramChatId, FirstName, LastName, Email, Role) " +
                     "VALUES (@TelegramChatId, @FirstName, @LastName, @Email, @Role)";

      using var command = new SQLiteCommand(query, connection);
      command.Parameters.AddWithValue("@TelegramChatId", user.TelegramChatId);
      command.Parameters.AddWithValue("@FirstName", user.FirstName);
      command.Parameters.AddWithValue("@LastName", user.LastName);
      command.Parameters.AddWithValue("@Email", user.Email);
      command.Parameters.AddWithValue("@Role", user.Role.ToString());

      command.ExecuteNonQuery();
      Logger.LogInfo($"Пользователь {user.FirstName} {user.LastName} добавлен в систему.");
    }

    /// <summary>
    /// Удаляет пользователя из системы по TelegramChatId.
    /// </summary>
    /// <param name="telegramChatId">Идентификатор чата Telegram</param>
    public void DeleteUser(long telegramChatId)
    {
      using var connection = new SQLiteConnection(_connectionString);
      connection.Open();

      string query = "DELETE FROM Users WHERE TelegramChatId = @TelegramChatId";
      using var command = new SQLiteCommand(query, connection);
      command.Parameters.AddWithValue("@TelegramChatId", telegramChatId);

      int rowsAffected = command.ExecuteNonQuery();
      if (rowsAffected > 0)
      {
        Logger.LogInfo($"Пользователь с TelegramChatId {telegramChatId} удалён.");
      }
      else
      {
        Logger.LogError($"Пользователь с TelegramChatId {telegramChatId} не найден.");
      }
    }

    /// <summary>
    /// Обновляет информацию о пользователе в системе.
    /// </summary>
    /// <param name="user">Модель пользователя</param>
    public void UpdateUser(UserModel user)
    {
      using var connection = new SQLiteConnection(_connectionString);
      connection.Open();

      string query = "UPDATE Users " +
                     "SET FirstName = @FirstName, LastName = @LastName, Email = @Email, Role = @Role " +
                     "WHERE TelegramChatId = @TelegramChatId";
      using var command = new SQLiteCommand(query, connection);
      command.Parameters.AddWithValue("@FirstName", user.FirstName);
      command.Parameters.AddWithValue("@LastName", user.LastName);
      command.Parameters.AddWithValue("@Email", user.Email);
      command.Parameters.AddWithValue("@Role", user.Role.ToString());
      command.Parameters.AddWithValue("@TelegramChatId", user.TelegramChatId);

      int rowsAffected = command.ExecuteNonQuery();
      if (rowsAffected > 0)
      {
        Logger.LogInfo($"Информация о пользователе {user.FirstName} {user.LastName} обновлена.");
      }
      else
      {
        Logger.LogError($"Пользователь с TelegramChatId {user.TelegramChatId} не найден для обновления.");
      }
    }

    /// <summary>
    /// Возвращает всех пользователей из системы.
    /// </summary>
    /// <returns>Список пользователей</returns>
    public List<UserModel> GetAllUsers()
    {
      using var connection = new SQLiteConnection(_connectionString);
      connection.Open();

      string query = "SELECT TelegramChatId, FirstName, LastName, Email, Role FROM Users";
      using var command = new SQLiteCommand(query, connection);

      using var reader = command.ExecuteReader();
      var users = new List<UserModel>();

      while (reader.Read())
      {
        var user = new UserModel(
          telegramChatId: Convert.ToInt64(reader["TelegramChatId"]),
          firstName: reader["FirstName"].ToString(),
          lastName: reader["LastName"].ToString(),
          email: reader["Email"].ToString(),
          role: Enum.Parse<UserRole>(reader["Role"].ToString())
        );

        users.Add(user);
      }

      Logger.LogInfo($"Получено {users.Count} пользователей из базы данных.");
      return users;
    }

    public UserModel? GetUserByTelegramChatId(long telegramChatId)
    {
      using var connection = new SQLiteConnection(_connectionString);
      connection.Open();

      string query = "SELECT TelegramChatId, FirstName, LastName, Email, Role FROM Users WHERE TelegramChatId = @TelegramChatId";
      using var command = new SQLiteCommand(query, connection);
      command.Parameters.AddWithValue("@TelegramChatId", telegramChatId);

      using var reader = command.ExecuteReader();
      if (reader.Read())
      {
        return new UserModel(
          telegramChatId: Convert.ToInt64(reader["TelegramChatId"]),
          firstName: reader["FirstName"].ToString(),
          lastName: reader["LastName"].ToString(),
          email: reader["Email"].ToString(),
          role: Enum.Parse<UserRole>(reader["Role"].ToString())
        );
      }

      return null;
    }
  }
}
