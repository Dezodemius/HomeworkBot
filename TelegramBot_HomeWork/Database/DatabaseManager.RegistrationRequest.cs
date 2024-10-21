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
    /// Создает новый запрос на регистрацию.
    /// </summary>
    /// <param name="request">Модель запроса на регистрацию</param>
    public void CreateRegistrationRequest(RegistrationRequest request)
    {
      using var connection = new SQLiteConnection(_connectionString);
      connection.Open();

      string query = @"INSERT INTO RegistrationRequests 
                        (TelegramChatId, FirstName, LastName, Email, CourseId, Status) 
                      VALUES 
                        (@TelegramChatId, @FirstName, @LastName, @Email, @CourseId, @Status)";
      using var command = new SQLiteCommand(query, connection);
      command.Parameters.AddWithValue("@TelegramChatId", request.TelegramChatId);
      command.Parameters.AddWithValue("@FirstName", request.FirstName);
      command.Parameters.AddWithValue("@LastName", request.LastName);
      command.Parameters.AddWithValue("@Email", request.Email);
      command.Parameters.AddWithValue("@CourseId", request.CourseId);
      command.Parameters.AddWithValue("@Status", request.Status);

      command.ExecuteNonQuery();
      Logger.LogInfo("Новый запрос на регистрацию добавлен в базу данных.");
    }

    /// <summary>
    /// Удаляет запрос на регистрацию по его уникальному идентификатору.
    /// </summary>
    /// <param name="id">Уникальный идентификатор запроса</param>
    public void DeleteRegistrationRequest(int id)
    {
      using var connection = new SQLiteConnection(_connectionString);
      connection.Open();

      string query = "DELETE FROM RegistrationRequests WHERE RequestId = @Id";
      using var command = new SQLiteCommand(query, connection);
      command.Parameters.AddWithValue("@Id", id);

      int rowsAffected = command.ExecuteNonQuery();
      if (rowsAffected > 0)
      {
        Logger.LogInfo($"Запрос на регистрацию с Id {id} успешно удален.");
      }
      else
      {
        Logger.LogError($"Запрос на регистрацию с Id {id} не найден.");
      }
    }

    /// <summary>
    /// Обновляет запрос на регистрацию по его уникальному идентификатору.
    /// </summary>
    /// <param name="id">Уникальный идентификатор запроса</param>
    /// <param name="updatedRequest">Обновленная модель запроса на регистрацию</param>
    public void UpdateRegistrationRequest(int id, RegistrationRequest updatedRequest)
    {
      using var connection = new SQLiteConnection(_connectionString);
      connection.Open();

      string query = @"UPDATE RegistrationRequests SET 
                        TelegramChatId = @TelegramChatId, 
                        FirstName = @FirstName, 
                        LastName = @LastName, 
                        Email = @Email, 
                        CourseId = @CourseId, 
                        Status = @Status 
                      WHERE RequestId = @Id";
      using var command = new SQLiteCommand(query, connection);
      command.Parameters.AddWithValue("@TelegramChatId", updatedRequest.TelegramChatId);
      command.Parameters.AddWithValue("@FirstName", updatedRequest.FirstName);
      command.Parameters.AddWithValue("@LastName", updatedRequest.LastName);
      command.Parameters.AddWithValue("@Email", updatedRequest.Email);
      command.Parameters.AddWithValue("@CourseId", updatedRequest.CourseId);
      command.Parameters.AddWithValue("@Status", updatedRequest.Status);
      command.Parameters.AddWithValue("@Id", id);

      int rowsAffected = command.ExecuteNonQuery();
      if (rowsAffected > 0)
      {
        Logger.LogInfo($"Запрос на регистрацию с Id {id} успешно обновлен.");
      }
      else
      {
        Logger.LogError($"Запрос на регистрацию с Id {id} не найден для обновления.");
      }
    }

    /// <summary>
    /// Возвращает все запросы на регистрацию из базы данных.
    /// </summary>
    /// <returns>Список всех запросов на регистрацию</returns>
    public List<RegistrationRequest> GetAllRegistrationRequests()
    {
      using var connection = new SQLiteConnection(_connectionString);
      connection.Open();

      string query = "SELECT RequestId, TelegramChatId, FirstName, LastName, Email, CourseId, Status FROM RegistrationRequests";
      using var command = new SQLiteCommand(query, connection);

      using var reader = command.ExecuteReader();
      var requests = new List<RegistrationRequest>();

      while (reader.Read())
      {
        var request = new RegistrationRequest
        {
          RequestId = Convert.ToInt32(reader["RequestId"]),
          TelegramChatId = Convert.ToInt64(reader["TelegramChatId"]),
          FirstName = reader["FirstName"].ToString(),
          LastName = reader["LastName"].ToString(),
          Email = reader["Email"].ToString(),
          CourseId = Convert.ToInt32(reader["CourseId"]),
          Status = reader["Status"].ToString()
        };

        requests.Add(request);
      }

      Logger.LogInfo($"Получено {requests.Count} запросов на регистрацию из базы данных.");
      return requests;
    }
  }
}
